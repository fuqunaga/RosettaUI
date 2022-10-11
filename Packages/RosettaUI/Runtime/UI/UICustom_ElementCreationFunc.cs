using System;
using System.Collections.Generic;

namespace RosettaUI
{
    public static partial class UICustom
    {
        public class CreationFunc
        {
            public Func<LabelElement, IBinder, Element> func;
        }
        
        private static readonly Dictionary<Type, CreationFunc> CreationFuncTable = new();

        
        public static void RegisterElementCreationFunc<T>(Func<LabelElement, Func<T>, Element> creationFunc)
            where T : class // GetObject() as T を呼ぶためにとりあえずwhere T : class
        {
            var cf = new CreationFunc
            {
                // binder から Element を生成するべきだが複雑な UI を作ろうとするとかなり大変なので
                // binderのかわりにFunc<T>から生成してもらう
                //
                // UI.Field(() => GetObj(), obj => SetObj(obj)); などのケースで
                // binder.GetObject()で返ってきたobjを編集するだけではだめでSetObject()で通知する必要がある
               func = (label,binder) =>
                   {
                       object obj = null;
                       return creationFunc(label, () => (obj = binder.GetObject()) as T)
                           .RegisterValueChangeCallback(() => binder.SetObject(obj));
                   }
            };

            RegisterElementCreationFunc(typeof(T), cf);
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
            
            if (!CreationFuncTable.TryGetValue(type, out var func))
            {
                func = GetElementCreationMethod(type.BaseType);
                CreationFuncTable[type] = func;
            }

            return func;
        }
        
        
        
        #region Scope
        
        public readonly ref struct ElementCreationFuncScope<T>
            where T : class
        {
            private readonly CreationFunc _creationFuncCache;

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