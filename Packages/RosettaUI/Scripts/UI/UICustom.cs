using System;
using System.Collections.Generic;

namespace RosettaUI
{
    public static class UICustom
    {
        #region Type Define
        
        public class CreationFunc
        {
            public Func<object, Element> func;
            public bool isOneLiner;
        }
        
        #endregion

        
        private static readonly Dictionary<Type, CreationFunc> CreationFuncTable = new Dictionary<Type, CreationFunc>();

        private static readonly Dictionary<Type, Dictionary<string, string>> propertyOrFieldLabelModifiers =
            new Dictionary<Type, Dictionary<string, string>>();

        
        #region CreationFunc
        
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
        
        #endregion
        
        
        #region PropertyOrField
        
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
        
        #endregion
        
        
        #region PropertyOrFieldLabelModifier
        
        public static void RegisterPropertyOrFieldLabelModifier<T>(string from, string to)
        {
            var type = typeof(T);
            if (!propertyOrFieldLabelModifiers.TryGetValue(type, out var dic) || dic == null)
            {
                dic = new Dictionary<string, string>();
                propertyOrFieldLabelModifiers[type] = dic;
            }

            dic[from] = to;
        }
        
        public static bool UnregisterPropertyOrFieldLabelModifier<T>(string from)
        {
            if (propertyOrFieldLabelModifiers.TryGetValue(typeof(T), out var dic) && dic != null)
            {
                return dic.Remove(from);
            }

            return false;
        }

        public static void UnregisterPropertyOrFieldLabelModifierAll<T>()
        {
            if (propertyOrFieldLabelModifiers.TryGetValue(typeof(T), out var dic) && dic != null)
            {
                dic.Clear();
            }
        }

        public static string ModifyPropertyOrFieldLabel(Type type, string propertyOrFieldName)
        {
            if (propertyOrFieldLabelModifiers.TryGetValue(type, out var dic) && dic != null)
            {
                if ( dic.TryGetValue(propertyOrFieldName, out var label))
                {
                    return label;
                }
            }

            return propertyOrFieldName;
        }
        
        #endregion
    }
}