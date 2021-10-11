using System;


namespace RosettaUI
{
    public static class ConstMinMaxGetter
    {
        public static ConstMinMaxGetter<T> Create<T>(MinMax<T> minMax) => new ConstMinMaxGetter<T>(minMax);
        public static ConstMinMaxGetter<T> Create<T>(T min, T max) => new ConstMinMaxGetter<T>(MinMax.Create(min, max));

        public static ConstMinMaxGetter<float> DefaultFloat { get; } = Create(0f, 1f);
        public static ConstMinMaxGetter<int> DefaultInt { get; } = Create(0, 100);
    }


    public class ConstMinMaxGetter<T> : ConstGetter<MinMax<T>>, IMinMaxGetter<T>
    {
        public ConstMinMaxGetter(MinMax<T> minMax) : base(minMax){}

        public Type MinMaxType => typeof(T);
    }

       
}