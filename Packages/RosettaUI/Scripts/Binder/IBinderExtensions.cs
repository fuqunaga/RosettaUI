using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RosettaUI
{
    public static class BinderExtensions
    {
        #if false
        #region Type Define

        // Genericを無視してメソッドを呼ぶ
        private class GenericMethodInvoker
        {
            private readonly MethodInfo _baseInfo;


            private readonly Dictionary<Type, MethodInfo> _infos = new Dictionary<Type, MethodInfo>();

            public GenericMethodInvoker(Type staticClassType, string methodName)
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                _baseInfo = staticClassType.GetMethods(flags).First(mi => mi.Name == methodName && mi.IsGenericMethod);
            }

            public object Invoke(Type type, params object[] args)
            {
                if (!_infos.TryGetValue(type, out var mi))
                {
                    mi = _baseInfo.MakeGenericMethod(type);
                    _infos[type] = mi;
                }

                return mi.Invoke(null, args);
            }

            #region static

            private static readonly Dictionary<(Type, string), GenericMethodInvoker> Table = new Dictionary<(Type, string), GenericMethodInvoker>();

            public static object Invoke(Type staticClassType, string methodName, Type genericParameter, params object[] args)
            {
                var key = (staticClassType, methodName);
                if (!Table.TryGetValue(key, out var invoker))
                {
                    invoker = new GenericMethodInvoker(staticClassType, methodName);
                    Table[key] = invoker;
                }

                return invoker.Invoke(genericParameter, args);
            }

            #endregion
        }

        #endregion
#endif

        private static readonly Dictionary<Type, bool> OneLinerDic = new Dictionary<Type, bool>();

        public static bool IsOneliner(this IBinder binder)
        {
            static bool IsSimpleType(Type t)
            {
                return t.IsPrimitive || t == typeof(string) || t.IsEnum;
            }

            var type = binder.GetType();
            var valueType = binder.ValueType;

            if (UICustom.HasElementCreationFunc(valueType, out var isOneLiner)) return isOneLiner;

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

                    var fieldTypes = TypeUtility.GetUITargetFieldTypes(valueType).ToList();
                    var fieldNames = TypeUtility.GetUITargetFieldNames(valueType);

                    ret = fieldTypes.Count <= oneLinerMaxCount
                          && fieldTypes.All(IsSimpleType)
                          && fieldNames.All(name => TypeUtility.GetRange(valueType, name) == null);
                }

                OneLinerDic[type] = ret;
            }

            return ret;
        }
    }
}