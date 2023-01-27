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
}