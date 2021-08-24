using System.Collections.Generic;
using UnityEngine;
using Cursor = UnityEngine.UIElements.Cursor;

namespace Comugi.UIToolkit
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
        static readonly Dictionary<CursorType, CursorData.Data> cursorTable;


        static CursorHolder()
        {
            var data = Resources.Load<CursorData>("cursorData");

            cursorTable = new Dictionary<CursorType, CursorData.Data>()
            {
                { CursorType.ResizeHorizontal, data.resizeHorizontal},
                { CursorType.ResizeVertical, data.resizeVertical},
                { CursorType.ResizeUpLeft, data.resizeUpLeft},
                { CursorType.ResizeUpRight, data.resizeUpRight},
            };
        }

        public static Cursor GetCursor(CursorType cursorType)
        {
            if (cursorTable.TryGetValue(cursorType, out var data))
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