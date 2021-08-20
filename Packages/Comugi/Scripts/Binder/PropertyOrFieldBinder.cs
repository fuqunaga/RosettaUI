using System;


namespace Comugi
{
    // 型とフィールドかプロパティ名から生成されるBinder
    public static class PropertyOrFieldBinder
    {
        public static IBinder Create<T>(T obj, string propertyOrFieldName) => CreateWithBinder(ConstBinder.Create(obj), propertyOrFieldName);

        public static IBinder CreateWithBinder(IBinder binder, string propertyOrFieldName)
        {
            var parent = binder.ValueType;
            var memberType = TypeUtility.GetPropertyOrFieldType(parent, propertyOrFieldName);
            var binderType = typeof(PropertyOrFieldBinder<,>).MakeGenericType(parent, memberType);

            return Activator.CreateInstance(binderType, binder, propertyOrFieldName) as IBinder;
        }
    }

    public class PropertyOrFieldBinder<TParent, TValue> : ChildBinder<TParent, TValue>
    {
        public PropertyOrFieldBinder(BinderBase<TParent> parentBinder, string propertyOrFieldName) : base(parentBinder, PropertyOrFieldGetterSetter<TParent, TValue>.GetGetterSetter(propertyOrFieldName))
        {
        }
    }
}