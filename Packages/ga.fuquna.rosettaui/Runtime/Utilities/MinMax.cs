using System;

namespace RosettaUI
{
    [Serializable]
    public struct MinMax<T>
    {
        public T min;
        public T max;

        public void Deconstruct(out T item1, out T item2)
        {
            item1 = min;
            item2 = max;
        }

        public override string ToString() => $"({min}, {max})";
    }

    public static class MinMax
    {
        public static MinMax<T> Create<T>(T min, T max) => new()
        {
            min = min,
            max = max
        };
    }
}