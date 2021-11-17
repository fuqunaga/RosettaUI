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
    
    
    public class UICustomScopePropertyOrFieldLabelModifier<T> : IDisposable
    {
        private readonly List<(string, string)> _cache = new List<(string, string)>();
        private readonly List<string> _remove = new List<string>();

        public UICustomScopePropertyOrFieldLabelModifier(string from, string to) : this((from, to))
        {
        }

        public UICustomScopePropertyOrFieldLabelModifier(params (string, string)[] pairs)
        {
            var type = typeof(T);
            
            foreach(var (from,to)  in pairs)
            {
                var modified = UICustom.ModifyPropertyOrFieldLabel(type, from);
                if (modified == from)
                {
                    _remove.Add(from);
                }
                else
                {
                    _cache.Add((from,modified));
                }
                
                UICustom.RegisterPropertyOrFieldLabelModifier<T>(from, to);
            }
        }

        public void Dispose()
        {
            foreach (var from in _remove)
            {
                UICustom.UnregisterPropertyOrFieldLabelModifier<T>(from);
            }
            
            foreach (var (from, to) in _cache)
            {
                UICustom.RegisterPropertyOrFieldLabelModifier<T>(from, to);
            }
        }
    }
    
}