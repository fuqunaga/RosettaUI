using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_DynamicElement(Element element, VisualElement visualElement)
        {
            // (visualElement is not VisualElement) is not work
            // visualElementがVisualElementを継承したクラスでもtrueになってしまうのでGetType()で厳密にチェックする
            if (element is not DynamicElement dynamicElement || visualElement.GetType() != typeof(VisualElement)) return false;
            
            dynamicElement.GetViewBridge().RegisterBindView(e => Bind_ElementGroupContents(e, visualElement));

            return true;
        }
    }
}