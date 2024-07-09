using System;
using System.Collections.Generic;

namespace RosettaUI
{
    /// <summary>
    /// 特定の型に標準のUI(UI.Field()で生成される）を指定できる
    /// </summary>
    public static partial class UICustom
    {
        private class CreationFunc
        {
            public readonly Func<LabelElement, IBinder, Element> func;
            public readonly bool includeParentTypes;

            public CreationFunc(Func<LabelElement, IBinder, Element> func, bool includeParentTypes = false) =>
                (this.func, this.includeParentTypes) = (func, includeParentTypes);
        }
        
        
        private static readonly Dictionary<Type, CreationFunc> CreationFuncTable = new();

        // exclude parent type
        public static void RegisterElementCreationFunc<T>(Func<LabelElement, Func<T>, Action<T>, Element> creationFunc)
        {
            RegisterElementCreationFunc(typeof(T), new CreationFunc((label, binder) =>
                {
                    var typedBinder = (IBinder<T>)binder;
                    return creationFunc(label, typedBinder.Get, typedBinder.Set);
                }
            ));
        }

        [Obsolete("Use other signature of RegisterElementCreationFunc().")]
        public static void RegisterElementCreationFunc<T>(Func<LabelElement, Func<T>, Element> creationFunc)
            where T : class // GetObject() as T を呼ぶためにとりあえずwhere T : class
        {
            // binder から Element を生成するべきだが複雑な UI を作ろうとするとかなり大変なので
            // binderのかわりにFunc<T>から生成してもらう
            //
            // UI.Field(() => GetObj(), obj => SetObj(obj)); などのケースで
            // binder.GetObject()で返ってきたobjを編集するだけではだめでSetObject()で通知する必要がある
            RegisterElementCreationFunc(typeof(T), new CreationFunc((label, binder) =>
                {
                    object obj = null;
                    return creationFunc(label, () => (obj = binder.GetObject()) as T)
                        .RegisterValueChangeCallback(() => binder.SetObject(obj));
                }
            ));
        }

        // exclude parent type
        public static void RegisterElementCreationFunc<T>(Func<LabelElement, IBinder<T>, Element> creationFunc)
        {
            RegisterElementCreationFunc(typeof(T),
                new CreationFunc((label, binder) => creationFunc(label, (IBinder<T>)binder)));
        }

        // include parent type
        public static void RegisterElementCreationFunc(Type type, Func<LabelElement, IBinder, Element> creationFunc)
        {
            RegisterElementCreationFunc(type, new CreationFunc(creationFunc, true));
        }
        
        private static void RegisterElementCreationFunc(Type type, CreationFunc creationFunc)
        {
            CreationFuncTable[type] = creationFunc;
        }

        public static bool UnregisterElementCreationFunc(Type type)
        {
            return CreationFuncTable.Remove(type);
        }

        
        public static Func<LabelElement, IBinder, Element> GetElementCreationFunc(Type type)
        {
            return GetElementCreationFuncData(type)?.func;
        }

        private static CreationFunc GetElementCreationFuncData(Type type, bool isTarget = true)
        {
            while (true)
            {
                if (type == null) return null;

                if (CreationFuncTable.TryGetValue(type, out var creationFunc) && (isTarget || creationFunc.includeParentTypes))
                {
                    return creationFunc;
                }

                type = type.BaseType;
                isTarget = false;
            }
        }

        #region Scope

        public readonly ref struct ElementCreationFuncScope
        {
            // exclude parent type
            public static ElementCreationFuncScope Create<T>(Func<LabelElement, Func<T>, Action<T>, Element> creationFunc)
            {
                var ret = new ElementCreationFuncScope(typeof(T));
                RegisterElementCreationFunc(creationFunc);
                return ret;
            }

            // exclude parent type
            public static ElementCreationFuncScope Create<T>(Func<LabelElement, IBinder<T>, Element> creationFunc)
            {
                var ret = new ElementCreationFuncScope(typeof(T));
                RegisterElementCreationFunc(creationFunc);
                return ret;
            }
            
            // include parent type
            public static ElementCreationFuncScope Create(Type type, Func<LabelElement, IBinder, Element> creationFunc)
            {
                var ret = new ElementCreationFuncScope(type);
                RegisterElementCreationFunc(type, creationFunc);
                return ret;
            }

            private readonly Type _type;
            private readonly CreationFunc _creationFuncCache;

            private ElementCreationFuncScope(Type type)
            {
                _type = type;
                _creationFuncCache = GetElementCreationFuncData(type);
            }
            
            public void Dispose()
            {
                RegisterElementCreationFunc(_type, _creationFuncCache);
            }
        }
        
        public readonly ref struct ElementCreationFuncScope<T>
            where T : class
        {
            private readonly CreationFunc _creationFuncCache;
            
            [Obsolete("Use ElementCreationFuncScope.Create() instead.")]
            public ElementCreationFuncScope(Func<LabelElement, Func<T>, Element> creationFunc)
            {
                _creationFuncCache = GetElementCreationFuncData(typeof(T));
                RegisterElementCreationFunc(creationFunc);
            }

            public void Dispose()
            {
                RegisterElementCreationFunc(typeof(T), _creationFuncCache);
            }
        }
        
        #endregion
    }
}