

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
        }

        private static Element RangeAttributeFunc(RangeAttribute rangeAttribute, LabelElement label, IBinder binder)
        {
            var (minGetter, maxGetter) = RangeUtility.CreateGetterMinMax(rangeAttribute, binder.ValueType);
            return UI.Slider(label, binder, minGetter, maxGetter);
        }
    }
}