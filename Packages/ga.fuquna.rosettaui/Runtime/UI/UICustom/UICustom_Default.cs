

using System.Collections.Generic;
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
            RegisterPropertyAttributeFunc<HeaderAttribute>(HeaderAttributeFunc);
        }

        
        #region PropertyAttributeFunc
        
        private static Element RangeAttributeFunc(RangeAttribute rangeAttribute, LabelElement label, IBinder binder)
        {
            var (minGetter, maxGetter) = RangeUtility.CreateGetterMinMax(rangeAttribute, binder.ValueType);
            return UI.Slider(label, binder, minGetter, maxGetter);
        }
        
        private static IEnumerable<Element> HeaderAttributeFunc(HeaderAttribute headerAttribute, Element originalElement)
        {
            yield return UI.Space().SetHeight(18f);
            yield return UI.Label($"<b>{headerAttribute.header}</b>");
        }
        
        #endregion
    }
}