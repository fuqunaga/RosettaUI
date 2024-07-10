using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosettaUI
{
    /// <summary>
    ///　Find a location where the Window does not overlap with an existing one 
    /// </summary>
    public static class WindowLayout
    {
        private static readonly Dictionary<WindowElement, List<WindowElement>> OpenedWindowTable = new();
        private static readonly Dictionary<WindowElement, Vector2> ParentWindowRightTopTable = new(); 
        
        public static Vector2 CalcOpenWindowPosition(Vector2 originalPos,
            Rect screenRect,
            WindowElement window,
            WindowElement parentWindow,
            Func<WindowElement, Rect> getWindowRect,
            Func<WindowElement, bool> isWindowMoved)
        {
            List<WindowElement> openedWindows = null;

            Rect parentRect = default;
            if (parentWindow != null)
            {
                if (!OpenedWindowTable.TryGetValue(parentWindow, out openedWindows))
                {
                    openedWindows = OpenedWindowTable[parentWindow] = new List<WindowElement>();
                }

                // 親Windowが以前の位置から動いていたら子Windowは全部無視
                parentRect = getWindowRect(parentWindow);
                var newRightTop = new Vector2(parentRect.xMax, parentRect.yMin);
                if (ParentWindowRightTopTable.TryGetValue(parentWindow, out var rightTop))
                {
                    if (newRightTop != rightTop)
                    {
                        openedWindows.Clear();
                    }
                }
                ParentWindowRightTopTable[parentWindow] = newRightTop;

                // 閉じているor動いたWindow以降のWindowは無視
                var removeStartIndex = openedWindows.FindIndex(w => !w.IsOpen || isWindowMoved(w));
                if (removeStartIndex >= 0)
                {
                    openedWindows.RemoveRange(removeStartIndex, openedWindows.Count - removeStartIndex);
                }
            }


            const float delta = 5f;

            var pos = originalPos;
            var alignX = false;
            if (parentWindow　!= null )
            {
                var x = parentRect.xMax + delta;
                alignX = x < screenRect.xMax;
                if ( alignX )
                {
                    pos = new Vector2(x, parentRect.yMin);
                }
            }

            if ((openedWindows?.Any() ?? false) && alignX)
            {
                var previousRect = getWindowRect(openedWindows.Last());

                //　新しいWindowの上部が画面の半分より上なら、previousの下に表示
                var newY = previousRect.yMax + delta;
                if (newY < screenRect.center.y) // 半分より上
                {
                    pos.x = previousRect.xMin;
                    pos.y = newY;
                }
                // previousの下が空いてないときはopenedWindowの一番上にあるものの隣
                else
                {
                    var mostTopRect = openedWindows
                        .Select(getWindowRect)
                        .Aggregate((l, r) => l.yMin < r.yMin ? l : r); // MinBy
                    
                    var newX = mostTopRect.xMax + delta;
                    if (newX < screenRect.xMax)
                    {
                        pos.x = newX;
                        pos.y = mostTopRect.yMin;
                    }
                }
            }

            if (pos.x < 0f) pos.x = 0f;
            if (pos.y < 0f) pos.y = 0f;

                            
            openedWindows?.Add(window);

            
            return pos;
        }
    }
}