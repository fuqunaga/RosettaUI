using System;


namespace RosettaUI
{
    public static class ConstMinMaxGetter
    {
        public static ConstMinMaxGetter<T> Create<T>((T, T) minMax) => new ConstMinMaxGetter<T>(minMax);
        public static ConstMinMaxGetter<T> Create<T>(T min, T max) => new ConstMinMaxGetter<T>((min, max));
    }


    public class ConstMinMaxGetter<T> : ConstGetter<(T, T)>, IMinMaxGetter<T>
    {
        public ConstMinMaxGetter((T, T) minMax) : base(minMax){}

        public Type MinMaxType => typeof(T);
    }

       
}