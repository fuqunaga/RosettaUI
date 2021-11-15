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
        
        public static void RegisterPropertyOrFields<T>(params string[] names)
        {
            TypeUtility.RegisterUITargetPropertyOrFields(typeof(T), names);
        }

        public static void UnregisterPropertyOrField<T>(string name)
        {
            TypeUtility.UnregisterUITargetPropertyOrField(typeof(T), name);
        }
        
        /// <returns>removed field name</returns>
        public static IEnumerable<string> UnregisterPropertyOrFieldAll<T>()
        {
            return TypeUtility.UnregisterUITargetPropertyOrFieldAll(typeof(T));
        }

        public class CreationFunc
        {
            public Func<object, Element> func;
            public bool isOneLiner;
        }
    }
}