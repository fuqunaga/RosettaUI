using System;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public class UICustomScopeCreationFunc<T> : IDisposable
    {
        private readonly UICustom.CreationFunc _creationFuncCache;

        public UICustomScopeCreationFunc(Func<T, Element> creationFunc, bool isOneLiner = false)
        {
            _creationFuncCache = UICustom.GetElementCreationMethod(typeof(T));
            UICustom.RegisterElementCreationFunc(creationFunc, isOneLiner);
        }
        
        public void Dispose()
        {
            UICustom.RegisterElementCreationFunc(typeof(T), _creationFuncCache);
        }
    }
    
    
    public class UICustomScopePropertyOrFields<T> : IDisposable
    {
        private readonly IEnumerable<string> _removedFieldNames;
        public UICustomScopePropertyOrFields(params string[] propertyOrFieldNames)
        {
            _removedFieldNames = UICustom.UnregisterPropertyOrFieldAll<T>();
            UICustom.RegisterPropertyOrFields<T>(propertyOrFieldNames);
        }

        public void Dispose()
        {
            UICustom.RegisterPropertyOrFields<T>(_removedFieldNames.ToArray());
        }
    }


    public class UICustomScopePropertyOrFieldLabelModifier : IDisposable
    {
        private readonly List<(string, string)> _cache = new List<(string, string)>();

        public UICustomScopePropertyOrFieldLabelModifier(string from, string to) : this((from, to))
        {
        }

        public UICustomScopePropertyOrFieldLabelModifier(params (string, string)[] pairs)
        {
            foreach (var (from, to) in pairs)
            {
                var prev = UICustom.RegisterPropertyOrFieldLabelModifier(from, to);
                _cache.Add((from, prev));
            }
        }

        public void Dispose()
        {
            foreach (var (from, to) in _cache)
            {
                if (to == null)
                {
                    UICustom.UnregisterPropertyOrFieldLabelModifier(from);
                }
                else
                {
                    UICustom.RegisterPropertyOrFieldLabelModifier(from, to);
                }
            }
        }
    }

    public class UICustomScopePropertyOrFieldLabelModifier<T> : IDisposable
    {
        private readonly List<(string, string)> _cache = new List<(string, string)>();

        public UICustomScopePropertyOrFieldLabelModifier(string from, string to) : this((from, to))
        {
        }

        public UICustomScopePropertyOrFieldLabelModifier(params (string, string)[] pairs)
        {
            var type = typeof(T);
            
            foreach(var (from,to)  in pairs)
            {
                var prev = UICustom.RegisterPropertyOrFieldLabelModifier<T>(from, to);
                _cache.Add((from, prev));
            }
        }

        public void Dispose()
        {
            foreach (var (from, to) in _cache)
            {
                if (to == null)
                {
                    UICustom.UnregisterPropertyOrFieldLabelModifier<T>(from);
                }
                else
                {
                    UICustom.RegisterPropertyOrFieldLabelModifier<T>(from, to);
                }
            }
        }
    }
    
}