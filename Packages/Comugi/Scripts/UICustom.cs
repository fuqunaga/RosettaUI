using System;
using System.Collections.Generic;

namespace RosettaUI
{
    public static class UICustom
    {
        public class CreationFunc
        {
            public Func<object, Element> func;
            public bool isOneLiner;
        }

        internal static Dictionary<Type, CreationFunc> creationFuncs = new Dictionary<Type, CreationFunc>();

        internal static bool HasElementCreationFunc(Type type, out bool isOneLiner)
        {
            var ret = creationFuncs.TryGetValue(type, out var func);
            isOneLiner = func?.isOneLiner ?? false;
            return ret;
        }

        public static void RegisterElementCreationFunc<T>(Func<T, Element> creationFunc, bool isOneLIner = false)
        {
            var cf = new CreationFunc()
            {
                func = (obj) => creationFunc((T)obj),
                isOneLiner = isOneLIner
            };

            RegisterElementCreationFunc(typeof(T), cf);
        }

        public static void RegisterElementCreationFunc(Type type, CreationFunc creationFunc)
        {
            creationFuncs[type] = creationFunc;
        }


        public static bool UnregisterElementCreationFunc<T>()
        {
            return creationFuncs.Remove(typeof(T));
        }

        public static bool TryGetElementCreationMethod(Type type, out CreationFunc func) =>  creationFuncs.TryGetValue(type, out func);
    }

    public class UICustomScope<T> : IDisposable
    {
        UICustom.CreationFunc creationFuncCache;

        public UICustomScope(Func<T, Element> creationFunc, bool isOneLiner = false)
        {
            UICustom.TryGetElementCreationMethod(typeof(T), out creationFuncCache);
            UICustom.RegisterElementCreationFunc(creationFunc, isOneLiner);
        }

        public void Dispose()
        {
            UICustom.RegisterElementCreationFunc(typeof(T), creationFuncCache);
        }
    }
}