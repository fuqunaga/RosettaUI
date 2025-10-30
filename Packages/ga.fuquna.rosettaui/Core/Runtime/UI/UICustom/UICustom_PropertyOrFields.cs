using System;
using System.Collections.Generic;
using System.Linq;

namespace RosettaUI
{
    public static partial class UICustom
    {
        public static void RegisterPropertyOrFields<T>(params string[] names)
        {
            TypeUtility.RegisterUITargetPropertyOrFields(typeof(T), names);
        }

        public static void UnregisterPropertyOrFields<T>(params string[] names)
        {
            TypeUtility.UnregisterUITargetPropertyOrFields(typeof(T), names);
        }
        
        /// <returns>removed field name</returns>
        public static IEnumerable<string> UnregisterPropertyOrFieldAll<T>()
        {
            return TypeUtility.UnregisterUITargetPropertyOrFieldAll(typeof(T));
        }
        
        
        #region Scope
        
        public readonly ref struct PropertyOrFieldsScope<T>
        {
            private readonly IEnumerable<string> _removedFieldNames;
            public PropertyOrFieldsScope(params string[] propertyOrFieldNames)
            {
                _removedFieldNames = UnregisterPropertyOrFieldAll<T>();
                RegisterPropertyOrFields<T>(propertyOrFieldNames);
            }

            public void Dispose()
            {
                RegisterPropertyOrFields<T>(_removedFieldNames.ToArray());
            }
        }

        #endregion
    }
}