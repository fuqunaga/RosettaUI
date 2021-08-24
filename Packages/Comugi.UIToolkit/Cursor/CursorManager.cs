using UnityEngine;


namespace Comugi.UIToolkit
{
    public static class CursorManager
    {
        public static void SetCursor(CursorType cursorType)
        {
            var cursor = CursorHolder.GetCursor(cursorType);

            Cursor.SetCursor(cursor.texture, cursor.hotspot, CursorMode.Auto);
        }

        public static void ResetCursor()
        {
            Cursor.SetCursor(null, default, CursorMode.Auto);
        }
    }
}