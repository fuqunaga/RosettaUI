using System;

namespace RosettaUI.Undo
{
    public interface IElementRestoreRecord : IDisposable
    {
        bool TryRestore(IUndoRestoreElement element);
    }
}