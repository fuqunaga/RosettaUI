using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RosettaUI
{
    public static class IBinderExtensions
    {

        #region Type Define

        // Genericを無視してメソッドを呼ぶ
        class GenericMethodInvoker
        {
            #region static

            static readonly Dictionary<(Type, string), GenericMethodInvoker> table = new Dictionary<(Type, string), GenericMethodInvoker>();

            public static object Invoke(Type staticClassType, string methodName, Type genericParameter, params object[] args)
            {
                var key = (staticClassType, methodName);
                if (!table.TryGetValue(key, out var invoker))
                {
                    invoker = new GenericMethodInvoker(staticClassType, methodName);
                    table[key] = invoker;
                }

                return invoker.Invoke(genericParameter, args);
            }

            #endregion


            MethodInfo baseInfo;

            public GenericMethodInvoker(Type staticClassType, string methodName)
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                baseInfo = staticClassType.GetMethods(flags).First(mi => (mi.Name == methodName) && mi.IsGenericMethod);
            }


            Dictionary<Type, MethodInfo> infos = new Dictionary<Type, MethodInfo>();

            public object Invoke(Type type, params object[] args)
            {
                if (!infos.TryGetValue(type, out var mi))
                {
                    mi = baseInfo.MakeGenericMethod(type);
                    infos[type] = mi;
                }

                return mi.Invoke(null, args);
            }
        }

        #endregion


        static readonly Dictionary<Type, bool> oneLinderDic = new Dictionary<Type, bool>();

        public static bool IsOneliner(this IBinder binder)
        {
            static bool IsSimpleType(Type t) => t.IsPrimitive || t == typeof(string) || t.IsEnum;

            var type = binder.GetType();
            var valueType = binder.ValueType;

            if (UICustom.HasElementCreationFunc(valueType, out var isOneLiner))
            {
                return isOneLiner;
            }

            if (!oneLinderDic.TryGetValue(type, out var ret))
            {
                if (typeof(IElementCreator).IsAssignableFrom(valueType)
                    || typeof(IList).IsAssignableFrom(valueType)
                    )
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

                    var fieldTypes = TypeUtility.GetSerializableFieldTypes(valueType).ToList();
                    var fieldNames = TypeUtility.GetSerializableFieldNames(valueType);

                    ret = fieldTypes.Count <= oneLinerMaxCount
                        && fieldTypes.All(IsSimpleType)
                        && fieldNames.All((name) => TypeUtility.GetRange(valueType, name) == null);
                }

                oneLinderDic[type] = ret;
            }

            return ret;
        }


#if false

#region Register/Unregister CreateFunc

        public delegate Element CreateFunc(IBinder binder, string label);

        static readonly Dictionary<Type, CreateFunc> createFuncs = new Dictionary<Type, CreateFunc>();



        public class TempCreateFunc : IDisposable
        {
            readonly Type type;
            readonly CreateFunc prevFunc;

            public TempCreateFunc(Type type, CreateFunc createFunc)
            {
                if (type != null)
                {
                    this.type = type;
                    createFuncs.TryGetValue(type, out prevFunc);
                    createFuncs[type] = createFunc;
                }
            }

            public void Dispose()
            {
                if (type != null)
                {
                    if (prevFunc != null)
                    {
                        createFuncs[type] = prevFunc;
                    }
                    else
                    {
                        createFuncs.Remove(type);
                    }
                }
            }
        }



        public static void RegisterCreateFunc<T>(Func<T, string, Element> createFunc)
            where T : class
        {
            createFuncs[typeof(T)] = (obj, label) => createFunc((T)obj, label);
        }

        public static bool UnregisterCreateFunc<T>()
            where T : class
        {
            return createFuncs.Remove(typeof(T));
        }

#endregion

#endif
    }
}