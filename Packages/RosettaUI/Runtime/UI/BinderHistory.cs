using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace RosettaUI
{
    /// <summary>
    /// BinderToElement内で循環参照を検知するために呼ばれたBinderのValueTypeを記録しておく
    /// </summary>
    internal static class BinderHistory
    {
        private static readonly HashSet<IBinder> History = new();


        public static bool IsTarget(IBinder binder) => !binder.ValueType.IsValueType;
        
        public static bool IsCircularReference(IBinder binder)
        {
            if (!IsTarget(binder)) return false;
            
            var obj = binder.GetObject();
            if (obj == null) return false;

            var valueType = binder.ValueType;

            return History
                .Where(b => b.ValueType == valueType)
                .Select(b => b.GetObject())
                .Contains(obj);
        }

        public static BinderHistoryScope GetScope(IBinder binder) => new(binder);

        internal readonly struct BinderHistoryScope : IDisposable
        {
            private readonly IBinder _binder;
            
            public BinderHistoryScope(IBinder binder)
            {
                if (IsTarget(binder))
                {
                    Assert.IsTrue(History.Add(binder));
                    _binder = binder;
                }
                else
                {
                    _binder = null;
                }
            }
            
            public void Dispose()
            {
                if (_binder != null)
                {
                    History.Remove(_binder);
                }
            }
        }
        
        
        internal class Snapshot
        {
            public static Snapshot Create() => new ();

            private readonly HashSet<IBinder> _snapshot;
        
            protected Snapshot()
            {
                _snapshot = new(History);
            }
            
            public SnapshotApplyScope GetApplyScope() => new(_snapshot);

            
            internal readonly struct SnapshotApplyScope : IDisposable
            {
                private readonly HashSet<IBinder> _snapshot;
                
                public SnapshotApplyScope(HashSet<IBinder> snapshot)
                {
                    foreach (var binder in snapshot)
                    {
                        var success = History.Add(binder);
                        Assert.IsTrue(success, $"[{binder}] has already existed in {nameof(BinderHistory)}");
                    }

                    _snapshot = snapshot;
                }
            
                public void Dispose()
                {
                    History.ExceptWith(_snapshot);
                }
            }
        }
    }


    
}