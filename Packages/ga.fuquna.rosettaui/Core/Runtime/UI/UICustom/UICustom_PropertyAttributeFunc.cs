using System;
using System.Collections.Generic;
using UnityEngine;
using OverrideFunc = System.Func<UnityEngine.PropertyAttribute, RosettaUI.LabelElement, RosettaUI.IBinder, RosettaUI.Element>;
using ModificationFunc = System.Func<UnityEngine.PropertyAttribute, RosettaUI.Element, RosettaUI.Element>;
using CreateTopElementFunc = System.Func<UnityEngine.PropertyAttribute, RosettaUI.Element, System.Collections.Generic.IEnumerable<RosettaUI.Element>>;


namespace RosettaUI
{
    /// <summary>
    /// PropertyAttributeでUIを改変できる
    /// </summary>
    public static partial class UICustom
    {
        private static readonly Dictionary<Type, PropertyAttributeFunc> PropertyAttributeFuncTable = new();


        #region Register/Unregister
        
        public static void RegisterPropertyAttributeFunc<TPropertyAttribute, TValue>(Func<TPropertyAttribute, LabelElement, Func<TValue>, Action<TValue>, Element> overrideFunc)
            where TPropertyAttribute : PropertyAttribute
        {
            Debug.Assert(overrideFunc != null, "overrideFunc is null");
            
            RegisterPropertyAttributeFunc<TPropertyAttribute, TValue>((typedAttribute, label, typedBinder) =>
                {
                    return overrideFunc(typedAttribute, label, typedBinder.Get, typedBinder.Set);
                }
            );
        }
        
        public static void RegisterPropertyAttributeFunc<TPropertyAttribute, TValue>(Func<TPropertyAttribute, LabelElement, IBinder<TValue>, Element> overrideFunc)
            where TPropertyAttribute : PropertyAttribute
        {
            Debug.Assert(overrideFunc != null, "overrideFunc is null");
            
            RegisterPropertyAttributeFunc<TPropertyAttribute>((attribute, label, binder) =>
                {
                    if (binder is not IBinder<TValue> typedBinder)
                    {
                        return null;
                    }
                    
                    return overrideFunc(attribute, label, typedBinder);
                }
            );
        }
        
        public static void RegisterPropertyAttributeFunc<TPropertyAttribute>(Func<TPropertyAttribute, LabelElement, IBinder, Element> overrideFunc)
            where TPropertyAttribute : PropertyAttribute
        {
            Debug.Assert(overrideFunc != null, "overrideFunc is null");

            RegisterPropertyAttributeFunc(typeof(TPropertyAttribute), (OverrideFunc)((attribute, label, binder) =>
            {
                if (attribute is not TPropertyAttribute typedAttribute)
                {
                    Debug.LogWarning($"Attribute is not {typeof(TPropertyAttribute)}");
                    return null;
                }

                return overrideFunc(typedAttribute, label, binder);
            }));
        }
        
        public static void RegisterPropertyAttributeFunc<TPropertyAttribute>(Func<TPropertyAttribute, Element, Element> modificationFunc)
        {
            Debug.Assert(modificationFunc != null, "modificationFunc is null");
            
            RegisterPropertyAttributeFunc(typeof(TPropertyAttribute), (ModificationFunc)((attribute, element) =>
            {
                if (attribute is not TPropertyAttribute typedAttribute)
                {
                    Debug.LogWarning($"Attribute is not {typeof(TPropertyAttribute)}");
                    return null;
                }

                return modificationFunc(typedAttribute, element);
            }));
        }
        
        public static void RegisterPropertyAttributeCreateTopElementFunc<TPropertyAttribute>(Func<TPropertyAttribute, Element, IEnumerable<Element>> createTopElementFunc)
        {
            Debug.Assert(createTopElementFunc != null, "createTopElementFunc is null");
            
            RegisterPropertyAttributeFunc(typeof(TPropertyAttribute), (CreateTopElementFunc)((attribute, element) =>
            {
                if (attribute is not TPropertyAttribute typedAttribute)
                {
                    Debug.LogWarning($"Attribute is not {typeof(TPropertyAttribute)}");
                    return null;
                }

                return createTopElementFunc(typedAttribute, element);
            }));
        }


        public static bool UnregisterPropertyAttributeFunc(Type attributeType)
        {
            return PropertyAttributeFuncTable.Remove(attributeType);
        }

        
        private static void RegisterPropertyAttributeFunc(Type attributeType, PropertyAttributeFunc propertyAttributeFunc)
        {
            PropertyAttributeFuncTable[attributeType] = propertyAttributeFunc;
        }
        
        #endregion

        
        #region Scope
        
        public readonly ref struct PropertyAttributeFuncScope
        {
            public static PropertyAttributeFuncScope Create<TPropertyAttribute, TValue>(Func<TPropertyAttribute, LabelElement, Func<TValue>, Action<TValue>, Element> overrideFunc)
                where TPropertyAttribute : PropertyAttribute
            {
                var ret = new PropertyAttributeFuncScope(typeof(TPropertyAttribute));
                RegisterPropertyAttributeFunc(overrideFunc);
                return ret;
            }
            
            public static PropertyAttributeFuncScope Create<TPropertyAttribute, TValue>(Func<TPropertyAttribute, LabelElement, IBinder<TValue>, Element> overrideFunc)
                where TPropertyAttribute : PropertyAttribute
            {
                var ret = new PropertyAttributeFuncScope(typeof(TPropertyAttribute));
                RegisterPropertyAttributeFunc(overrideFunc);
                return ret;
            }
            
            public static PropertyAttributeFuncScope Create<TPropertyAttribute>(Func<TPropertyAttribute, LabelElement, IBinder, Element> overrideFunc)
                where TPropertyAttribute : PropertyAttribute
            {
                var ret = new PropertyAttributeFuncScope(typeof(TPropertyAttribute));
                RegisterPropertyAttributeFunc(overrideFunc);
                return ret;
            }

            public static PropertyAttributeFuncScope Create<TPropertyAttribute>(Func<TPropertyAttribute, Element, Element> modificationFunc)
                where TPropertyAttribute : PropertyAttribute
            {
                var ret = new PropertyAttributeFuncScope(typeof(TPropertyAttribute));
                RegisterPropertyAttributeFunc(modificationFunc);
                return ret;
            }

            public static PropertyAttributeFuncScope Create<TPropertyAttribute>(Func<TPropertyAttribute, Element, IEnumerable<Element>> createTopElementFunc)
                where TPropertyAttribute : PropertyAttribute
            {
                var ret = new PropertyAttributeFuncScope(typeof(TPropertyAttribute));
                RegisterPropertyAttributeCreateTopElementFunc(createTopElementFunc);
                return ret;
            }
            
            
            private readonly Type _attributeType;
            private readonly PropertyAttributeFunc _propertyAttributeFuncCache;

            
            private PropertyAttributeFuncScope(Type attributeType)
            {
                _attributeType = attributeType;
                _propertyAttributeFuncCache = GetPropertyAttributeFunc(attributeType);
            }

            public void Dispose()
            {
                RegisterPropertyAttributeFunc(_attributeType,  _propertyAttributeFuncCache);
            }
        }
        
        #endregion

        
        #region Get PropertyAttributeFunc
        
        public static OverrideFunc GetPropertyAttributeOverrideFunc(Type attributeType)
        {
            return GetPropertyAttributeFunc(attributeType);
        }

        public static ModificationFunc GetPropertyAttributeModificationFunc(Type attributeType)
        {
            return GetPropertyAttributeFunc(attributeType);
        }

        public static CreateTopElementFunc GetPropertyAttributeAddTopFunc(Type attributeType)
        {
            return GetPropertyAttributeFunc(attributeType);
        }
        
        private static PropertyAttributeFunc GetPropertyAttributeFunc(Type attributeType)
        {
            return PropertyAttributeFuncTable.GetValueOrDefault(attributeType);
        }

        #endregion
        
        
        // OverrideFuncとModificationFuncを透過的に扱うためのクラス
        private class PropertyAttributeFunc
        {
            private readonly OverrideFunc _overrideFuncDictionary;
            private readonly ModificationFunc _modificationFunc;
            private readonly CreateTopElementFunc _createTopElementFunc;
            
            private PropertyAttributeFunc(OverrideFunc overrideFunc)
            {
                _overrideFuncDictionary = overrideFunc;
            }
            
            private PropertyAttributeFunc(ModificationFunc modificationFunc)
            {
                _modificationFunc = modificationFunc;
            }
            
            private PropertyAttributeFunc(CreateTopElementFunc createTopElementFunc)
            {
                _createTopElementFunc = createTopElementFunc;
            }

            public static implicit operator PropertyAttributeFunc(ModificationFunc modificationFunc)
            {
                return new PropertyAttributeFunc(modificationFunc);
            }
       
            public static implicit operator PropertyAttributeFunc(OverrideFunc overrideFunc)
            {
                return new PropertyAttributeFunc(overrideFunc);
            }
            
            public static implicit operator PropertyAttributeFunc(CreateTopElementFunc createTopElementFunc)
            {
                return new PropertyAttributeFunc(createTopElementFunc);
            }
            
            public static implicit operator OverrideFunc(PropertyAttributeFunc propertyAttributeFunc)
            {
                return propertyAttributeFunc?._overrideFuncDictionary;
            }
            
            public static implicit operator ModificationFunc(PropertyAttributeFunc propertyAttributeFunc)
            {
                return propertyAttributeFunc?._modificationFunc;
            }
            
            public static implicit operator CreateTopElementFunc(PropertyAttributeFunc propertyAttributeFunc)
            {
                return propertyAttributeFunc?._createTopElementFunc;
            }
        }
    }
}
