using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI
{
    /// <summary>
    /// UICustomのデフォルト設定登録
    /// </summary>
    public static partial class UICustom
    {
        static UICustom()
        {
            RegisterDefaultPropertyAttributeFunc();
        }

        private static void RegisterDefaultPropertyAttributeFunc()
        {
            RegisterPropertyAttributeFunc<RangeAttribute>(RangeAttributeFunc);
            RegisterPropertyAttributeCreateTopElementFunc<HeaderAttribute>(HeaderAttributeFunc);
            RegisterPropertyAttributeCreateTopElementFunc<SpaceAttribute>(SpaceAttributeFunc);
            RegisterPropertyAttributeFunc<MultilineAttribute>(MultilineAttributeFunc);
        }
        

        #region PropertyAttributeFunc
        
        private static Element RangeAttributeFunc(RangeAttribute rangeAttribute, LabelElement label, IBinder binder)
        {
            var (minGetter, maxGetter) = RangeUtility.CreateGetterMinMax(rangeAttribute, binder.ValueType);
            return UI.Slider(label, binder, minGetter, maxGetter);
        }
        
        private static IEnumerable<Element> HeaderAttributeFunc(HeaderAttribute headerAttribute, Element _)
        {
            yield return UI.Space().SetHeight(18f);
            yield return UI.Label($"<b>{headerAttribute.header}</b>");
        }
        
        
        private static IEnumerable<Element> SpaceAttributeFunc(SpaceAttribute spaceAttribute, Element _)
        {
            yield return UI.Space().SetHeight(spaceAttribute.height);
        }

        private static Element MultilineAttributeFunc(MultilineAttribute _, Element originalElement)
        {
            var textField = originalElement.Query<TextFieldElement>().FirstOrDefault();
            if (textField != null)
            {
                textField.IsMultiLine = true;
            }

            return originalElement;
        }
        
        #endregion
    }
}