namespace AlloyDemoKit.Models
{
    public interface IRatingPage
    {
        bool RatingEnabled { get; set; }

        bool IgnorePublish { get; set; }

        string RatingQuestion { get; set; }
    }
}
