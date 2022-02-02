using System;
using RosettaUI.Reactive;
using UnityEngine;
using UnityEngine.UIElements;


namespace RosettaUI.UIToolkit
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


        public readonly bool resizable;

        static readonly string UssClassName = "rosettaui-window";
        static readonly string UssClassNameTitleBarContainer = UssClassName + "__titlebar-container";
        static readonly string UssClassNameTitleBarContainerLeft = UssClassNameTitleBarContainer + "__left";
        static readonly string UssClassNameTitleBarContainerRight = UssClassNameTitleBarContainer + "__right";
        static readonly string UssClassNameContentContainer = UssClassName + "__content-container";
       
        readonly VisualElement _titleBarContainer = new();
        readonly VisualElement _contentContainer = new();
        

        Button _closeButton;

        DragMode _dragMode;

        Vector2 _draggingLocalPosition;

        ResizeEdge _resizeEdge;
        
        public bool IsMoved { get; protected set; }

        public VisualElement TitleBarContainerLeft { get; } = new();

        public VisualElement TitleBarContainerRight { get; } = new();


        public Button CloseButton
        {
            get => _closeButton;
            set
            {
                if (_closeButton != value)
                {
                    if (_closeButton != null)
                    {
                        TitleBarContainerRight.Remove(_closeButton);
                    }

                    _closeButton = value;
                    TitleBarContainerRight.Add(_closeButton);
                }
            }
        }

        public override VisualElement contentContainer => _contentContainer;

        public Window(bool resizable = true)
        {
            this.resizable = resizable;

            AddToClassList(UssClassName);

            _titleBarContainer.AddToClassList(UssClassNameTitleBarContainer);
            TitleBarContainerLeft.AddToClassList(UssClassNameTitleBarContainerLeft);
            TitleBarContainerRight.AddToClassList(UssClassNameTitleBarContainerRight);
            
            _titleBarContainer.Add(TitleBarContainerLeft);
            _titleBarContainer.Add(TitleBarContainerRight);
            hierarchy.Add(_titleBarContainer);

            CloseButton = new WindowTitleButton();
            CloseButton.clicked += Hide;
            
            _contentContainer.AddToClassList(UssClassNameContentContainer);
            hierarchy.Add(_contentContainer);

            RegisterCallback<PointerDownEvent>(OnPointerDownTrickleDown, TrickleDown.TrickleDown);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseOutEvent>(OnMouseOut);
        }


        #region Event

        protected virtual void OnPointerDownTrickleDown(PointerDownEvent evt)
        {
            BringToFront();
        }

        protected virtual void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 0)
            {
                if (_resizeEdge != ResizeEdge.None)
                {
                    StartResizeWindow();
                }
                else
                {
                    StartDragWindow(evt.localPosition);
                }

                RegisterPanelCallback();
                evt.StopPropagation();
            }
        }

        protected virtual void OnPointerMoveOnRoot(PointerMoveEvent evt)
        {
            if (_dragMode != DragMode.None)
            {
                // 画面外でボタンをUpされた場合検知できないので、現在押下中かどうかで判定する
                if ((evt.pressedButtons & 0x1) != 0)
                {
                    switch (_dragMode)
                    {
                        case DragMode.DragWindow:
                            UpdateDragWindow(evt.position);
                            break;

                        case DragMode.ResizeWindow:
                            UpdateResizeWindow(evt.position);
                            break;
                        
                        case DragMode.None:
                        default:
                            break;
                    }
                }
                else
                {
                    FinishDrag();
                }
            }
        }


        protected virtual void OnPointerUpOnRoot(PointerUpEvent evt)
        {
            if (evt.button == 0)
            {
                _dragMode = DragMode.None;
                FinishDrag();
            }
        }


        protected virtual void OnMouseMove(MouseMoveEvent evt)
        {
            if (resizable && _dragMode == DragMode.None)
            {
                UpdateResizeEdgeAndCursor(evt.localMousePosition);
            }
        }

        protected virtual void OnMouseOut(MouseOutEvent evt)
        {
            if (_dragMode != DragMode.ResizeWindow)
            {
                CursorManager.ResetCursor();
            }
        }

        #endregion


        void FinishDrag()
        {
            _dragMode = DragMode.None;
            UnregisterPanelCallback();

            ResetCursor();
        }

        void ResetCursor()
        {
            if (_resizeEdge != ResizeEdge.None)
            {
                CursorManager.ResetCursor();
            }
        }

        #region DragWindow

        void StartDragWindow(Vector2 localPosition)
        {
            _dragMode = DragMode.DragWindow;
            _draggingLocalPosition = localPosition;
        }


        void UpdateDragWindow(Vector2 position)
        {
            var pos = position - _draggingLocalPosition;

            style.left = pos.x;
            style.top = pos.y;

            IsMoved = true;
        }


        void RegisterPanelCallback()
        {
            var root = panel.visualTree;
            root.RegisterCallback<PointerMoveEvent>(OnPointerMoveOnRoot);
            root.RegisterCallback<PointerUpEvent>(OnPointerUpOnRoot);
        }

        protected void UnregisterPanelCallback()
        {
            var root = panel?.visualTree;
            if (root != null)
            {
                root.UnregisterCallback<PointerMoveEvent>(OnPointerMoveOnRoot);
                root.UnregisterCallback<PointerUpEvent>(OnPointerUpOnRoot);
            }
        }

        #endregion

        #region Resize Window

        void StartResizeWindow()
        {
            _dragMode = DragMode.ResizeWindow;
        }

        private void UpdateResizeWindow(Vector2 position)
        {
            SetResizeCursor();


            if (_resizeEdge.HasFlag(ResizeEdge.Top))
            {
                var diff = resolvedStyle.top - position.y;

                style.top = position.y;
                style.minHeight = diff + layout.height;
            }

            if (_resizeEdge.HasFlag(ResizeEdge.Bottom))
            {
                var top = resolvedStyle.top;
                style.minHeight = position.y - top;
            }

            if (_resizeEdge.HasFlag(ResizeEdge.Left))
            {
                var diff = resolvedStyle.left - position.x;

                style.left = position.x;
                style.minWidth = diff + layout.width;
            }

            if (_resizeEdge.HasFlag(ResizeEdge.Right))
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

        static CursorType ToCursorType(ResizeEdge edge)
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
            _resizeEdge = CalcEdge(localPosition);
            SetResizeCursor();
        }

        void SetResizeCursor()
        {
            var cursorType = ToCursorType(_resizeEdge);

            // カーソルを元に戻すのはOnMouseOutイベントで行う
            // ここので元に戻すと子要素でカーソルを変化させてもここで上書きしてしまう
            if (cursorType != CursorType.Default)
            {
                CursorManager.SetCursor(cursorType);
            }
        }

        #endregion

        
        public virtual void Show()
        {
            style.display = DisplayStyle.Flex;
        }

        public virtual void Hide()
        {
            style.display = DisplayStyle.None;
        }
        

        protected virtual VisualElement SelfRoot => this;


        public virtual void Show(Vector2 position, VisualElement target)
        {
            var root = target.panel.visualTree.Q<TemplateContainer>();

            if (root == null)
            {
                Debug.LogError("Could not find rootVisualContainer...");
                return;
            }

            root.Add(SelfRoot);

            var local = root.WorldToLocal(position);
            style.left = local.x - root.layout.x;
            style.top = local.y - root.layout.y;

            //schedule.Execute(EnsureVisibilityInParent);

            /*
            if (targetElement != null)
                targetElement.pseudoStates |= PseudoStates.Active;
            */
            Show();
        }

        public void EnsureVisibilityInParent()
        {
            var root = panel?.visualTree;
            if (root != null && !float.IsNaN(layout.width) && !float.IsNaN(layout.height))
            {
                //if (m_DesiredRect == Rect.zero)
                {
                    var posX = Mathf.Min(layout.x, root.layout.width - layout.width);
                    var posY = Mathf.Min(layout.y, Mathf.Max(0, root.layout.height - layout.height));

                    style.left = posX;
                    style.top = posY;
                }

                style.height = Mathf.Min(
                    root.layout.height - root.layout.y - layout.y,
                    layout.height + resolvedStyle.borderBottomWidth + resolvedStyle.borderTopWidth);

                /*
                if (m_DesiredRect != Rect.zero)
                {
                    m_OuterContainer.style.width = m_DesiredRect.width;
                }
                */
            }
        }
    }
}