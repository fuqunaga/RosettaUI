using System;
using System.Linq;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// A visual element that allows you to edit an <see cref="AnimationCurve"/>.
    /// </summary>
    public partial class AnimationCurveEditor : ModalEditor<AnimationCurve>
    {
        #region Static Window Management
        
        private static AnimationCurveEditor _instance;
        
        static AnimationCurveEditor()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(() =>
            {
                _instance = null;
            });
        }

        public static void Show(Vector2 position, VisualElement target, AnimationCurve initialCurve, Action<AnimationCurve> onCurveChanged)
        {
            _instance ??= new AnimationCurveEditor();;
            
            _instance.Show(position, target, onCurveChanged);
            
            _instance.CopiedValue = initialCurve;
            
            // SetCurve()内のFitViewToCurve()時はまだ表示領域のサイズが確定していないため、
            // GeometryChangedEventでFitViewToCurve()を呼び出す
            _instance.RegisterCallbackOnce<GeometryChangedEvent>(_ =>
            {
                _instance.FitViewToCurve();
            });
        }

        #endregion
        
        
        public const string USSClassName = "rosettaui-animation-curve-editor";
        private const string PersistentSizeKey = "RosettaUI_AnimationCurveEditor_Size";
        
        // ReSharper disable MemberCanBePrivate.Global
        public static Vector2 DefaultWindowSize { get; set; } = new(800, 500);
        public static string VisualTreeAssetName { get; set; } = "RosettaUI_AnimationCurveEditor";
        public static RectOffset FitViewPaddingPixel { get; } = new(24, 24, 40, 20);
        // ReSharper restore MemberCanBePrivate.Global
        
        private readonly CurveController _curveController;
        
        private VisualElement _curvePreviewElement;
        private VisualElementKeyEventHelper _keyEventHelper;
        private ToggleButton _snapXButton;
        private ToggleButton _snapYButton;
        private ScrollerController _scrollerController;
        private AxisLabelController _axisLabelController;
        private SelectedControlPointsRect _selectedControlPointsRect;
        private SelectionRect _selectionRect;
        private ControlPointDisplayPositionPopup _controlPointDisplayPositionPopup;
        private ControlPointsEditPositionPopup _controlPointsEditPositionPopup;

        private PreviewTransform _previewTransform;
        private readonly ControlPointsDragManipulatorSource _controlPointsDragManipulatorSource;
        
        private bool _prevSnapX;
        private bool _prevSnapY;

        private Vector2 _zoomStartPosition;
        private float _lastPointerDownTime;
        
        #region ModalEditor

        protected override AnimationCurve CopiedValue
        {
            get => AnimationCurveHelper.Clone(_curveController.Curve);
            set => SetCurve(AnimationCurveHelper.Clone(value));
        }

        #endregion


        private AnimationCurveEditor() : base(PersistentSizeKey, DefaultWindowSize, VisualTreeAssetName)
        {
            AddToClassList(USSClassName);
            
            var controlPointContainer = this.Q("control-point-container");
            
            _curveController = new CurveController(controlPointContainer, CreateControlPoint);
            _curveController.onCurveChanged += () =>
            {
                UpdateView();
                NotifyEditorValueChanged();
            };
            _curveController.onControlPointSelectionChanged += () => _selectedControlPointsRect.UpdateView();
            
            InitUI();
            InitPresetsUI();
            SetupKeyBindings();
            
            _controlPointsDragManipulatorSource = new ControlPointsDragManipulatorSource(
                _curveController,
                _previewTransform,
                _controlPointDisplayPositionPopup
            );
            
            _selectedControlPointsRect.AddManipulator(_controlPointsDragManipulatorSource.CreateManipulator());
            
            return;
            
            
            ControlPoint CreateControlPoint(CurveController curveController)
            {
                var controlPoint = new ControlPoint(
                    curveController,
                    _previewTransform,
                    showPopupMenu: (position, cp) => ControlPointsPopupMenu.Show(
                        position, 
                        _curveController.SelectedControlPointsEditor,
                        () => ShowControlPointsEditPositionPopup(cp.LocalPosition), 
                        cp)
                );
                controlPoint.AddManipulator(_controlPointsDragManipulatorSource.CreateManipulator());
                return controlPoint;
            }
        }

        /// <summary>
        /// Set the curve to be edited.
        /// </summary>
        private void SetCurve(AnimationCurve curve)
        {
            _curveController.SetCurve(curve);
            UnselectAllControlPoint();
            FitViewToCurve();
        }
        
        #region Initialization
        
        private void InitUI()
        {
            // Buttons
            var fitButton = this.Q<Button>("fit-curve-button");
            fitButton.clicked += FitViewToCurve;
            _snapXButton = this.Q<ToggleButton>("time-snapping-button");
            _snapYButton = this.Q<ToggleButton>("value-snapping-button");
            
            // Curve Preview
            _curvePreviewElement = this.Q("preview-back");
            _curvePreviewElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _curvePreviewElement.RegisterCallback<WheelEvent>(OnWheel);
            _curvePreviewElement.RegisterCallback<GeometryChangedEvent>(_ => UpdateView());

            // Curve Preview Transform
            _previewTransform = new PreviewTransform(
                _curvePreviewElement,
                () => (_snapXButton.value, _snapYButton.value)
            );
            
            // Axis Labels & Scroller
            _axisLabelController = new AxisLabelController(this);
            _scrollerController = new ScrollerController(this,
                yCenter =>
                {
                    _previewTransform.SetYCenter(yCenter);
                    _curveController.UpdateView();
                    UpdateCurvePreview();
                    _axisLabelController.UpdateAxisLabel(_previewTransform.PreviewRect);
                },
                xCenter =>
                {
                    _previewTransform.SetXCenter(xCenter);
                    _curveController.UpdateView();
                    UpdateCurvePreview();
                    _axisLabelController.UpdateAxisLabel(_previewTransform.PreviewRect);
                }
            );
            
            // Selected Control Points Rect
            _selectedControlPointsRect = new SelectedControlPointsRect(_curveController, _previewTransform);
            _curvePreviewElement.Add(_selectedControlPointsRect);
            
            // Selection Rect
            _selectionRect = new SelectionRect();
            _curvePreviewElement.Add(_selectionRect);
            
            // Parameter Popup
            var curveGroup = this.Q("curve-group");
            _controlPointDisplayPositionPopup = new ControlPointDisplayPositionPopup(_previewTransform);
            curveGroup.Add(_controlPointDisplayPositionPopup);
            
            // Edit Key Popup
            _controlPointsEditPositionPopup = new ControlPointsEditPositionPopup();
            curveGroup.Add(_controlPointsEditPositionPopup);
        }

        private void SetupKeyBindings()
        {
            _keyEventHelper = new VisualElementKeyEventHelper(this);
            _keyEventHelper.RegisterKeyAction(new [] { KeyCode.LeftControl, KeyCode.LeftCommand, KeyCode.RightControl, KeyCode.RightCommand }, evt =>
            {
                switch (evt)
                {
                    case KeyEventType.KeyDown:
                        _prevSnapX = _snapXButton.value;
                        _prevSnapY = _snapYButton.value;
                        _snapXButton.SetValueWithoutNotify(true);
                        _snapYButton.SetValueWithoutNotify(true);
                        break;
                    case KeyEventType.KeyUp:
                        _snapXButton.SetValueWithoutNotify(_prevSnapX);
                        _snapYButton.SetValueWithoutNotify(_prevSnapY);
                        break;
                    case KeyEventType.KeyPress:
                    default:
                        break;
                }
            });
            _keyEventHelper.RegisterKeyAction(KeyCode.A, evt =>
            {
                if (evt != KeyEventType.KeyDown) return;
                FitViewToCurve();
            });
            _keyEventHelper.RegisterKeyAction(KeyCode.F, evt =>
            {
                if (evt != KeyEventType.KeyDown) return;
                FitViewToSelection();
            });

            _keyEventHelper.RegisterKeyAction(KeyCode.Delete, evt =>
            {
                if (evt != KeyEventType.KeyDown) return;
                _curveController.SelectedControlPointsEditor.RemoveAll();
            });

            _keyEventHelper.RegisterKeyAction(KeyCode.Return, evt =>
            {
                if (evt != KeyEventType.KeyDown) return;
                
                var firstControlPoint = _curveController.SelectedControlPoints.FirstOrDefault();
                if (firstControlPoint != null)
                {
                    // 矩形範囲選択（複数選択）してたらその中心
                    // そうでなければ単数選択なのでControlPointの中心
                    var pos = _selectedControlPointsRect.IsShown()
                        ? _selectedControlPointsRect.layout.center
                        : firstControlPoint.GetLocalPosition();

                    ShowControlPointsEditPositionPopup(pos);
                }
            });

        }
        
        #endregion
        
        #region View Update
        
        private void FitViewToCurve() => FitViewToCurve(..);

        private void FitViewToCurve(Range　range)
        {
            var rect = _curveController.Curve.GetCurveRect(range);
            
            _previewTransform.FitToRect(rect, FitViewPaddingPixel);
            UpdateView();
        }
        
        
        // Fit view to the selected control points
        // 一つもない場合はFitToCurve
        // 一つだけの場合は左右のKeyframeを選択範囲にする
        private void FitViewToSelection()
        {
            var controlPoints = _curveController.ControlPoints;
            var firstIndex = controlPoints.FindIndex(cp => cp.IsActive);
            var lastIndex = controlPoints.FindLastIndex(cp => cp.IsActive);
            
            // no selection
            if (firstIndex < 0)
            {
                FitViewToCurve();
                return;
            }
            
            // single selection
            // select both side keyframes if possible
            if (firstIndex == lastIndex)
            {
                firstIndex = Mathf.Max(0, firstIndex - 1);
                lastIndex = Mathf.Min(controlPoints.Count - 1, lastIndex + 1);
            }

            var range = firstIndex..(lastIndex + 1);
            FitViewToCurve(range);
        }
        
        private void UpdateView()
        {
            var rect = _previewTransform.PreviewRect;
            _scrollerController.UpdateScroller(rect, _curveController.Curve.GetCurveRect());
            _axisLabelController.UpdateAxisLabel(rect);
            _curveController.UpdateView();
            _selectedControlPointsRect.UpdateView();
            UpdateCurvePreview();
        }
        
        private void UpdateCurvePreview()
        {
            var curve = _curveController.Curve;
            
            var gridViewport = _previewTransform.PreviewGridViewport;
            var viewInfo = new AnimationCurvePreviewRenderer.CurvePreviewViewInfo()
            {
                offsetZoom = _previewTransform.OffsetZoom,
                wrapEnabled = curve.length > 0,
                gridEnabled = true,
                gridParams = new Vector4(gridViewport.XOrder, gridViewport.YOrder, gridViewport.XTick, gridViewport.YTick),
            };
            
            AnimationCurveVisualElementHelper.UpdatePreviewToBackgroundImage(curve, _curvePreviewElement, viewInfo);
        }
        
        #endregion
        
        #region Control Point
 
        private void AddControlPoint(Vector2 keyFramePosition)
        {
            UnselectAllControlPoint();
            _curveController.AddKey(keyFramePosition);
        }
        
        private void UnselectAllControlPoint()
        {
            _curveController.UnselectAllControlPoints();
            _controlPointDisplayPositionPopup.Hide();
            _controlPointsEditPositionPopup.Hide();
        }

        private void ShowControlPointsEditPositionPopup(Vector2 localPosition)
        {
            _controlPointsEditPositionPopup.Show(localPosition, _curveController.SelectedControlPointsEditor, this);
        }
        
        #endregion
        
        #region Pointer Events
        private void OnPointerDown(PointerDownEvent evt)
        {
            var button = evt.button;
            switch (button)
            {
                // Zoom
                // Alt + Right button
                case 1 when evt.altKey:
                    _zoomStartPosition = evt.localPosition;
                    StartDrag();
                    break;

                // Pan
                // Middle button or Alt + Left button
                case 2:
                case 0 when evt.altKey:
                    StartDrag();
                    break;
            
                
                // Left button
                // single click: Start selection rect
                // double click: Add control point
                case 0:
                    // Add control point if double click
                    if (Time.realtimeSinceStartup - _lastPointerDownTime < 0.3f)
                    {
                        AddControlPoint(_previewTransform.GetCurvePosFromScreenPos(evt.localPosition));
                        evt.StopPropagation();
                    }
                    else
                    {
                        if (!IsPreserveCurrentSelectKey(evt))
                        {
                            UnselectAllControlPoint();
                            _lastPointerDownTime = Time.realtimeSinceStartup;
                        }

                        _selectionRect.Show(evt.localPosition);
                        StartDrag();
                    }
                    break;
                
                case 1:
                    UnselectAllControlPoint();
                
                    var pos = _previewTransform.GetCurvePosFromScreenPos(evt.localPosition);
                    PopupMenuUtility.Show(
                        new []{
                            new MenuItem("Add Key", () =>
                            {
                                AddControlPoint(pos);
                            })
                        }, evt.position, this);
                
                    evt.StopPropagation();
                    break;
            }

            return;

            void StartDrag()
            {
                _curvePreviewElement.CapturePointer(evt.pointerId);
                _curvePreviewElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                _curvePreviewElement.RegisterCallback<PointerUpEvent>(OnPointerUp);

                evt.StopPropagation();
            }
        }
        
        private void OnPointerMove(PointerMoveEvent evt)
        {
            var leftButton = (evt.pressedButtons & (1 << 0)) != 0;
            var rightButton = (evt.pressedButtons & (1 << 1)) != 0;
            var middleButton = (evt.pressedButtons & (1 << 2)) != 0;

            var eventUsed = false;
            
            // Pan
            if (middleButton || (leftButton && evt.altKey))
            {
                _previewTransform.AdjustOffsetByScreenDelta(evt.deltaPosition);
                UpdateView();
                eventUsed = true; 
            }
            // Zoom
            else if (rightButton && evt.altKey)
            {
                ApplyZoomByInputEvent(_zoomStartPosition,
                    -evt.deltaPosition,
                    evt.shiftKey,
                    evt.ctrlKey || evt.commandKey);
                eventUsed = true;
            }
            // Drag
            else if (leftButton)
            {
                _selectionRect.SetMovablePosition(evt.localPosition);
                eventUsed = true;
            }


            if (eventUsed)
            {
                SelectControlPointsInSelectionRectIfVisible(IsPreserveCurrentSelectKey(evt));
                evt.StopPropagation();
            }
        }
        
        private void OnPointerUp(PointerUpEvent evt)
        {
            _selectionRect.Hide();

            if (_curvePreviewElement.HasPointerCapture(evt.pointerId))
            {
                _curvePreviewElement.ReleasePointer(evt.pointerId);
            }
            _curvePreviewElement.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _curvePreviewElement.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            evt.StopPropagation();
        }
        
        private static bool IsPreserveCurrentSelectKey(IPointerEvent evt)
        {
            return evt.shiftKey || evt.ctrlKey || evt.commandKey;
        }

        // WheelEventのdelta
        // - shift時はevt.delta.xに値が入る
        // - また横に動かせるホイールの場合はちゃんとdelta.xに値が入る
        //
        // AnimationCurveEditor
        // - deltaのxyは考慮せず、キー入力との組合せによってズーム軸が決まる
        //  - no key:       zoom:xy
        //  - ctrl,command: zoom:x
        //  - shift:        zoom:y
        private void OnWheel(WheelEvent evt)
        {
            ApplyZoomByInputEvent(evt.localMousePosition,
                evt.delta,
                evt.shiftKey,
                evt.ctrlKey || evt.commandKey);
            
            SelectControlPointsInSelectionRectIfVisible();
            evt.StopPropagation();
        }

        private void ApplyZoomByInputEvent(Vector2 localPosition, Vector3 deltaXYZ, bool shiftKey, bool ctrlOrCommandKey)
        {
            const float deltaScale = 0.05f;
            const float deltaMax = 1f;
            var deltaAmount = (deltaXYZ.x + deltaXYZ.y + deltaXYZ.z) * deltaScale;
            
            var delta = Mathf.Clamp(deltaAmount, -deltaMax, deltaMax);
            var amount = Vector2.one + new Vector2(
                shiftKey ? 0f :delta,
                ctrlOrCommandKey ? 0f : delta
            );
            
            var mouseUv = new Vector2(
                localPosition.x / _curvePreviewElement.resolvedStyle.width,
                1f - localPosition.y / _curvePreviewElement.resolvedStyle.height
            );
            var mousePositionOnCurve = _previewTransform.GetCurvePosFromScreenUv(mouseUv);

            _previewTransform.AdjustZoom(amount, mousePositionOnCurve);
            UpdateView();
        }
        
        private void SelectControlPointsInSelectionRectIfVisible(bool preserveCurrentSelection = false)
        {
            if (_selectionRect.IsVisible)
            {
                _curveController.SelectControlPointsInRect(_selectionRect.Rect, preserveCurrentSelection);
            }
        }
        
        #endregion
    }
}