using System;

namespace Comugi
{
    public static class PropertyOrFieldMinMaxGetter
    {
        public static IMinMaxGetter Create(IMinMaxGetter minMaxGetter, string propertyOrFieldName)
        {
            var valueType = minMaxGetter.MinMaxType;
            var memberType = TypeUtility.GetPropertyOrFieldType(valueType, propertyOrFieldName);
            var getterType = typeof(PropertyOrFieldMinMaxGetter<,>).MakeGenericType(valueType, memberType);

            return Activator.CreateInstance(getterType, minMaxGetter, propertyOrFieldName) as IMinMaxGetter;
        }
    }


    public class PropertyOrFieldMinMaxGetter<TParent, TValue> : MinMaxGetter<TValue>
    {
        readonly IMinMaxGetter<TParent> parentGetter;

        static Func<(TValue,TValue)> CreateChildGetter(IMinMaxGetter<TParent> parentGetter, string propertyOrFieldName)
        {
            var (childGetter, _) = PropertyOrFieldGetterSetter<TParent, TValue>.GetGetterSetter(propertyOrFieldName);


            return () =>
            {
                var (min, max) = parentGetter.Get();
                return (childGetter(min), childGetter(max));
            };
        }

        public PropertyOrFieldMinMaxGetter(IMinMaxGetter<TParent> parentGetter, string propertyOrFieldName) : base(CreateChildGetter(parentGetter, propertyOrFieldName))
        {
            this.parentGetter = parentGetter;
        }

        public override bool IsConst => parentGetter.IsConst;
    }
}