//#define ENABLE_IL2CPP

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace RosettaUI
{
    public static class PropertyOrFieldGetterSetter<TParent, TValue>
    {
        static readonly Dictionary<string, (Func<TParent, TValue>, Func<TParent, TValue, TParent>)> Table = new Dictionary<string, (Func<TParent, TValue>, Func<TParent, TValue, TParent>)>();


        public static (Func<TParent, TValue>, Func<TParent, TValue, TParent>) GetGetterSetter(string propertyOrFieldName)
        {
            if (!Table.TryGetValue(propertyOrFieldName, out var pair))
            {
#if ENABLE_IL2CPP
                var getter = IL2CPP.PropertyOrFieldGetterSetter_IL2CPP<TParent, TValue>.CreateGetter(propertyOrFieldName);
                var setter = IL2CPP.PropertyOrFieldGetterSetter_IL2CPP<TParent, TValue>.CreateSetter(propertyOrFieldName);
#else
                var getter = CreateGetter(propertyOrFieldName);
                var setter = CreateSetter(propertyOrFieldName);
#endif

                Table[propertyOrFieldName] = pair = (getter, setter);
            }

            return pair;
        }

        // Expression.PropertyOrField is not use because it is not case sensitive
        static MemberExpression PropertyOrField(Expression expression, string propertyOrFieldName)
        {
            var memberInfo = TypeUtility.GetMemberInfo(expression.Type, propertyOrFieldName);

            return memberInfo switch
            {
                FieldInfo fi => Expression.Field(expression, fi),
                PropertyInfo pi => Expression.Property(expression, pi),
                _ => throw new ArgumentException($"{expression.Type} does not have a member '{propertyOrFieldName}'", nameof(propertyOrFieldName))
            };
        }
        
        // Getter
        // Base Code:
        // return obj.propertyOrFieldName;
        static Func<TParent, TValue> CreateGetter(string propertyOrFieldName)
        {
            var objParam = Expression.Parameter(typeof(TParent));
            return Expression.Lambda<Func<TParent, TValue>>(PropertyOrField(objParam, propertyOrFieldName), objParam).Compile();
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

            BlockExpression block = null;
            try
            {
                block = Expression.Block(
                    typeof(TParent),
                    Expression.Assign(PropertyOrField(objParam, propertyOrFieldName), setterValueParam),
                    Expression.Return(returnTarget, objParam),
                    Expression.Label(returnTarget, Expression.Default(typeof(TParent)))
                );
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            return Expression.Lambda<Func<TParent, TValue, TParent>>(block, objParam, setterValueParam).Compile();
        }
    }
}