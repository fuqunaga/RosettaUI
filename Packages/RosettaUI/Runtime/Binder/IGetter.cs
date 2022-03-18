using System;

namespace RosettaUI
{
    public interface IGetter
    {
        bool IsNull { get; }

        bool IsNullable { get; }

        bool IsConst { get; }

        Type ValueType { get; }
    }

    public interface IGetter<out T> : IGetter
    {
        T Get();
    }
}