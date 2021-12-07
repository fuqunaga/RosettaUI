using UnityEngine;


namespace RosettaUI.UIToolkit
{
    public static class CursorManager
    {
        public static void SetCursor(CursorType cursorType)
        {
            var data = CursorHolder.GetCursor(cursorType);

            Cursor.SetCursor(data.tex, data.hotspot, CursorMode.Auto);
        }

        public static void ResetCursor()
        {
            Cursor.SetCursor(null, default, CursorMode.Auto);
        }
    }
}