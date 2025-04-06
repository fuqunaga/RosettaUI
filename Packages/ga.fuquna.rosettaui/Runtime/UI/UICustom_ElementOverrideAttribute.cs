using System;
using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI
{
    /// <summary>
    /// 特定のAttributeで標準のフィールドのUIを改変できる
    /// </summary>
    public static partial class UICustom
    {
        private class OverrideFunc
        {
            public readonly bool isModify;
            public readonly Func<PropertyAttribute, Element, LabelElement, IBinder, Element> func;
            public readonly bool includeParentTypes;

            public OverrideFunc(bool isModify, Func<PropertyAttribute, Element, LabelElement, IBinder, Element> func, bool includeParentTypes = false)
            {
                this.isModify = isModify;
                this.func = func;
                this.includeParentTypes = includeParentTypes;
            }
        }

        private static readonly Dictionary<(Type, Type), OverrideFunc> OverrideByAttributeFuncTable = new();


        #region ElementOverrideAttribute
        // exclude parent type
        public static void RegisterElementOverrideAttribute<TAttribute, TValue>(Func<TAttribute, LabelElement, Func<TValue>, Action<TValue>, Element> overrideFunc)
            where TAttribute : PropertyAttribute
        {
            RegisterElementOverrideAttribute(typeof(TAttribute), typeof(TValue), new OverrideFunc(false, (attribute, originalElement, label, binder) =>
                {
                    if (binder is not IBinder<TValue> typedBinder) return originalElement;
                    return overrideFunc((TAttribute)attribute, label, typedBinder.Get, typedBinder.Set);
                }
            ));
        }

        // exclude parent type
        public static void RegisterElementOverrideAttribute<TAttribute, TValue>(Func<TAttribute, LabelElement, IBinder<TValue>, Element> overrideFunc)
            where TAttribute : PropertyAttribute
        {
            RegisterElementOverrideAttribute(typeof(TAttribute), typeof(TValue), new OverrideFunc(false, (attribute, originalElement, label, binder) =>
                {
                    if (binder is not IBinder<TValue> typedBinder) return originalElement;
                    return overrideFunc((TAttribute)attribute, label, typedBinder);
                }
            ));
        }

        // include parent type
        public static void RegisterElementOverrideAttribute<TAttribute>(Type type, Func<TAttribute, LabelElement, IBinder, Element> overrideFunc)
            where TAttribute : PropertyAttribute
        {
            RegisterElementOverrideAttribute(typeof(TAttribute), type, new OverrideFunc(false, (attribute, _, label, binder)
                    => overrideFunc((TAttribute)attribute, label, binder),
                true
            ));
        }

        #region Scope
        public readonly ref struct ElementOverrideAttributeScope
        {
            // exclude parent type
            public static ElementOverrideAttributeScope Create<TAttribute, TValue>(Func<TAttribute, LabelElement, Func<TValue>, Action<TValue>, Element> overrideFunc)
                where TAttribute : PropertyAttribute
            {
                var ret = new ElementOverrideAttributeScope(typeof(TAttribute), typeof(TValue));
                RegisterElementOverrideAttribute(overrideFunc);
                return ret;
            }

            // exclude parent type
            public static ElementOverrideAttributeScope Create<TAttribute, TValue>(Func<TAttribute, LabelElement, IBinder<TValue>, Element> overrideFunc)
                where TAttribute : PropertyAttribute
            {
                var ret = new ElementOverrideAttributeScope(typeof(TAttribute), typeof(TValue));
                RegisterElementOverrideAttribute(overrideFunc);
                return ret;
            }

            // include parent type
            public static ElementOverrideAttributeScope Create<TAttribute>(Type type, Func<TAttribute, LabelElement, IBinder, Element> overrideFunc)
                where TAttribute : PropertyAttribute
            {
                var ret = new ElementOverrideAttributeScope(type);
                RegisterElementOverrideAttribute(type, overrideFunc);
                return ret;
            }

            private readonly Type _attributeType;
            private readonly Type _valueType;
            private readonly OverrideFunc _attributeOverriderCache;

            private ElementOverrideAttributeScope(Type attributeType, Type valueType = null)
            {
                _attributeType = attributeType;
                _valueType = valueType;
                _attributeOverriderCache = GetElementOverrideAttributeData(attributeType, valueType);
            }

            public void Dispose()
            {
                RegisterElementOverrideAttribute(_attributeType,  _valueType, _attributeOverriderCache);
            }
        }
        #endregion

        #endregion

        #region ElementModifyAttribute
        public static void RegisterElementModifyAttribute<TAttribute>(Func<TAttribute, Element, Element> modifyFunc)
            where TAttribute : PropertyAttribute
        {
            RegisterElementOverrideAttribute(typeof(TAttribute), null, new OverrideFunc(true, (attribute, originalElement, _, _) =>
                {
                    return modifyFunc((TAttribute)attribute, originalElement);
                },
                true
            ));
        }

        #region Scope
        public readonly ref struct ElementModifyAttributeScope
        {
            public static ElementModifyAttributeScope Create<T>(Func<T, Element, Element> modifyFunc)
                where T : PropertyAttribute
            {
                var ret = new ElementModifyAttributeScope(typeof(T));
                RegisterElementModifyAttribute(modifyFunc);
                return ret;
            }

            private readonly Type _type;
            private readonly OverrideFunc _overrideFuncCache;

            private ElementModifyAttributeScope(Type type)
            {
                _type = type;
                _overrideFuncCache = GetElementOverrideAttributeData(type, null);
            }

            public void Dispose()
            {
                RegisterElementOverrideAttribute(_type, null, _overrideFuncCache);
            }
        }
        #endregion
        #endregion

        private static void RegisterElementOverrideAttribute(Type attributeType, Type valueType, OverrideFunc overrideFunc)
        {
            OverrideByAttributeFuncTable[(attributeType, valueType)] = overrideFunc;
        }

        public static bool UnregisterElementOverrideAttribute(Type attributeType, Type valueType)
        {
            return OverrideByAttributeFuncTable.Remove((attributeType, valueType));
        }

        #region DataGetter
        public static Func<PropertyAttribute, Element, LabelElement, IBinder, Element> GetElementOverrideAttribute(Type attributeType, Type valueType)
        {
            var overrideFunc = GetElementOverrideAttributeData(attributeType, valueType);
            if (overrideFunc == null || overrideFunc.isModify) return null;
            return overrideFunc.func;
        }

        public static Func<PropertyAttribute, Element, LabelElement, IBinder, Element> GetElementModifyAttribute(Type attributeType, Type valueType)
        {
            var overrideFunc = GetElementOverrideAttributeData(attributeType, valueType);
            if (overrideFunc == null || !overrideFunc.isModify) return null;
            return overrideFunc.func;
        }

        private static OverrideFunc GetElementOverrideAttributeData(Type attributeType, Type valueType, bool isTarget = true)
        {
            while (true)
            {
                if (OverrideByAttributeFuncTable.TryGetValue((attributeType, valueType), out var overrideFunc) && (isTarget || (overrideFunc?.includeParentTypes ?? false)))
                {
                    return overrideFunc;
                }

                if (valueType == null) return null;

                valueType = valueType.BaseType;
                isTarget = false;
            }
        }
        #endregion
    }
}
