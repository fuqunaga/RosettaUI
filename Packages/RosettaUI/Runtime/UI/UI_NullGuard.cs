using System;

namespace RosettaUI
{
    public static partial class UI
    {
        private static readonly BinderBase<string> NullStrBinder = ConstBinder.Create("null");

        public static Element NullGuardIfNeed(LabelElement label, IGetter getter, Func<Element> createElement)
        {
            return (getter.IsNullable && getter.ValueType != typeof(string))
                ? NullGuard(label, getter, createElement)
                : createElement();
        }

        public static Element NullGuard(LabelElement label, IGetter getter, Func<Element> createElement)
        {
            return DynamicElement.Create(
                () => getter.IsNull,
                isNull => isNull
                    ? new TextFieldElement(label, NullStrBinder, null).SetInteractable(false)
                    : createElement(),
                $"NullGuard({nameof(DynamicElement)})"
            );
        }
    }
}