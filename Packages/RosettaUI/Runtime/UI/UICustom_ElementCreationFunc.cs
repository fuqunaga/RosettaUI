using System;
using System.Collections.Generic;

namespace RosettaUI
{
    using CreationFunc = Func<LabelElement, IBinder, Element>;
    
    public static partial class UICustom
    {
        private static readonly Dictionary<Type, CreationFunc> CreationFuncTable = new();

        public static void RegisterElementCreationFunc<T>(Func<LabelElement, Func<T>, Action<T>, Element> creationFunc)
        {
            RegisterElementCreationFunc(typeof(T), CreationFunc);
            
            Element CreationFunc(LabelElement label, IBinder binder)
            {
                var typedBinder = (IBinder<T>)binder;
                return creationFunc(label, typedBinder.Get, typedBinder.Set);
            }
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
            Element CreationFunc(LabelElement label, IBinder binder)
            {
                object obj = null;
                return creationFunc(label, () => (obj = binder.GetObject()) as T)
                    .RegisterValueChangeCallback(() => binder.SetObject(obj));
            }

            RegisterElementCreationFunc(typeof(T), CreationFunc);
        }

        public static void RegisterElementCreationFunc<T>(Func<LabelElement, IBinder<T>, Element> creationFunc)
        {
            RegisterElementCreationFunc(typeof(T), CreationFunc);
            
            Element CreationFunc(LabelElement label, IBinder binder)
            {
                return creationFunc(label, (IBinder<T>)binder);
            }
        }
        
        public static void RegisterElementCreationFunc(Type type, CreationFunc creationFunc)
        {
            CreationFuncTable[type] = creationFunc;
        }

        public static bool UnregisterElementCreationFunc(Type type)
        {
            return CreationFuncTable.Remove(type);
        }

        public static CreationFunc GetElementCreationMethod(Type type)
        {
            if (type == null) return null;

            return CreationFuncTable.TryGetValue(type, out var func)
                ? func
                : CreationFuncTable[type] = GetElementCreationMethod(type.BaseType);
        }
        
        
        #region Scope

        public readonly ref struct ElementCreationFuncScope
        {
            public static ElementCreationFuncScope Create<T>(Func<LabelElement, Func<T>, Action<T>, Element> creationFunc)
            {
                var ret = new ElementCreationFuncScope(typeof(T));
                RegisterElementCreationFunc(creationFunc);
                return ret;
            }

            public static ElementCreationFuncScope Create<T>(Func<LabelElement, IBinder<T>, Element> creationFunc)
            {
                var ret = new ElementCreationFuncScope(typeof(T));
                RegisterElementCreationFunc(creationFunc);
                return ret;
            }
            
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
                _creationFuncCache = GetElementCreationMethod(type);
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
                _creationFuncCache = GetElementCreationMethod(typeof(T));
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