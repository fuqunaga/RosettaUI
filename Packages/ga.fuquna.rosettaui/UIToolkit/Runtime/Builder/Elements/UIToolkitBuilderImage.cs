using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private static bool Bind_Image(Element element, VisualElement visualElement)
        {
            if (element is not ImageElement imageElement || visualElement is not Image image) return false;
            imageElement.GetViewBridge().SubscribeValueOnUpdateCallOnce(tex => image.image = tex);

            return true;
        }
    }
}