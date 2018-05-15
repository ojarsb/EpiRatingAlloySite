using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Http;
using AlloyDemoKit.Api.Models;
using AlloyDemoKit.Models;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.Editor;
using EPiServer.Filters;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Geta.EPi.Rating.Core;
using Geta.EPi.Rating.Core.Models;
using NuGet;
using ILogger = EPiServer.Logging.ILogger;

namespace AlloyDemoKit.Api.Controllers
{
    [RoutePrefix("api/rating")]
    public class PageRatingController : ApiController
    {
        private readonly IContentLoader _loader;
        private readonly ILogger _logger = LogManager.GetLogger();
        private readonly IContentRepository _repository;
        private readonly IReviewService _reviewService;

        private readonly UrlResolver _urlResolver;

        //private readonly ContentAssetHelper _contentAssetHelper;
        private ContentAssetHelper contentAssetHelper;

        public PageRatingController()
        {
            _reviewService = ServiceLocator.Current.GetInstance<IReviewService>();
            _loader = ServiceLocator.Current.GetInstance<IContentLoader>();
            contentAssetHelper = ServiceLocator.Current.GetInstance<ContentAssetHelper>();
            _repository = ServiceLocator.Current.GetInstance<IContentRepository>();
            _urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
            //_logger = logger;
        }

        [Route("ratepage")]
        [HttpPost]
        public void RatePage(RatingDto ratingData)
        {
            ContentReference reviewPageReference;
            if (ContentReference.TryParse(ratingData.ContentId, out reviewPageReference))
            {
                var review = new ReviewModel
                {
                    Rating = ratingData.Rating ? 1 : -1,
                    ReviewText = ratingData.Comment,
                    ReviewOwnerContentLink = reviewPageReference
                };

                _reviewService.Create(review);

                AddCookie(ratingData.ContentId);

                SendNotificationEmail();
            }
            else
            {
                _logger.Log(Level.Error, $"Error parsing content reference {ratingData.ContentId}");
            }
        }

        [Route("getratings")]
        [HttpGet]
        public RatingListDto GetRatings([FromUri] RatingFilterDto filterParams)
        {
            var ratingDataList = new RatingListDto();
            var filter = new FilterContentForVisitor();
            var pages = GetChildPages(ContentReference.StartPage).ToList();
            filter.Filter(pages);
            var ratingTableDataList = new List<RatingTableDataDto>();

            var ratingInterfacePages = filterParams != null && filterParams.RatingEnabled
                ? pages.OfType<IRatingPage>().Where(p => p.RatingEnabled == filterParams.RatingEnabled).ToList()
                : pages.OfType<IRatingPage>().ToList();


            //var reviews = GetReviews(ratingInterfacePages);


            foreach (var ratingPage in ratingInterfacePages)
            {
                var ratingContent = (IContent) ratingPage;
                var ratings = _reviewService.GetReviews(ratingContent.ContentLink);

                if (ratings == null)
                {
                    continue;
                }

                var ratingsList = ratings.ToList();

                var ratingTableData = new RatingTableDataDto
                {
                    PageName = ratingContent.Name,
                    RatingEnabled = ratingPage.RatingEnabled,
                    ContentId = ratingContent.ContentLink.ID.ToString(),
                    ContentUrl = PageEditing.GetEditUrl(ratingContent.ContentLink),
                    PageFriendlyUrl = _urlResolver.GetUrl(ratingContent.ContentLink)
                };

                if (ratingsList.Any())
                {
                    if (filterParams != null)
                    {
                        ratingsList = ratingsList.Where(r =>
                                !filterParams.DateFrom.HasValue || r.Created.Date >= filterParams.DateFrom.Value.Date &&
                                (!filterParams.DateTo.HasValue || r.Created.Date <= filterParams.DateTo.Value.Date))
                            .ToList();
                    }

                    var allCommentsDto = ratingsList.Where(r => !string.IsNullOrEmpty(r.Text))
                        .Select(r => new RatingCommentDto {CommentText = r.Text, CommentDate = r.Created}).ToList();

                    ratingTableData.ShortComments = new List<RatingCommentDto>(allCommentsDto
                        .OrderByDescending(c => c.CommentDate).Take(5)
                        .Select(c => new RatingCommentDto
                        {
                            CommentText =
                                c.CommentText.Length > 500 ? c.CommentText.Substring(0, 500) + "..." : c.CommentText,
                            CommentDate = c.CommentDate
                        }));

                    ratingTableData.Comments = new List<RatingCommentDto>(allCommentsDto.Select(comment =>
                        new RatingCommentDto
                        {
                            CommentText = comment.CommentText + "{nl}",
                            CommentDate = comment.CommentDate
                        }));

                    ratingTableData.Rating = (int) ratingsList.Select(r => r.Rating).Sum();
                    ratingTableData.LastCommentDate =
                        allCommentsDto.OrderByDescending(c => c.CommentDate).First().CommentDate;
                    ratingTableData.RatingCount = ratingsList.Count;
                    ratingTableData.PositiveRatingCount = ratingsList.Count(r => r.Rating > 0);
                    ratingTableData.NegativeRatingCount = ratingsList.Count(r => r.Rating < 0);
                }

                if (filterParams == null || !filterParams.OnlyRatedPages ||
                    (filterParams.OnlyRatedPages && ratingsList.Any()))
                {
                    ratingTableDataList.Add(ratingTableData);
                }
            }

            ratingDataList.RatingData = ratingTableDataList;

            return ratingDataList;
        }


        [Route("getpagecomments")]
        [HttpGet]
        public RatingListDto GetPageComments([FromUri] RatingFilterDto filterParams)
        {
            var ratingTableData = new RatingTableDataDto();

            ContentReference contentRef;

            if (ContentReference.TryParse(filterParams.ContentId, out contentRef))
            {
                var ratingPageContent = _loader.Get<IContent>(contentRef);
                var ratings = GetReviews(contentRef);

                if (ratings != null)
                {
                    ratingTableData.PageName = ratingPageContent.Name;
                    ratingTableData.ContentId = ratingPageContent.ContentLink.ID.ToString();
                    ratingTableData.Comments =
                        ratings.Where(r => !string.IsNullOrEmpty(r.Text)).OrderByDescending(r => r.Created).Select(r =>
                            new RatingCommentDto {CommentText = r.Text, CommentDate = r.Created});
                }
            }

            return new RatingListDto {RatingData = new List<RatingTableDataDto> {ratingTableData}};
        }


        public IEnumerable<Review> GetReviews(IEnumerable<IRatingPage> contentReferences)
        {
            var crs = contentReferences.OfType<PageData>().Select(x => x.ContentLink);

            var assetFolders = new List<ContentAssetFolder>();

            foreach (var r in crs)
            {
                assetFolders.Add(contentAssetHelper.GetAssetFolder(r));
            }

            //var items = _loader.GetItems(assetFolders.Select(x => x.ContentLink), CultureInfo.CurrentUICulture);

            var items = new List<Review>();

            foreach (var item in assetFolders)
            {
                items.AddRange(_loader.GetChildren<Review>(item.ContentLink, CultureInfo.CurrentUICulture));
            }

            return items;
        }


        public IEnumerable<Review> GetReviews(ContentReference contentReference)
        {
            var assetFolder = contentAssetHelper.GetAssetFolder(contentReference);

            if (assetFolder == null)
            {
                return null;
            }

            return _loader.GetChildren<Review>(assetFolder.ContentLink).OrderByDescending(m => m.StartPublish);
        }


        [Route("enablerating")]
        [HttpPost]
        public void EnableRating(RatingDto actionInfo)
        {
            ContentReference reviewPageReference;

            if (ContentReference.TryParse(actionInfo.ContentId, out reviewPageReference))
            {
                var page = _loader.Get<PageData>(reviewPageReference);
                var writablePage = page.CreateWritableClone();

                if (writablePage is IRatingPage ratingPage)
                {
                    ratingPage.RatingEnabled = actionInfo.RatingEnabled;
                }

                _repository.Save(writablePage, SaveAction.Publish);
            }
        }

        [Route("pageispublished")]
        [HttpGet]
        public ResponseDto PageIsPublished(RatingDto actionInfo)
        {
            var response = new ResponseDto();

            if (ContentReference.TryParse(actionInfo.ContentId, out var reviewPageReference))
            {
                var page = _loader.Get<PageData>(reviewPageReference);

                response.PageIsPublished = page.Status == VersionStatus.Published;
            }

            return response;
        }

        private IEnumerable<IContent> GetChildPages(ContentReference levelRootLink, IList<IContent> pages = null)
        {
            if (pages == null)
            {
                pages = new List<IContent>();
            }

            var children = _loader.GetChildren<IContent>(levelRootLink).ToList();

            if (children.Any())
            {
                pages.AddRange(children);
            }

            foreach (var levelItems in children)
            {
                GetChildPages(levelItems.ContentLink, pages);
            }

            return pages;
        }

        private void SendNotificationEmail()
        {
            _logger.Log(Level.Information, "Notification sent successfully");
        }

        private void AddCookie(string contentId)
        {
            if (HttpContext.Current == null)
            {
                return;
            }

            var ratingCookie = HttpContext.Current.Request.Cookies["Ratings"] ??
                               new HttpCookie("Ratings") {HttpOnly = false};
            ratingCookie.Expires = DateTime.Now.AddYears(1);
            var cookieSubkeyName = $"c_{contentId}";
            var cookieSubkeyValue = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            ratingCookie.Values.Remove(cookieSubkeyName);
            ratingCookie.Values.Add(cookieSubkeyName, cookieSubkeyValue);
            HttpContext.Current.Response.Cookies.Add(ratingCookie);
        }
    }
}