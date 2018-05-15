using Newtonsoft.Json;

namespace AlloyDemoKit.Api.Models
{
    public class RatingDto
    {
        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("rating")]
        public bool Rating { get; set; }

        [JsonProperty("contentId")]
        public string ContentId { get; set; }

        [JsonProperty("ratingEnabled")]
        public bool RatingEnabled { get; set; }

    }
}