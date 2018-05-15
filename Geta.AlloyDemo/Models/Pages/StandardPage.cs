using System;
using System.ComponentModel.DataAnnotations;
using AlloyDemoKit.Models.Blocks;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.Shell.ObjectEditing;

namespace AlloyDemoKit.Models.Pages
{
    /// <summary>
    /// Used for the pages mainly consisting of manually created content such as text, images, and blocks
    /// </summary>
    [SiteContentType(GUID = "9CCC8A41-5C8C-4BE0-8E73-520FF3DE8267")]
    [SiteImageUrl(Global.StaticGraphicsFolderPath + "page-type-thumbnail-standard.png")]
    public class StandardPage : SitePageData, IRatingPage
    {
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 310)]
        [CultureSpecific]
        public virtual XhtmlString MainBody { get; set; }

        [Display(
            GroupName = SystemTabNames.Content,
            Order = 320)]
        public virtual ContentArea MainContentArea { get; set; }

        public virtual bool RatingEnabled { get; set; }
        public virtual bool IgnorePublish { get; set; }
        public virtual string RatingQuestion { get; set; }

        [Display(Name = "", GroupName = "Rating", Order = 600)]
        [UIHint("RatingProperty")]
        public virtual string RatingData { get; set; }

    }
}
