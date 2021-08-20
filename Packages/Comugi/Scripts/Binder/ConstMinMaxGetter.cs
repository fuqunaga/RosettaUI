using System;


namespace Comugi
{
    public static class ConstMinMaxGetter
    {
        public static ConstMinMaxGetter<T> Create<T>((T, T) minMax) => new ConstMinMaxGetter<T>(minMax);
    }


    public class ConstMinMaxGetter<T> : ConstGetter<(T, T)>, IMinMaxGetter<T>
    {
        public ConstMinMaxGetter((T, T) minMax) : base(minMax){}

        public Type MinMaxType => typeof(T);
    }

       
}