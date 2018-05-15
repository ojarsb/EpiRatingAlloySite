using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace AlloyDemoKit.Api.Models
{
    public class RatingTableDataDto
    {
        [JsonProperty("pageName")]
        public string PageName { get; set; }
        [JsonProperty("rating")]
        public int Rating { get; set; }
        [JsonProperty("comments")]
        public IEnumerable<RatingCommentDto> Comments { get; set; }
        [JsonProperty("shortComments")]
        public IEnumerable<RatingCommentDto> ShortComments { get; set; }
        [JsonProperty("ratingEnabled")]
        public bool RatingEnabled { get; set; }
        [JsonProperty("contentId")]
        public string ContentId { get; set; }
        [JsonProperty("contentUrl")]
        public string ContentUrl { get; set; }
        [JsonProperty("lastCommentDateFormatted")]
        public string LastCommentDateFormatted => LastCommentDate > DateTime.MinValue ? LastCommentDate.ToString(CultureInfo.InvariantCulture) : string.Empty;

        public DateTime LastCommentDate { get; set; }

        [JsonProperty("ratingCount")]
        public int RatingCount { get; set; }
        [JsonProperty("positiveRatingCount")]
        public int PositiveRatingCount { get; set; }
        [JsonProperty("negativeRatingCount")]
        public int NegativeRatingCount { get; set; }
        [JsonProperty("pageFriendlyUrl")]
        public string PageFriendlyUrl { get; set; }
    }
}