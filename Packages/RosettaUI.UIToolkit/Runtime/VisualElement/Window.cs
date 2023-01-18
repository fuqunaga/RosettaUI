using System;
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

        private const string UssClassName = "rosettaui-window";
        private const string UssClassNameFocused = UssClassName + "--focused";
        private const string UssClassNameTitleBarContainer = UssClassName + "__titlebar-container";
        private const string UssClassNameTitleBarContainerLeft = UssClassNameTitleBarContainer + "__left";
        private const string UssClassNameTitleBarContainerRight = UssClassNameTitleBarContainer + "__right";
        private const string UssClassNameContentContainer = UssClassName + "__content-container";

        public static KeyCode closeKey = KeyCode.Q;
        public static EventModifiers closeKeyModifiers = EventModifiers.None;

        public readonly bool resizable;
        
        public event Action onShow;
        public event Action onHide;
        
        protected VisualElement dragRoot;

        private readonly VisualElement _titleBarContainer = new();
        private readonly VisualElement _contentContainer = new();

        private Button _closeButton;
        private DragMode _dragMode;
        private Vector2 _draggingLocalPosition;
        private ResizeEdge _resizeEdge;
        private bool _focused;
        
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
        
        protected virtual VisualElement SelfRoot => this;

        public override VisualElement contentContainer => _contentContainer;

        public Vector2 Position
        {
            get
            {
                var rs = resolvedStyle;
                return new Vector2(rs.left, rs.top);
            }
            set
            {
                if (Position != value)
                {
                    style.left = value.x;
                    style.top = value.y;
                    IsMoved = true;
                }
            }
        }

        public bool IsFocused
        {
            get => _focused;
            protected set
            {
                if (_focused == value) return;
                
                _focused = value;
                if ( _focused) AddToClassList(UssClassNameFocused);
                else RemoveFromClassList(UssClassNameFocused);
            }
        }

        public Window() : this(true)
        {
        }

        public Window(bool resizable)
        {
            this.resizable = resizable;

            focusable = true;
            pickingMode = PickingMode.Position;

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
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<FocusEvent>(OnFocus, TrickleDown.TrickleDown);
            RegisterCallback<BlurEvent>(OnBlur, TrickleDown.TrickleDown);

            
            RegisterCallback<ChangeVisibleEvent>(_ =>
            {
                style.width = StyleKeyword.Null;
                ResetFixedSize();
            });
            
            // Focusable.ExecuteDefaultEvent() 内の this.focusController?.SwitchFocusOnEvent(evt) で
            // NavigationMoveEvent 方向にフォーカスを移動しようとする
            // キー入力をしている場合などにフォーカスが移ってしまうのは避けたいのでWindow単位で抑制しておく
            // UnityデフォルトでもTextFieldは抑制できているが、IntegerField.inputFieldでは出来ていないなど挙動に一貫性がない
            RegisterCallback<NavigationMoveEvent>(evt => evt.PreventDefault());

            this.AddBoxShadow();
            
            ResetFixedSize();
        }

        // 幅固定
        // ほぼルートのWindowエレメントのサイズが不定だとレイアウトの計算がめちゃくちゃ重い
        // 特にHorizontal方向はほとんどのエレメントが固定サイズを持っていないので再計算が走りまくるようで重い
        // これを回避するためWindowは内容物のレイアウトが落ち着いたら幅を固定しておく
        // 同一フレームだとGeometryChangedEventなどでサイズが変わるやつがいるので１フレーム待つ
        private void ResetFixedSize()
        {
            var startFrameCount = Time.frameCount;
            schedule.Execute(() =>
            {
                if (Time.frameCount <= startFrameCount) return;
                style.width = layout.width;
            }).Until(() => Time.frameCount > startFrameCount); 
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
        
        
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!IsFocused || hideKey == KeyCode.None) return;

            if (evt.keyCode == hideKey && evt.modifiers == hideKeyModifiers)
            {
                Hide();
            }
        }
        
        private void OnFocus(FocusEvent evt)
        {
            IsFocused = true;
        }
        
        private void OnBlur(BlurEvent evt)
        {
            IsFocused = false;
        }

        #endregion


        private void FinishDrag()
        {
            _dragMode = DragMode.None;
            UnregisterPanelCallback();

            ResetCursor();
        }

        private void ResetCursor()
        {
            if (_resizeEdge != ResizeEdge.None)
            {
                CursorManager.ResetCursor();
            }
        }

        #region DragWindow

        private bool _beforeDrag;

        private void StartDragWindow(Vector2 localPosition)
        {
            _dragMode = DragMode.DragWindow;
            _draggingLocalPosition = localPosition;

            _beforeDrag = true;
        }

        private Vector2 WorldToDragRootLocal(Vector2 worldPosition)
        {
            return dragRoot?.WorldToLocal(worldPosition) ?? worldPosition;
        }


        private void UpdateDragWindow(Vector2 worldPosition)
        {
            var localPosition = WorldToDragRootLocal(worldPosition);
            var pos = localPosition - _draggingLocalPosition;
            
            // ListView の reorderable でアイテムをドラッグするときに Window が少し動いてしまう問題対策
            // ListView は一定距離 PointerMove で移動しないとドラッグ判定にはならないためその間 Window のドラッグが成立してしまう
            // Window も遊びを入れることで対処
            // refs: https://github.com/Unity-Technologies/UnityCsReference/blob/9b50c8698e84387d83073f53e07524cfc31dd919/ModuleOverrides/com.unity.ui/Core/DragAndDrop/DragEventsProcessor.cs#L204-L205
            if (_beforeDrag)
            {
                var current = new Vector2(resolvedStyle.left, resolvedStyle.top);
                var diff = pos - current;

                if (Mathf.Abs(diff.x) < 5f && Mathf.Abs(diff.y) < 5f) return;
                _beforeDrag = false;
            }

            Position = pos;
        }


        private void RegisterPanelCallback()
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

        private void StartResizeWindow()
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


        private ResizeEdge CalcEdge(Vector2 localPosition)
        {
            const float edgeWidth = 4f;
            var top = localPosition.y <= edgeWidth;
            var bottom = localPosition.y >= resolvedStyle.height - edgeWidth;
            var left = localPosition.x <= edgeWidth;
            var right = localPosition.x >= resolvedStyle.width - edgeWidth;

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

        private static CursorType ToCursorType(ResizeEdge edge)
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


        private void UpdateResizeEdgeAndCursor(Vector2 localPosition)
        {
            _resizeEdge = CalcEdge(localPosition);
            SetResizeCursor();
        }

        private void SetResizeCursor()
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
            Focus();
            onShow?.Invoke();
        }

        public virtual void Hide()
        {
            style.display = DisplayStyle.None;
            onHide?.Invoke();
        }

        public virtual void Show(VisualElement target)
        {
            SearchDragRootAndAdd(target);
            Show();
        }

        public virtual void Show(Vector2 position, VisualElement target)
        {
            SearchDragRootAndAdd(target);

            var local = dragRoot.WorldToLocal(position);
            Position = local - dragRoot.layout.position;
            IsMoved = false;

            //schedule.Execute(EnsureVisibilityInParent);

            Show();
        }

        private void SearchDragRootAndAdd(VisualElement target)
        {
            dragRoot = target.panel.visualTree.Q<TemplateContainer>()
                       ?? target.panel.visualTree.Query(null, RosettaUIRootUIToolkit.USSRootClassName).First();
            
            dragRoot.Add(SelfRoot);
        }


#if false
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
#endif
    }
}