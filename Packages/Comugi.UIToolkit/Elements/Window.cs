using System;
using UnityEngine;
using UnityEngine.UIElements;


namespace Comugi.UIToolkit
{
    public class Window : VisualElement
    {
        #region Type Define

        [Flags]
        public enum ResizeEdge
        {
            None = 0,
            Top = 1,
            Bottom = 2,
            Left = 4,
            Right = 8,
        }


        public enum DragMode
        {
            None,
            DragWindow,
            ResizeWindow
        }

        #endregion

        public static readonly string ussClassName = "comugi-window";
        readonly VisualElement _titleBarContainer = new VisualElement();
        readonly VisualElement _titleBarContainerRight = new VisualElement();
        readonly VisualElement _titleBarContainerLeft = new VisualElement();
        Button _closeButton;

        DragMode dragMode;

        Vector2 draggingLocalPosition;

        ResizeEdge resizeEdge;


        public VisualElement titleBarContainerRight => _titleBarContainerRight;
        public VisualElement titleBarContainerLeft => _titleBarContainerLeft;

        public Button closeButton
        {
            get => _closeButton;
            set
            {
                if (_closeButton != value)
                {
                    if (_closeButton != null)
                    {
                        hierarchy.Remove(_closeButton);
                    }

                    _closeButton = value;
                    titleBarContainerRight.Add(_closeButton);
                }
            }
        }


        public Window()
        {
            AddToClassList(ussClassName);

            _titleBarContainer.AddToClassList(ussClassName + "__titlebar-container");
            _titleBarContainerRight.AddToClassList(ussClassName + "__titlebar-container__right");
            _titleBarContainerLeft.AddToClassList(ussClassName + "__titlebar-container__left");

            _titleBarContainer.Add(titleBarContainerLeft);
            _titleBarContainer.Add(titleBarContainerRight);
            hierarchy.Add(_titleBarContainer);

            closeButton = new CloseButton();
            closeButton.clicked += () => visible = !visible;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseOutEvent>(OnMouseOut);
        }


        #region Event

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                if (resizeEdge != ResizeEdge.None)
                {
                    StartResizeWindow();
                }
                else
                {
                    StartDragWindow(evt.localPosition);
                }

                RegisterPanelCallback();
            }
        }

        private void OnPointerMoveOnRoot(PointerMoveEvent evt)
        {
            if (dragMode != DragMode.None)
            {
                // 画面外でボタンをUpされた場合検知できないので、現在押下中かどうかで判定する
                if ((evt.pressedButtons & 0x1) != 0)
                {
                    switch (dragMode)
                    {
                        case DragMode.DragWindow:
                            UpdateDragWindw(evt.position);
                            break;

                        case DragMode.ResizeWindow:
                            UpdateResizeWindow(evt.position);
                            break;
                    }
                }
                else
                {
                    FinishDrag();
                }
            }
        }


        private void OnPointerUpOnRoot(PointerUpEvent evt)
        {
            if (evt.button == 0)
            {
                dragMode = DragMode.None;
                FinishDrag();
            }
        }


        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (dragMode == DragMode.None)
            {
                UpdateResizeEdgeAndCursor(evt.localMousePosition);
            }
        }

        private void OnMouseOut(MouseOutEvent evt)
        {
            if (dragMode != DragMode.ResizeWindow)
            {
                CursorManager.ResetCursor();
            }
        }


        #endregion


        void FinishDrag()
        {
            dragMode = DragMode.None;
            UnregisterPanelCallback();

            ResetCursor();
        }

        void ResetCursor()
        {
            if (resizeEdge != ResizeEdge.None)
            {
                CursorManager.ResetCursor();
            }
        }


        #region DragWindow

        void StartDragWindow(Vector2 localPosition)
        {
            dragMode = DragMode.DragWindow;
            draggingLocalPosition = localPosition;
        }


        void UpdateDragWindw(Vector2 position)
        {
            var pos = position - draggingLocalPosition;

            style.left = pos.x;
            style.top = pos.y;
        }



        void RegisterPanelCallback()
        {
            var root = panel.visualTree;
            root.RegisterCallback<PointerMoveEvent>(OnPointerMoveOnRoot);
            root.RegisterCallback<PointerUpEvent>(OnPointerUpOnRoot);
        }

        void UnregisterPanelCallback()
        {
            var root = panel.visualTree;
            root.UnregisterCallback<PointerMoveEvent>(OnPointerMoveOnRoot);
            root.UnregisterCallback<PointerUpEvent>(OnPointerUpOnRoot);
        }

        #endregion



        #region Resize Window

        void StartResizeWindow()
        {
            dragMode = DragMode.ResizeWindow;
        }

        private void UpdateResizeWindow(Vector2 position)
        {
            SetResizeCursor();


            if (resizeEdge.HasFlag(ResizeEdge.Top))
            {
                var diff = resolvedStyle.top - position.y;

                style.top = position.y;
                style.minHeight = diff + layout.height;
            }

            if (resizeEdge.HasFlag(ResizeEdge.Bottom))
            {
                var top = resolvedStyle.top;
                style.minHeight = position.y - top;

            }

            if (resizeEdge.HasFlag(ResizeEdge.Left))
            {
                var diff = resolvedStyle.left - position.x;

                style.left = position.x;
                style.minWidth = diff + layout.width;
            }

            if (resizeEdge.HasFlag(ResizeEdge.Right))
            {
                var left = resolvedStyle.left;
                style.minWidth = position.x - left;
            }
        }


        ResizeEdge CalcEdge(Vector2 localPosition)
        {
            var top = localPosition.y <= resolvedStyle.borderTopWidth;
            var bottom = localPosition.y >= resolvedStyle.height - resolvedStyle.borderBottomWidth;
            var left = localPosition.x <= resolvedStyle.borderLeftWidth;
            var right = localPosition.x >= resolvedStyle.width - resolvedStyle.borderRightWidth;


            var edge = ResizeEdge.None;
            if (top)
            {
                edge |= ResizeEdge.Top;
            }
            else if (bottom)
            {
                edge |= ResizeEdge.Bottom;
            }

            if (left)
            {
                edge |= ResizeEdge.Left;
            }
            else if (right)
            {
                edge |= ResizeEdge.Right;
            }

            return edge;
        }

        CursorType ToCursorType(ResizeEdge edge)
        {
            var type = CursorType.Default;

            var top = edge.HasFlag(ResizeEdge.Top);
            var bottom = edge.HasFlag(ResizeEdge.Bottom);
            var left = edge.HasFlag(ResizeEdge.Left);
            var right = edge.HasFlag(ResizeEdge.Right);

            if (top)
            {
                type = left ? CursorType.ResizeUpLeft : (right ? CursorType.ResizeUpRight : CursorType.ResizeVertical);
            }
            else if (bottom)
            {
                type = left ? CursorType.ResizeUpRight : (right ? CursorType.ResizeUpLeft : CursorType.ResizeVertical);
            }
            else if (left || right)
            {
                type = CursorType.ResizeHorizontal;
            }

            return type;
        }


        void UpdateResizeEdgeAndCursor(Vector2 localPosition)
        {
            resizeEdge = CalcEdge(localPosition);
            SetResizeCursor();
        }

        void SetResizeCursor()
        {
            var cursorType = ToCursorType(resizeEdge);

            CursorManager.SetCursor(cursorType);
        }
    }

    #endregion
}