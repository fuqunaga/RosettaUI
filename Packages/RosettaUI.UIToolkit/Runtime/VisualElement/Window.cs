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
        private bool _closable;
        private IVisualElementScheduledItem _focusTask;

        public bool IsMoved { get; protected set; }

        public VisualElement TitleBarContainerLeft { get; } = new();

        public VisualElement TitleBarContainerRight { get; } = new();


        public Button CloseButton
        {
            get => _closeButton;
            set
            {
                if (_closeButton == value) return;

                if (_closeButton != null)
                {
                    TitleBarContainerRight.Remove(_closeButton);
                }

                _closeButton = value;
                TitleBarContainerRight.Add(_closeButton);
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
                _focusTask?.Pause();
                if (_focused == value) return;

                _focused = value;
                if (_focused) AddToClassList(UssClassNameFocused);
                else RemoveFromClassList(UssClassNameFocused);
            }
        }

        public bool Closable
        {
            get => _closable;
            set
            {
                if (_closable == value) return;
                _closable = value;

                if (_closable)
                {
                    RegisterCallback<KeyDownEvent>(OnKeyDown);

                    if (CloseButton == null)
                    {
                        CloseButton = new WindowTitleButton();
                        CloseButton.clicked += Hide;
                    }

                    CloseButton.visible = true;
                }
                else
                {
                    UnregisterCallback<KeyDownEvent>(OnKeyDown);

                    if (CloseButton != null)
                    {
                        CloseButton.visible = false;
                    }
                }
            }
        }

        public Window() : this(true, true)
        {
        }

        public Window(bool resizable, bool closable)
        {
            this.resizable = resizable;

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = -1;

            AddToClassList(UssClassName);

            _titleBarContainer.AddToClassList(UssClassNameTitleBarContainer);
            TitleBarContainerLeft.AddToClassList(UssClassNameTitleBarContainerLeft);
            TitleBarContainerRight.AddToClassList(UssClassNameTitleBarContainerRight);

            _titleBarContainer.Add(TitleBarContainerLeft);
            _titleBarContainer.Add(TitleBarContainerRight);
            hierarchy.Add(_titleBarContainer);

            _contentContainer.AddToClassList(UssClassNameContentContainer);
            hierarchy.Add(_contentContainer);

            Closable = closable;


            this.AddBoxShadow();

            // ResizeはWindowの少し外側から有効
            // BoxShadowがWindow外のサイズをもつ子供のエレメントなので
            // PointerMoveEventを拾うように有効化する
            if (this.resizable)
            {
                var boxShadow = this.Q<BoxShadow>();
                boxShadow.pickingMode = PickingMode.Position;
                RegisterCallback<PointerMoveEvent>(OnPointerMove);
            }

            RegisterCallback<PointerDownEvent>(OnPointerDownTrickleDown, TrickleDown.TrickleDown);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
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
            if (evt.button != 0) return;

            if (_resizeEdge != ResizeEdge.None)
            {
                StartDrag(DragMode.ResizeWindow);
                evt.StopPropagation();
            }
            // BoxShadowがWindowの範囲より広いのでWindow外のポインタイベントも入ってくる
            else if (layout.Contains(evt.position))
            {
                StartDragWindow(evt.localPosition);
            }
        }

        protected virtual void OnPointerMove(PointerMoveEvent evt)
        {
            if (_dragMode != DragMode.None) return;

            UpdateResizeEdgeAndCursor(evt.localPosition);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (!IsFocused || closeKey == KeyCode.None) return;

            if (evt.keyCode == closeKey && evt.modifiers == closeKeyModifiers)
            {
                Hide();
            }
        }

        // Focusは短時間で複数回変わるケースがあるので様子を見る
        // ・Window内の要素同士でのフォーカスの移動
        // ・PopupFieldをクリックすると一度WindowがFocusになったあとDropdownMenuにフォーカスが移る
        private void OnFocus(FocusEvent evt)
        {
            _focusTask?.Pause();
            _focusTask = schedule.Execute(() => IsFocused = true).StartingIn(50);
        }

        private void OnBlur(BlurEvent evt)
        {
            _focusTask?.Pause();
            _focusTask = schedule.Execute(() => IsFocused = false).StartingIn(50);
        }

        #endregion

        
        #region Drag(DragWindow or Resize)

        private void StartDrag(DragMode dragMode)
        {
            _dragMode = dragMode;
            RegisterPanelCallback();
        }
        
        private void FinishDrag()
        {
            if (_dragMode == DragMode.None) return;
            
            _dragMode = DragMode.None;
            UnregisterPanelCallback();

            CursorManager.ResetCursor();
        }
        
        private void RegisterPanelCallback()
        {
            var root = panel.visualTree;
            root.RegisterCallback<PointerMoveEvent>(OnPointerMoveOnPanel);
            root.RegisterCallback<PointerUpEvent>(OnPointerUpOnPanel);
        }

        private void UnregisterPanelCallback()
        {
            var root = panel?.visualTree;
            if (root == null) return;
            root.UnregisterCallback<PointerMoveEvent>(OnPointerMoveOnPanel);
            root.UnregisterCallback<PointerUpEvent>(OnPointerUpOnPanel);
        }
        
        protected virtual void OnPointerMoveOnPanel(PointerMoveEvent evt)
        {
            if (_dragMode == DragMode.None) return;
            
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


        protected virtual void OnPointerUpOnPanel(PointerUpEvent evt)
        {
            if (evt.button != 0) return;
            FinishDrag();
        }
        
        #endregion


        #region DragWindow

        private bool _beforeDrag;

        private void StartDragWindow(Vector2 localPosition)
        {
            StartDrag(DragMode.DragWindow);
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


        #endregion


        #region Resize Window

        private void UpdateResizeEdgeAndCursor(Vector2 localPosition)
        {
            var prevResizeEdge = _resizeEdge;
            _resizeEdge = CalcEdge(localPosition);

            // _resizeEdgeがNoneではないとき、カーソルを変更
            // _resizeEdgeがNoneのとき、
            //  別のエレメントでカーソルが変わっていた場合はSetResizeCursor()したくない
            //  このWindowがカーソルを変えていた場合は戻したい
            //  →とりあえずprevResizeEdgeがNoneじゃなければ元に戻す
            if (_resizeEdge != ResizeEdge.None || prevResizeEdge != ResizeEdge.None)
            {
                SetResizeCursor();
            }
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
            const float edgeWidthOuter = 4f;
            const float edgeWidthInner = 2f;

            var rect = new Rect() { size = layout.size };
            var outerRect = new Rect()
            {
                min = -Vector2.one * edgeWidthOuter,
                max = rect.size + Vector2.one * edgeWidthOuter
            };
            var innerRect = new Rect()
            {
                min = Vector2.one * edgeWidthInner,
                max = rect.size - Vector2.one * edgeWidthInner
            };

            if (!outerRect.Contains(localPosition)) return ResizeEdge.None;

            var top = localPosition.y <= innerRect.yMin;
            var bottom = localPosition.y >= innerRect.yMax;
            var left = localPosition.x <= innerRect.xMin;
            var right = localPosition.x >= innerRect.xMax;

            var edge = ResizeEdge.None;
            if (top) edge |= ResizeEdge.Top;
            if (bottom) edge |= ResizeEdge.Bottom;
            if (left) edge |= ResizeEdge.Left;
            if (right) edge |= ResizeEdge.Right;
            
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


        private void SetResizeCursor()
        {
            var cursorType = ToCursorType(_resizeEdge);
            CursorManager.SetCursor(cursorType);
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
            FinishDrag();
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