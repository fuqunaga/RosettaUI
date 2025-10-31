using UnityEngine;


namespace RosettaUI.UIToolkit
{
    public static class CursorManager
    {
        public static void SetCursor(CursorType cursorType)
        {
            var data = CursorHolder.GetCursorData(cursorType);

            if (data == null)
            {
                ResetCursor();
            }
            else
            {
                Cursor.SetCursor(data.tex, data.hotspot, CursorMode.Auto);
            }
        }

        public static void ResetCursor()
        {
            Cursor.SetCursor(null, default, CursorMode.Auto);
        }
    }
}