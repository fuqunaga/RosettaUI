using System.Collections.Generic;
using NUnit.Framework;
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
        // ReSharper disable once MemberCanBePrivate.Global
        public static string CursorDataPath { get; set; } = "RosettaUI_CursorData";
        
        private static readonly Dictionary<CursorType, CursorData.Data> CursorTable;


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

        public static CursorData.Data GetCursorData(CursorType cursorType)
        {
            CursorTable.TryGetValue(cursorType, out var data);
            return data;
        }

        public static Cursor Get(CursorType cursorType)
        {
            var data = GetCursorData(cursorType);
            Assert.IsNotNull(data);
        
            return new Cursor
            {
                texture = data.tex,
                hotspot = data.hotspot
            };
        }
    }
}