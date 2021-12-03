using System.Collections.Generic;

namespace RosettaUI
{
    public interface IBinderSet<T>
    {
        IEnumerable<IBinder<T>> Binders { get; }
    }
}