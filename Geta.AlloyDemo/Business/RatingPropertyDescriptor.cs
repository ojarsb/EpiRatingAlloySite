using EPiServer.Shell.ObjectEditing.EditorDescriptors;

namespace AlloyDemoKit.Business
{
    [EditorDescriptorRegistration(
      TargetType = typeof(string),
      UIHint = "RatingProperty")]
    public class RatingPropertyDescriptor : EditorDescriptor
    {
        public RatingPropertyDescriptor()
        {
            ClientEditingClass = "ratingModule/RatingProperty";
        }
    }
}