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

        public static CreationFunc GetElementCreationMethod(Type type)
        {
            if (type == null) return null;
            
            if (!CreationFuncTable.TryGetValue(type, out var func))
            {
                func = GetElementCreationMethod(type.BaseType);
                CreationFuncTable[type] = func;
            }

            return func;
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
            _creationFuncCache = UICustom.GetElementCreationMethod(typeof(T));
            UICustom.RegisterElementCreationFunc(creationFunc, isOneLiner);
        }

        public void Dispose()
        {
            UICustom.RegisterElementCreationFunc(typeof(T), _creationFuncCache);
        }
    }
}