using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// 中に任意のコンテンツを含むVisualElement
    /// </summary>
    public class WrapElement : VisualElement
    {
        private const string UssClassName = "rosettaui-wrap-element";
        
        public WrapElement()
        {
            AddToClassList(UssClassName);
        }
    }
}