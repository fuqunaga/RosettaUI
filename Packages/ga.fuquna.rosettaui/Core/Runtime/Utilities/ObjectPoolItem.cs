using System;
using UnityEngine.Pool;

namespace RosettaUI.Utilities
{
    public class ObjectPoolItem<TObject> : IDisposable
        where TObject : ObjectPoolItem<TObject>, new()
    {
        private static readonly ObjectPool<TObject> Pool = new(() => new TObject());

        public static TObject GetPooled() => Pool.Get();

        protected static void Release(TObject record) => Pool.Release(record);

        public virtual void Dispose()
        {
            Release((TObject)this);
        }
    }
}