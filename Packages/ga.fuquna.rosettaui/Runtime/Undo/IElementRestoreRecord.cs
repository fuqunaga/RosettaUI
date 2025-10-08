using System;

namespace RosettaUI.UndoSystem
{
    public interface IElementRestoreRecord : IDisposable
    {
        bool TryRestore(Element element);
    }
}