using System;

namespace RosettaUI.UndoSystem
{
    public interface IElementRestoreRecord : IDisposable
    {
        bool TryRestore(IUndoRestoreElement element);
    }
}