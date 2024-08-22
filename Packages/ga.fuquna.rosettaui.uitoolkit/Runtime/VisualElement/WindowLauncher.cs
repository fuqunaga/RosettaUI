using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class WindowLauncher : Toggle
    {
        private const string UssClassName = "rosettaui-window-launcher";
        private const string CheckmarkUssClassName = UssClassName + "__checkmark";
        
        public WindowLauncher()
        {
            AddToClassList(UssClassName);
            
            // unityのussの影響を避けるためにクラス名を変更する
            var checkmarkElement = this.Q("unity-checkmark");
            checkmarkElement.RemoveFromClassList(checkmarkUssClassName);
            checkmarkElement.AddToClassList(UssClassName + "__checkmark");
        }
    }
}