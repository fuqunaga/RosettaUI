using System;

namespace RosettaUI
{
    /// <summary>
    ///     型とフィールドかプロパティ名から生成されるGetter
    /// </summary>
    public static class PropertyOrFieldGetter
    {
        public static IGetter Create(IGetter getter, string propertyOrFieldName)
        {
            if (getter == null) return null;
            
            var parentType = getter.ValueType;
            var memberType = TypeUtility.GetPropertyOrFieldType(parentType, propertyOrFieldName);
            var getterType = typeof(PropertyOrFieldGetter<,>).MakeGenericType(parentType, memberType);

            return Activator.CreateInstance(getterType, getter, propertyOrFieldName) as IGetter;
        }
    }

    public class PropertyOrFieldGetter<TParent, TValue> : ChildGetterBase<TParent, TValue>
    {
        private readonly Func<TParent, TValue> _getFunc;

        public PropertyOrFieldGetter(IGetter<TParent> parentGetter, string propertyOrFieldName)
            : base(parentGetter)
        {
            (_getFunc, _) = PropertyOrFieldGetterSetter<TParent, TValue>.GetGetterSetter(propertyOrFieldName);
        }

        protected override TValue GetFromChild(TParent parent)
        {
            return _getFunc(parent);
        }
    }
}