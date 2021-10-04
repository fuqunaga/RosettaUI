using System.Collections.Generic;
using UnityEngine;
using Cursor = UnityEngine.UIElements.Cursor;

namespace RosettaUI.UIToolkit
{
    public enum CursorType
    {
        Default,
        ResizeHorizontal,
        ResizeVertical,
        ResizeUpLeft,
        ResizeUpRight,
    }

    public static class CursorHolder
    {
        static readonly Dictionary<CursorType, CursorData.Data> CursorTable;
        static readonly string CursorDataPath = "RosettaUI_CursorData";


        static CursorHolder()
        {
            var data = Resources.Load<CursorData>(CursorDataPath);

            CursorTable = new Dictionary<CursorType, CursorData.Data>()
            {
                { CursorType.ResizeHorizontal, data.resizeHorizontal},
                { CursorType.ResizeVertical, data.resizeVertical},
                { CursorType.ResizeUpLeft, data.resizeUpLeft},
                { CursorType.ResizeUpRight, data.resizeUpRight},
            };
        }

        public static Cursor GetCursor(CursorType cursorType)
        {
            if (CursorTable.TryGetValue(cursorType, out var data))
            {
                return new Cursor()
                {
                    texture = data.tex,
                    hotspot = data.hotspot
                };
            }

            return default;
        }
    }
}