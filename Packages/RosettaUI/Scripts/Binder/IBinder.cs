using System;

namespace RosettaUI
{
    public interface IBinder : IGetter
    {
        bool IsReadOnly { get; }

        object GetObject();
        void SetObject(object obj);
    }

    public interface IBinder<T> : IBinder, IGetter<T>
    {
        void Set(T v);
    }


    public abstract class BinderBase<T> : IBinder<T>
    {
        protected IGetter<T> getter;

        protected BinderBase()
        {
        }

        protected BinderBase(IGetter<T> getter)
        {
            this.getter = getter;
        }

        public void Set(T v)
        {
            if (!IsConst) SetInternal(v);
        }

        protected abstract void SetInternal(T t);


        #region IGetter<T>

        public virtual bool IsNull => getter.IsNull;

        public virtual bool IsNullable => getter.IsNullable;

        public virtual Type ValueType => getter.ValueType;

        public virtual bool IsConst => getter.IsConst;

        public virtual T Get()
        {
            return getter.Get();
        }

        #endregion


        #region IBinder

        public abstract bool IsReadOnly { get; }

        public virtual object GetObject()
        {
            return Get();
        }

        public virtual void SetObject(object obj)
        {
            Set((T) obj);
        }

        #endregion
    }
}