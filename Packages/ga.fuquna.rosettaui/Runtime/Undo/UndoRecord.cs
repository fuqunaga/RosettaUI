using System;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.Pool;

namespace RosettaUI.UndoSystem
{
    public interface IUndoRecord : IDisposable
    {
        string Name { get; }
        bool IsExpired { get; }
        
        void Undo();
        void Redo();
        
        bool CanMerge(IUndoRecord newer);
        void Merge(IUndoRecord newer);
    }

    
    public abstract class UndoRecordWithPool<TUndoRecord> 
        where TUndoRecord : UndoRecordWithPool<TUndoRecord>, new()
    {
        private static readonly ObjectPool<TUndoRecord> Pool = new(() => new TUndoRecord());
        
        public static TUndoRecord GetPooled() => Pool.Get();

        protected static void Release(TUndoRecord record) => Pool.Release(record);
    }
    
    
    public abstract class ElementUndoRecord<TUndoRecord> : UndoRecordWithPool<TUndoRecord> , IUndoRecord
        where TUndoRecord : ElementUndoRecord<TUndoRecord>, new()
    {
        protected Element element;
        
        protected void Initialize(Element targetElement) => element = targetElement;

        public abstract string Name { get; }
        public bool IsExpired => !element.EnableInHierarchy();
        public abstract void Undo();
        public abstract void Redo();

        public virtual bool CanMerge(IUndoRecord newer) => (newer is TUndoRecord r) && r.element == element;
        
        public abstract void Merge(IUndoRecord newer);
        
        public virtual void Dispose()
        {
            element = null;
            Release((TUndoRecord)this);
        }
    }

    
    public class FieldBaseElementUndoRecord<TValue> : ElementUndoRecord<FieldBaseElementUndoRecord<TValue>>
    {
        public static void Register(FieldBaseElement<TValue> field, TValue before, TValue after)
        {
            var record = GetPooled();
            record.Initialize(field, before, after);
            UndoHistory.Add(record);
        }
        
        // To prevent the value used for Undo from being modified externally, make a copy
        // For reference types, such as ObjectField's Object, it's only relevant in the Editor, so it's ignored
        private static TValue Clone(TValue value)
        {
            return value switch
            {
                Gradient g => (TValue)(object)GradientHelper.Clone(g),
                AnimationCurve ac => (TValue)(object)AnimationCurveHelper.Clone(ac),
                _ => value
            };
        }

        
        private TValue _before;
        private TValue _after;
        
        private FieldBaseElement<TValue> Element => (FieldBaseElement<TValue>)element;


        private void Initialize(FieldBaseElement<TValue> field, TValue before, TValue after)
        {
            base.Initialize(field);
            _before = Clone(before);
            _after = Clone(after);
        }

        public override string Name => Element.Label;
        
        public override void Undo()
        {
            Element.GetViewBridge().SetValueFromView(Clone(_before));
        }

        public override void Redo()
        {
            Element.GetViewBridge().SetValueFromView(Clone(_after));
        }

        public override bool CanMerge(IUndoRecord newer) => base.CanMerge(newer) && typeof(TValue) != typeof(bool);
        
        public override void Merge(IUndoRecord newer)
        {
            if (newer is not FieldBaseElementUndoRecord<TValue> r)
            {
                throw new InvalidOperationException($"Cannot merge {GetType()} with {newer.GetType()}");
            }

            _after = r._after;
        }

        public override void Dispose()
        {
            _before = default;
            _after = default;
            base.Dispose();
        }
    }
}