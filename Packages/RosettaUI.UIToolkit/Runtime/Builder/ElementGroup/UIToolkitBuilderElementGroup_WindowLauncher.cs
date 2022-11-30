using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private static readonly Dictionary<WindowElement, List<WindowElement>> WindowLauncherOpenedWindowTable = new();

        private VisualElement Build_WindowLauncher(Element element)
        {
            var launcherElement = (WindowLauncherElement) element;
            var windowElement = launcherElement.window;
            var window = (Window) Build(windowElement);

            var toggle = Build_Field<bool, Toggle>(launcherElement, false);
            toggle.AddToClassList(UssClassName.WindowLauncher);
            toggle.RegisterCallback<PointerUpEvent>(OnPointUpEventFirst);

            var labelElement = launcherElement.label;
            labelElement?.GetViewBridge().SubscribeValueOnUpdateCallOnce(v => toggle.text = v);

            ApplyMinusIndentIfPossible(toggle, element);
            
            
            return toggle;

            // ほかのWindowにかぶらない位置を計算する
            // 一度ドラッグしたWindowはその位置を覚えてこの処理の対象にはならない
            void OnPointUpEventFirst(PointerUpEvent evt)
            {
                // Toggleの値が変わるのはこのイベントの後
                if (windowElement.Enable) return;
                
                // positionが指定されていたらそちらを優先
                if (windowElement.positionRx.Value.HasValue)
                {
                    window.Show(toggle);
                    return;
                }
                
                // Auto layout
                var pos = evt.originalMousePosition;
                var parentWindowElement = launcherElement.Parents().OfType<WindowElement>().FirstOrDefault();
                
                if (parentWindowElement != null && GetUIObj(parentWindowElement) is Window parentWindow)
                {
                    const float delta = 5f;
                    var area = parentWindow.panel.visualTree.layout;
                    
                    var rect = parentWindow.layout;
                    var x = rect.xMax + delta;
                    if (x < area.xMax)
                    {
                        pos = new Vector2(x, rect.yMin);
                    }

                    if (!WindowLauncherOpenedWindowTable.TryGetValue(parentWindowElement, out var openedWindows))
                    {
                        openedWindows = WindowLauncherOpenedWindowTable[parentWindowElement] = new List<WindowElement>();
                    }

                    var lastIndex = openedWindows.FindLastIndex(w => w is {IsOpen: true} && GetUIObj(w) is Window {IsMoved: false});
                    if (lastIndex >= 0)
                    {
                        var removeIndex = lastIndex + 1;
                        openedWindows.RemoveRange(removeIndex, openedWindows.Count - removeIndex);
                    }

                    var lastOpenedWindowElement = openedWindows.LastOrDefault();
                    if (lastOpenedWindowElement != null &&  GetUIObj(lastOpenedWindowElement) is Window targetWindow)
                    {
                        var targetWindowRect = targetWindow.layout;

                        if ( Vector2.Distance(pos,targetWindowRect.position) < delta)
                        {
                            
                            var areaYHalf = area.center.y;

                            var yMax = targetWindowRect.yMax;
                            if (yMax < areaYHalf)
                            {
                                pos.y = yMax + delta;
                            }
                            else
                            {
                                var newX = targetWindowRect.xMax + delta; 
                                if (newX < area.xMax)
                                {
                                    pos.x = newX;
                                }
                            }
                        }
                    }
                    
                    openedWindows.Add(windowElement);
                }

                if (pos.x < 0f) pos.x = 0f;
                if (pos.y < 0f) pos.y = 0f;

                window.Show(pos, toggle);
            }
        }
    }
}