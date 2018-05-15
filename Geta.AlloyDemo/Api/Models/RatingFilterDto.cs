using System;
using Newtonsoft.Json;

namespace AlloyDemoKit.Api.Models
{
    public class RatingFilterDto
    {
        [JsonProperty("dateFrom")]
        public DateTime? DateFrom { get; set; }
        [JsonProperty("dateTo")]
        public DateTime? DateTo { get; set; }
        [JsonProperty("contentId")]
        public string ContentId { get; set; }
        [JsonProperty("ratingEnabled")]
        public bool RatingEnabled { get; set; }
        [JsonProperty("onlyRatedPages")]
        public bool OnlyRatedPages { get; set; }
    }
}