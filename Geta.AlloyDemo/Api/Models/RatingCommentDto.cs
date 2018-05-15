using System;
using System.Globalization;
using Newtonsoft.Json;

namespace AlloyDemoKit.Api.Models
{
    public class RatingCommentDto
    {
        [JsonProperty("commentText")]
        public string CommentText { get; set; }

        [JsonProperty("commentDate")]
        public DateTime CommentDate { get; set; }

        [JsonProperty("commentDateFormated")]
        public string CommentDateFormated => CommentDate.ToString(CultureInfo.InvariantCulture);
    }
}