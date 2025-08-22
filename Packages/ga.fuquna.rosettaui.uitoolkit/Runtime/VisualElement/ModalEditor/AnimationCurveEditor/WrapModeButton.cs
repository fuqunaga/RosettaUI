using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class WrapModeButton : VisualElement
    {
        public enum PreOrPost
        {
            Pre,
            Post
        }
        
        private const string UssClassName = "rosettaui-animation-curve-editor__wrap-mode-button";
        private const string UssClassNamePre = UssClassName + "--pre";
        private const string UssClassNamePost = UssClassName + "--post";

        public PreOrPost CurrentPreOrPost => ClassListContains(UssClassNamePre) ? PreOrPost.Pre : PreOrPost.Post;
        
        public WrapModeButton() {
            AddToClassList(UssClassName);
            Hide();
        }
        
        public void Show(PreOrPost preOrPost)
        {
            EnableInClassList(UssClassNamePre, preOrPost == PreOrPost.Pre);
            EnableInClassList(UssClassNamePost, preOrPost == PreOrPost.Post);
            
            style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            style.display = DisplayStyle.None;
        }
    }
}