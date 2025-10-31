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
        
        
        public static Vector2 minSize = Vector2.one * 30f;

        
        public readonly bool resizable;

        public event Action onShow;
        
        /// <summary>
        /// Called when the window is hidden (for example, by pressing the close button or the Escape key).
        /// The argument is true if the window was cancelled by the Escape key, or false if it was closed by the close button.
        /// </summary>
        public event Action<bool> onHide;

        protected VisualElement dragRoot;

        private readonly VisualElement _titleBarContainer = new();
        private readonly VisualElement _contentContainer = new();

        private Button _closeButton;
        private DragMode _dragMode;
        private Vector2 _draggingLocalPosition;
        private bool _focused;
        private bool _closable;
        private IVisualElementScheduledItem _focusTask;
        private IVisualElementScheduledItem _freezeFixedSizeTask;

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
                    RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);

                    if (CloseButton == null)
                    {
                        CloseButton = new WindowTitleButton()
                        {
                            tabIndex = -1
                        };
                        CloseButton.clicked += Hide;
                    }

                    CloseButton.style.display = DisplayStyle.Flex;
                }
                else
                {
                    UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);

                    if (CloseButton != null)
                    {
                        CloseButton.style.display = DisplayStyle.None;
                    }
                }
            }
        }

        public Window() : this(true, true)
        {
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public Window(bool resizable, bool closable)
        {
            this.resizable = resizable;

            // ReSharper disable once VirtualMemberCallInConstructor
            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = -1;

            AddToClassList(UssClassName);

            _titleBarContainer.AddToClassList(UssClassNameTitleBarContainer);
            TitleBarContainerLeft.AddToClassList(UssClassNameTitleBarContainerLeft);
            TitleBarContainerRight.AddToClassList(UssClassNameTitleBarContainerRight);

            if (this.resizable)
            {
                InitResizeHandles();
            }
            
            _titleBarContainer.Add(TitleBarContainerLeft);
            _titleBarContainer.Add(TitleBarContainerRight);
            hierarchy.Add(_titleBarContainer);

            _contentContainer.AddToClassList(UssClassNameContentContainer);
            hierarchy.Add(_contentContainer);

            Closable = closable;
    
            this.AddBoxShadow();

            RegisterCallback<PointerDownEvent>(OnPointerDownTrickleDown, TrickleDown.TrickleDown);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<FocusEvent>(OnFocus, TrickleDown.TrickleDown);
            RegisterCallback<BlurEvent>(OnBlur, TrickleDown.TrickleDown);
            RegisterCallback<RequestResizeWindowEvent>(evt =>
            {
                ResetFixedSize();
                evt.StopPropagation();
            });
            
            this.AddManipulator(new FocusTrapManipulator());
            
            ResetFixedSize();
        }
        
            // 8方向にリサイズ用の透明なハンドルを追加する
        private void InitResizeHandles()
        {
            Span<ResizeEdge> edges = stackalloc ResizeEdge[]
            {
                ResizeEdge.Top,
                ResizeEdge.Bottom,
                ResizeEdge.Left,
                ResizeEdge.Right,
                ResizeEdge.Top | ResizeEdge.Left,
                ResizeEdge.Top | ResizeEdge.Right,
                ResizeEdge.Bottom | ResizeEdge.Left,
                ResizeEdge.Bottom | ResizeEdge.Right,
            };
            
            var handleContainer = new VisualElement()
            {
                name = "resize-handle-container",
                pickingMode = PickingMode.Ignore
            };
            handleContainer.AddToClassList("rosettaui-window__resize-handle-container");
            
            foreach (var edge in edges)
            {
                var handle = CreateHandle(edge);
                handleContainer.Add(handle);
            }
            
            hierarchy.Add(handleContainer);

            return;

            VisualElement CreateHandle(ResizeEdge edge)
            {
                var edgeName = edge.ToString().Replace(", ", "-").ToLower();
                
                // horizontal or vertical or corner
                var edgeTypeName = edge switch
                {
                    ResizeEdge.Top or ResizeEdge.Bottom => "horizontal",
                    ResizeEdge.Left or ResizeEdge.Right => "vertical",
                    _ => "corner"
                };
                
                var handle = new VisualElement
                {
                    name = $"resize-handle-{edgeName}",
                    pickingMode = PickingMode.Position
                };
                handle.AddToClassList("rosettaui-window__resize-handle");
                handle.AddToClassList($"rosettaui-window__resize-handle-{edgeTypeName}");
                handle.AddToClassList($"rosettaui-window__resize-handle-{edgeName}");

                var manipulator = new DragManipulator(
                    null,
                    (e) => OnHandleDrag(e, edge),
                    OnHandleDragEnd
                );
                manipulator.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
                handle.AddManipulator(manipulator);
                
                return handle;
            }
        }

        // サイズ固定
        // ほぼルートのWindowエレメントのサイズが不定だとレイアウトの計算がめちゃくちゃ重い
        // これを回避するためWindowは内容物のレイアウトが落ち着いたらサイズを固定しておく
        // 同一フレームだとGeometryChangedEventなどでサイズが変わるやつがいるので１フレーム待つ
        private void ResetFixedSize()
        {
            FreeFixedSize();
            
            var startFrameCount = Time.frameCount;
            _freezeFixedSizeTask?.Pause();
            _freezeFixedSizeTask = schedule.Execute(() =>
            {
                if (Time.frameCount <= startFrameCount) return;
                FreezeFixedSize();
            }).Until(() => Time.frameCount > startFrameCount);
        }

        private void FreezeFixedSize(bool fixHeight = false)
        {
            style.width = layout.width;
            style.minWidth = StyleKeyword.Null;

            // heightは基本的に内容物に応じるようにして固定しない
            // しかし手動でResizeした場合は強制的に固定する
            // そうしないとDragが終わった途端広がったりして変
            if (fixHeight)
            {
                style.height = layout.height;
            }
        }
        
        private void FreeFixedSize()
        {
            // 現状のサイズは保持。拡大しかしない。勝手に縮小されるのは違和感があるが拡大されるのは内容物で膨らんだ形で問題ない印象
            style.minWidth = layout.width; 
            style.width = StyleKeyword.Null;

            style.height = StyleKeyword.Null;
        }


        #region Event

        protected virtual void OnPointerDownTrickleDown(PointerDownEvent evt)
        {
            BringToFront();
        }

        protected virtual void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;
            StartDragWindow(evt.localPosition);
        }

        protected virtual void OnNavigationCancel(NavigationCancelEvent evt)
        {
            Hide(isCancelled: true);
        }

        // Focusは短時間で複数回変わるケースがあるので様子を見る
        // ・Window内の要素同士でのフォーカスの移動
        // ・PopupFieldをクリックすると一度WindowがFocusになったあとDropdownMenuにフォーカスが移る
        protected virtual void OnFocus(FocusEvent evt)
        {
            _focusTask?.Pause();
            _focusTask = schedule.Execute(() => IsFocused = true);
        }

        protected virtual void OnBlur(BlurEvent evt)
        {
            var relatedTarget = evt.relatedTarget;
            
            _focusTask?.Pause();
            _focusTask = schedule.Execute(() =>
            {
                // DropdownMenuはこのWindowの子ではないが見た目上子供のような挙動が自然
                // 簡易的にWindow内ではないVisualElementはDropdownMenu（などのポップアップ）と判定
                // DropdownMenu中はIsFocused==falseにせずに見た目をキープし、
                // DropdownMenuが削除されたらFocus()で正しいフォーカス状態（キー入力を受け付ける）に戻す
                if (relatedTarget is VisualElement visualElement && !IsInWindow(visualElement))
                {
                    visualElement.RegisterCallback<DetachFromPanelEvent>(_ => Focus());
                }
                else
                {
                    IsFocused = false;
                }
            });
            
            return;

            static bool IsInWindow(VisualElement visualElement)
            {
                return visualElement switch
                {
                    null => false,
                    Window => true,
                    _ => IsInWindow(visualElement.parent)
                };
            }
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
            
            if (_dragMode == DragMode.ResizeWindow)
            {
                FreezeFixedSize(fixHeight: true);
            }
            
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
                        // UpdateResizeWindow(evt.position);
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

                const float distanceToActivation = 10f;
                const float distanceToActivationSqr = distanceToActivation * distanceToActivation;
                if (Vector2.SqrMagnitude(diff) <  distanceToActivationSqr)
                {
                    return;
                }
                
                _beforeDrag = false;
            }

            Position = pos;
        }


        #endregion


        #region Resize Window
        
        private void OnHandleDrag(PointerMoveEvent evt, ResizeEdge edge)
        {
            UpdateResizeWindow(evt.position, edge);
            evt.StopPropagation();
        }
        
        private void OnHandleDragEnd(EventBase _)
        {
            FreezeFixedSize(fixHeight: true);
        }
        
        private void UpdateResizeWindow(Vector2 position, ResizeEdge edge)
        {
            if (edge.HasFlag(ResizeEdge.Top))
            {
                var diff = resolvedStyle.top - position.y;

                style.top = position.y;
                style.height =  Mathf.Max(diff + layout.height, minSize.y);
            }

            if (edge.HasFlag(ResizeEdge.Bottom))
            {
                var top = resolvedStyle.top;
                style.height = Mathf.Max(position.y - top, minSize.y);
            }

            if (edge.HasFlag(ResizeEdge.Left))
            {
                var diff = resolvedStyle.left - position.x;

                style.left = position.x;
                style.width = Mathf.Max(diff + layout.width, minSize.x);
            }

            if (edge.HasFlag(ResizeEdge.Right))
            {
                var left = resolvedStyle.left;
                style.width = Mathf.Max(position.x - left, minSize.x);
            }
        }

        #endregion


        public virtual void Show()
        {
            style.display = DisplayStyle.Flex;
            Focus();
            onShow?.Invoke();
        }

        public virtual void Hide() => Hide(false);
        
        public virtual void Hide(bool isCancelled)
        {
            style.display = DisplayStyle.None;
            FinishDrag();
            onHide?.Invoke(isCancelled);
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
    }
}