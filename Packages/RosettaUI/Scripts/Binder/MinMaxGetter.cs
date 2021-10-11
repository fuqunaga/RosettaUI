using System;

namespace RosettaUI
{
    public interface IMinMaxGetter : IGetter
    {
        Type MinMaxType { get; }
    }

    public interface IMinMaxGetter<T> : IMinMaxGetter, IGetter<MinMax<T>>
    {
    }

    public class MinMaxGetter<T> : Getter<MinMax<T>>, IMinMaxGetter<T>
    {
        public MinMaxGetter(Func<MinMax<T>> func) : base(func) { }

        public Type MinMaxType => typeof(T);
    }
}