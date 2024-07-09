using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private bool Bind_DynamicElement(Element element, VisualElement visualElement)
        {
            // (visualElement is not WrapElement) is not work
            // visualElement が WrapElement を継承したクラスでもtrueになってしまうのでGetType()で厳密にチェックする
            if (element is not DynamicElement dynamicElement || visualElement.GetType() != typeof(WrapElement)) return false;
            
            dynamicElement.GetViewBridge().RegisterBindViewAndCallOnce(e =>
            {
                Bind_ElementGroupContents(e, visualElement);
                RequestResizeWindowEvent.Send(visualElement);
            });

            return true;
        }
    }
}