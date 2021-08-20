using System;


namespace Comugi
{
    public interface IBinder : IGetter
    {
        bool IsReadOnly { get; }

        object GetObject();
        void SetObject(object obj);
    }


    public abstract class BinderBase<T> : IBinder, IGetter<T>
    {
        public event Action<T> onValueChanged;

        readonly IGetter<T> getter;

        protected BinderBase(IGetter<T> getter) => this.getter = getter;


        #region IGetter<T>

        public virtual bool IsNull => getter.IsNull;

        public virtual bool IsNullable => getter.IsNullable;

        public virtual Type ValueType => getter.ValueType;

        public virtual bool IsConst => getter.IsConst;

        public virtual T Get() => getter.Get();

        #endregion


        #region IBinder

        public virtual bool IsReadOnly => onValueChanged == null;

        public virtual object GetObject() => Get();

        public virtual void SetObject(object obj) => Set((T)obj);

        #endregion


        protected abstract void SetInternal(T t);

        public void Set(T v)
        {
            if (!IsConst)
            {
                SetInternal(v);
            }

            onValueChanged?.Invoke(v);
        }
    }
}