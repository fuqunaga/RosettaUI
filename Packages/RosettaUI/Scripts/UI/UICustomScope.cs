using System;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public class UICustomScope<T> : IDisposable
    {
        private readonly UICustom.CreationFunc _creationFuncCache;
        private readonly IEnumerable<string> _removedFieldNames;

        public UICustomScope(Func<T, Element> creationFunc, bool isOneLiner = false)
        {
            _creationFuncCache = UICustom.GetElementCreationMethod(typeof(T));
            UICustom.RegisterElementCreationFunc(creationFunc, isOneLiner);
        }

        public UICustomScope(params string[] propertyOrFieldNames)
        {
            _removedFieldNames = UICustom.UnregisterPropertyOrFieldAll<T>();
            UICustom.RegisterPropertyOrFields<T>(propertyOrFieldNames);
        }

        public void Dispose()
        {
            UICustom.RegisterElementCreationFunc(typeof(T), _creationFuncCache);
            UICustom.RegisterPropertyOrFields<T>(_removedFieldNames.ToArray());
        }
    }
}