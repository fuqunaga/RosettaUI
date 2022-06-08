using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace RosettaUI
{
    /// <summary>
    /// BinderToElement内で循環参照を検知するために呼ばれたBinderのValueTypeを記録しておく
    /// </summary>
    internal static class BinderTypeHistory
    {
        private static readonly HashSet<Type> History = new();

        public static bool IsExistingType(Type type) => History.Contains(type);
        
        public static BinderTypeHistoryScope GetScope(Type type)
        {
            Assert.IsTrue(History.Add(type));
            return new BinderTypeHistoryScope(type);
        } 

        internal readonly struct BinderTypeHistoryScope : IDisposable
        {
            private readonly Type _type;
            
            public BinderTypeHistoryScope(Type type) => _type = type;
            public void Dispose() => History.Remove(_type);
        }
        
        
        internal class Snapshot
        {
            public static Snapshot Create() => new ();

            private readonly HashSet<Type> _snapshot;
        
            protected Snapshot()
            {
                _snapshot = new(History);
            }
            
            public SnapshotApplyScope GetApplyScope() => new(_snapshot);

            
            internal readonly struct SnapshotApplyScope : IDisposable
            {
                private readonly HashSet<Type> _snapshot;
                
                public SnapshotApplyScope(HashSet<Type> snapshot)
                {
                    foreach (var type in snapshot)
                    {
                        var success = History.Add(type);
                        Assert.IsTrue(success, $"[{type}] has already existed in {nameof(BinderTypeHistory)}");
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