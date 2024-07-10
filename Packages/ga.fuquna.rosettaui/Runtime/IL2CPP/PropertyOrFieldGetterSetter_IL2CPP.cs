using System;
using System.Linq;

namespace RosettaUI.IL2CPP
{
    public static class PropertyOrFieldGetterSetter_IL2CPP<TParent, TValue>
    {
        // Getter
        // Base Code:
        // return obj.propertyOrFieldName;
        public static Func<TParent, TValue> CreateGetter(string propertyOrFieldName)
        {
            var fi = typeof(TParent).GetFields().FirstOrDefault(info => info.Name == propertyOrFieldName && info.FieldType == typeof(TValue));
            if (fi != null)
            {
                return (parent) => (TValue)fi.GetValue(parent);
            }

            var pi = typeof(TParent).GetProperties().FirstOrDefault(info => info.Name == propertyOrFieldName && info.PropertyType == typeof(TValue));
            if (pi != null)
            {
                return (parent) => (TValue)pi.GetValue(parent);
            }

            return null;
        }



        // Setter
        // Base Code:
        // obj.propertyOrFieldName = setterParam;
        // return obj;
        public static Func<TParent, TValue, TParent> CreateSetter(string propertyOrFieldName)
        {
            var fi = typeof(TParent).GetFields().FirstOrDefault(info => info.Name == propertyOrFieldName && info.FieldType == typeof(TValue));
            if (fi != null)
            {
                return (parent, value) =>
                {
                    var boxed = (object)parent;
                    fi.SetValue(boxed, value);
                    return (TParent)boxed;
                };
            }

            var pi = typeof(TParent).GetProperties().FirstOrDefault(info => info.Name == propertyOrFieldName && info.PropertyType == typeof(TValue));
            if (pi != null)
            {
                return (parent, value) =>
                {
                    var boxed = (object)parent;
                    pi.SetValue(boxed, value);
                    return (TParent)boxed;
                };
            }

            return null;
        }
    }
}