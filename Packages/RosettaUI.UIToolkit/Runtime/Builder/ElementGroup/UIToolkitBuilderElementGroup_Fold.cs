using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_Fold(Element element)
        {
            var fold = new FoldoutCustom();
            SetupOpenCloseBaseElement(fold, (FoldElement) element);
   
            var ret =  Build_ElementGroupContents(fold, element);
            return ret;
        }

    }
}