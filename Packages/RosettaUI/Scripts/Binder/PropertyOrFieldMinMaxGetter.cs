using System;

namespace RosettaUI
{
    public static class PropertyOrFieldMinMaxGetter
    {
        public static IGetter Create(IGetter minMaxGetter, string propertyOrFieldName)
        {
            var valueType = minMaxGetter.GetMinMaxValueType();
            var memberType = TypeUtility.GetPropertyOrFieldType(valueType, propertyOrFieldName);
            var getterType = typeof(PropertyOrFieldMinMaxGetter<,>).MakeGenericType(valueType, memberType);

            return (IGetter) Activator.CreateInstance(getterType, minMaxGetter, propertyOrFieldName);
        }
    }


    public class PropertyOrFieldMinMaxGetter<TParent, TValue> : Getter<MinMax<TValue>>
    {
        private readonly IGetter<MinMax<TParent>> _parentGetter;

        public PropertyOrFieldMinMaxGetter(IGetter<MinMax<TParent>> parentGetter, string propertyOrFieldName)
            : base(CreateChildGetter(parentGetter, propertyOrFieldName))
        {
            _parentGetter = parentGetter;
        }

        public override bool IsConst => _parentGetter.IsConst;

        private static Func<MinMax<TValue>> CreateChildGetter(IGetter<MinMax<TParent>> parentGetter,
            string propertyOrFieldName)
        {
            var (childGetter, _) = PropertyOrFieldGetterSetter<TParent, TValue>.GetGetterSetter(propertyOrFieldName);

            return () =>
            {
                var (min, max) = parentGetter.Get();
                return MinMax.Create(childGetter(min), childGetter(max));
            };
        }
    }
}