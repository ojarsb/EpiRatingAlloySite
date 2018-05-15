using System.Collections.Generic;
using Newtonsoft.Json;

namespace AlloyDemoKit.Api.Models
{
    public class RatingListDto
    {
        [JsonProperty("ratingData")]
        public IEnumerable<RatingTableDataDto> RatingData { get; set; }
    }
}