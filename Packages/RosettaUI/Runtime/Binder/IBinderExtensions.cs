using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public static class BinderExtensions
    {
        private static readonly Dictionary<Type, bool> OneLinerDic = new Dictionary<Type, bool>();

        public static bool IsOneliner(this IBinder binder)
        {
            static bool IsSimpleType(Type t)
            {
                return t.IsPrimitive || t == typeof(string) || t.IsEnum;
            }

            var type = binder.GetType();
            var valueType = binder.ValueType;

            if (UICustom.GetElementCreationMethod(valueType) is { } creationFunc) return creationFunc.isOneLiner;

            if (!OneLinerDic.TryGetValue(type, out var ret))
            {
                if (typeof(IElementCreator).IsAssignableFrom(valueType) || typeof(IList).IsAssignableFrom(valueType))
                {
                    ret = false;
                }
                else if (IsSimpleType(valueType))
                {
                    ret = true;
                }
                else
                {
                    const int oneLinerMaxCount = 3;
                    const int nameMaxLength = 3;

                    var fieldTypes = TypeUtility.GetUITargetFieldTypes(valueType).ToList();
                    var fieldNames = TypeUtility.GetUITargetFieldNames(valueType);

                    ret = fieldTypes.Count <= oneLinerMaxCount
                          && fieldTypes.All(IsSimpleType)
                          && fieldNames.All(name =>
                          {
                              return name.Length <= nameMaxLength
                                     && TypeUtility.GetRange(valueType, name) == null;
                          });
                }

                OneLinerDic[type] = ret;
            }

            return ret;
        }
    }
}