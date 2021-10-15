using System;

namespace RosettaUI
{
    public static class PropertyOrFieldMinMaxBinder
    {
        public static IBinder Create(IBinder binder, string propertyOrFieldName)
        {
            var valueType = binder.GetMinMaxValueType();
            var memberType = TypeUtility.GetPropertyOrFieldType(valueType, propertyOrFieldName);
            var binderType = typeof(PropertyOrFieldMinMaxBinder<,>).MakeGenericType(valueType, memberType);

            return (IBinder)Activator.CreateInstance(binderType, binder, propertyOrFieldName);
        }
    }

    public class PropertyOrFieldMinMaxBinder<TParent, TValue> : ChildBinderBase<MinMax<TParent>, MinMax<TValue>>
    {
        private readonly Func<TParent, TValue> _childGetter;
        private readonly Func<TParent, TValue, TParent> _childSetter;

        public PropertyOrFieldMinMaxBinder(IBinder<MinMax<TParent>> parentBinder, string propertyOrFieldName)
            : base(parentBinder)
        {
            (_childGetter, _childSetter) =
                PropertyOrFieldGetterSetter<TParent, TValue>.GetGetterSetter(propertyOrFieldName);
        }

        protected override MinMax<TValue> GetFromChild(MinMax<TParent> parent)
        {
            var (min, max) = parent;
            return MinMax.Create(_childGetter(min), _childGetter(max));
        }

        protected override MinMax<TParent> SetToParent(MinMax<TParent> parent, MinMax<TValue> value)
        {
            var (min, max) = parent;
            var (valueMin, valueMax) = value;
            min = _childSetter(min, valueMin);
            max = _childSetter(max, valueMax);
            return MinMax.Create(min, max);
        }
    }
}