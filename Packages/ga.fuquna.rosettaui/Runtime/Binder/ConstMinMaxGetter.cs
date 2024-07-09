namespace RosettaUI
{
    public static class ConstMinMaxGetter
    {
        public static ConstGetter<MinMax<float>> DefaultFloat { get; } = Create(0f, 1f);
        public static ConstGetter<MinMax<int>> DefaultInt { get; } = Create(0, 100);

        public static ConstGetter<MinMax<T>> Create<T>(MinMax<T> minMax)
        {
            return new ConstGetter<MinMax<T>>(minMax);
        }

        public static ConstGetter<MinMax<T>> Create<T>(T min, T max)
        {
            return new ConstGetter<MinMax<T>>(MinMax.Create(min, max));
        }
    }
}