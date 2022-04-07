using System;
using System.Collections.Generic;

namespace RosettaUI
{
    public static partial class UICustom
    {
        private static readonly Dictionary<Type, Dictionary<string, string>> PropertyOrFieldLabelModifiers = new();

        private static readonly Dictionary<string, string> PropertyOrFieldLabelModifiersAnyType = new();


        public static string RegisterPropertyOrFieldLabelModifier(string from, string to)
        {
            PropertyOrFieldLabelModifiersAnyType.TryGetValue(from, out var prev);
            PropertyOrFieldLabelModifiersAnyType[from] = to;

            return prev;
        }

        public static bool UnregisterPropertyOrFieldLabelModifier(string from)
        {
            return PropertyOrFieldLabelModifiersAnyType.Remove(from);
        }
        
        public static string RegisterPropertyOrFieldLabelModifier<T>(string from, string to)
        {
            var type = typeof(T);
            if (!PropertyOrFieldLabelModifiers.TryGetValue(type, out var dic) || dic == null)
            {
                dic = new Dictionary<string, string>();
                PropertyOrFieldLabelModifiers[type] = dic;
            }

            dic.TryGetValue(from, out var prev);
            dic[from] = to;

            return prev;
        }
        
        public static bool UnregisterPropertyOrFieldLabelModifier<T>(string from)
        {
            if (PropertyOrFieldLabelModifiers.TryGetValue(typeof(T), out var dic) && dic != null)
            {
                return dic.Remove(from);
            }

            return false;
        }

        public static void UnregisterPropertyOrFieldLabelModifierAll<T>()
        {
            if (PropertyOrFieldLabelModifiers.TryGetValue(typeof(T), out var dic) && dic != null)
            {
                dic.Clear();
            }
        }

        public static string ModifyPropertyOrFieldLabel(Type type, string propertyOrFieldName)
        {
            if (PropertyOrFieldLabelModifiers.TryGetValue(type, out var dic) && dic != null)
            {
                if ( dic.TryGetValue(propertyOrFieldName, out var label))
                {
                    return label;
                }
            }

            if (PropertyOrFieldLabelModifiersAnyType.TryGetValue(propertyOrFieldName, out var to))
            {
                return to;
            }
            

            return propertyOrFieldName;
        }
        
        
        
        #region Scope

        public readonly ref struct PropertyOrFieldLabelModifierScope
        {
            private readonly List<(string, string)> _cache;

            public PropertyOrFieldLabelModifierScope(string from, string to) : this((from, to))
            {
            }

            public PropertyOrFieldLabelModifierScope(params (string, string)[] pairs)
            {
                _cache = new();
                
                foreach (var (from, to) in pairs)
                {
                    var prev = RegisterPropertyOrFieldLabelModifier(from, to);
                    _cache.Add((from, prev));
                }
            }

            public void Dispose()
            {
                foreach (var (from, to) in _cache)
                {
                    if (to == null)
                    {
                        UnregisterPropertyOrFieldLabelModifier(from);
                    }
                    else
                    {
                        RegisterPropertyOrFieldLabelModifier(from, to);
                    }
                }
            }
        }

        public readonly ref struct PropertyOrFieldLabelModifierScope<T>
        {
            private readonly List<(string, string)> _cache;

            public PropertyOrFieldLabelModifierScope(string from, string to) : this((from, to))
            {
            }

            public PropertyOrFieldLabelModifierScope(params (string, string)[] pairs)
            {
                _cache = new();
                
                foreach (var (from, to) in pairs)
                {
                    var prev = RegisterPropertyOrFieldLabelModifier<T>(from, to);
                    _cache.Add((from, prev));
                }
            }

            public void Dispose()
            {
                foreach (var (from, to) in _cache)
                {
                    if (to == null)
                    {
                        UnregisterPropertyOrFieldLabelModifier<T>(from);
                    }
                    else
                    {
                        RegisterPropertyOrFieldLabelModifier<T>(from, to);
                    }
                }
            }
        }
        
        #endregion
    }
}