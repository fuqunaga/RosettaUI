namespace RosettaUI
{
    public static class ConstMinMaxGetter
    {
        public static ConstMinMaxGetter<float> DefaultFloat { get; } = Create(0f, 1f);
        public static ConstMinMaxGetter<int> DefaultInt { get; } = Create(0, 100);

        public static ConstMinMaxGetter<T> Create<T>(MinMax<T> minMax)
        {
            return new ConstMinMaxGetter<T>(minMax);
        }

        public static ConstMinMaxGetter<T> Create<T>(T min, T max)
        {
            return new ConstMinMaxGetter<T>(MinMax.Create(min, max));
        }
    }


    public class ConstMinMaxGetter<T> : ConstGetter<MinMax<T>>
    {
        public ConstMinMaxGetter(MinMax<T> minMax) : base(minMax)
        {
        }
    }
}