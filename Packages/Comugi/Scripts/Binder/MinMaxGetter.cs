using System;

namespace RosettaUI
{
    public interface IMinMaxGetter : IGetter
    {
        Type MinMaxType { get; }
    }

    public interface IMinMaxGetter<T> : IMinMaxGetter, IGetter<(T,T)>
    {
    }

    public class MinMaxGetter<T> : Getter<(T, T)>, IMinMaxGetter<T>
    {
        public MinMaxGetter(Func<(T, T)> func) : base(func) { }

        public Type MinMaxType => typeof(T);
    }
}