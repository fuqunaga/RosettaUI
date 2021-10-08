using System;
using System.Collections.Generic;

namespace RosettaUI
{
    public static class UICustom
    {
        private static readonly Dictionary<Type, CreationFunc> CreationFuncTable = new Dictionary<Type, CreationFunc>();
        
        public static void RegisterElementCreationFunc<T>(Func<T, Element> creationFunc, bool isOneLiner = false)
        {
            var cf = new CreationFunc
            {
                func = obj => creationFunc((T) obj),
                isOneLiner = isOneLiner
            };

            RegisterElementCreationFunc(typeof(T), cf);
        }

        public static void RegisterElementCreationFunc(Type type, CreationFunc creationFunc)
        {
            CreationFuncTable[type] = creationFunc;
        }

        public static bool UnregisterElementCreationFunc<T>()
        {
            return CreationFuncTable.Remove(typeof(T));
        }

        public static bool TryGetElementCreationMethod(Type type, out CreationFunc func)
        {
            return CreationFuncTable.TryGetValue(type, out func);
        }
        
        public static bool HasElementCreationFunc(Type type, out bool isOneLiner)
        {
            var ret = TryGetElementCreationMethod(type, out var func);
            isOneLiner = func?.isOneLiner ?? false;
            return ret;
        }


        public static void RegisterProperties<T>(params string[] propertyNames)
        {
            TypeUtility.RegisterUITargetProperties(typeof(T), propertyNames);
        }

        public static void UnregisterProperties<T>(string propertyName)
        {
            TypeUtility.UnregisterUITargetProperties(typeof(T), propertyName);
        }

        public class CreationFunc
        {
            public Func<object, Element> func;
            public bool isOneLiner;
        }
    }

    public class UICustomScope<T> : IDisposable
    {
        private readonly UICustom.CreationFunc _creationFuncCache;

        public UICustomScope(Func<T, Element> creationFunc, bool isOneLiner = false)
        {
            UICustom.TryGetElementCreationMethod(typeof(T), out _creationFuncCache);
            UICustom.RegisterElementCreationFunc(creationFunc, isOneLiner);
        }

        public void Dispose()
        {
            UICustom.RegisterElementCreationFunc(typeof(T), _creationFuncCache);
        }
    }
}