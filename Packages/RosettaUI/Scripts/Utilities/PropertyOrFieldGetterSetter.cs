//#define ENABLE_IL2CPP

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static class PropertyOrFieldGetterSetter<TParent, TValue>
    {
        static readonly Dictionary<string, (Func<TParent, TValue>, Func<TParent, TValue, TParent>)> table = new Dictionary<string, (Func<TParent, TValue>, Func<TParent, TValue, TParent>)>();


        public static (Func<TParent, TValue>, Func<TParent, TValue, TParent>) GetGetterSetter(string propertyOrFieldName)
        {
            if (!table.TryGetValue(propertyOrFieldName, out var pair))
            {
#if ENABLE_IL2CPP
                var getter = IL2CPP.PropertyOrFieldGetterSetter_IL2CPP<TParent, TValue>.CreateGetter(propertyOrFieldName);
                var setter = IL2CPP.PropertyOrFieldGetterSetter_IL2CPP<TParent, TValue>.CreateSetter(propertyOrFieldName);
#else
                var getter = CreateGetter(propertyOrFieldName);
                var setter = CreateSetter(propertyOrFieldName);
#endif

                table[propertyOrFieldName] = pair = (getter, setter);
            }

            return pair;
        }

        // Getter
        // Base Code:
        // return obj.propertyOrFieldName;
        static Func<TParent, TValue> CreateGetter(string propertyOrFieldName)
        {
            var objParam = Expression.Parameter(typeof(TParent));
            return Expression.Lambda<Func<TParent, TValue>>(Expression.PropertyOrField(objParam, propertyOrFieldName), objParam).Compile();
        }



        // Setter
        // Base Code:
        // obj.propertyOrFieldName = setterParam;
        // return obj;
        static Func<TParent, TValue, TParent> CreateSetter(string propertyOrFieldName)
        {
            var objParam = Expression.Parameter(typeof(TParent));
            var setterValueParam = Expression.Parameter(typeof(TValue));
            var returnTarget = Expression.Label(typeof(TParent));

            var block = Expression.Block(
                typeof(TParent),
                Expression.Assign(Expression.PropertyOrField(objParam, propertyOrFieldName), setterValueParam),
                Expression.Return(returnTarget, objParam),
                Expression.Label(returnTarget, Expression.Default(typeof(TParent)))
            );

            return Expression.Lambda<Func<TParent, TValue, TParent>>(block, objParam, setterValueParam).Compile();
        }
    }
}