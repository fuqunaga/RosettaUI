using System;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// A visual element that allows you to edit an <see cref="AnimationCurve"/>.
    /// </summary>
    public class AnimationCurveEditor : ModalEditor<AnimationCurve>, IDisposable
    {
        #region Static Window Management
        
        // ReSharper disable once MemberCanBePrivate.Global
        public static Vector2Int DefaultWindowSize { get; set; } = new(600, 400);
        
        private static AnimationCurveEditor _instance;
        
        static AnimationCurveEditor()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(() =>
            {
                _instance?.Dispose();
                _instance = null;
            });
        }

        public static void Show(Vector2 position, VisualElement target, AnimationCurve initialCurve, Action<AnimationCurve> onCurveChanged)
        {
            if ( _instance == null)
            {
                _instance = new AnimationCurveEditor();
                
                var windowStyle = _instance.window.style;
                windowStyle.width = DefaultWindowSize.x;
                windowStyle.height = DefaultWindowSize.y;
            };
            
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
        
        
        private const string USSClassName = "rosettaui-animation-curve-editor";
        private static readonly Vector2 ZoomRange = new(0.0001f, 10000f);

        // ReSharper disable once MemberCanBePrivate.Global
        public static string VisualTreeAssetName { get; set; } = "RosettaUI_AnimationCurveEditor";

        // ReSharper disable once MemberCanBePrivate.Global
        public static RectOffset FitViewPaddingPixel { get; } = new(24, 24, 40, 20);
        
        private readonly CurveController _curveController;
        
        
        private RenderTexture _curveEditorTexture = null;

        private VisualElement _curvePreviewElement;
        private VisualElementKeyEventHelper _keyEventHelper;
        private ToggleButton _snapXButton;
        private ToggleButton _snapYButton;
        private ScrollerController _scrollerController;
        private AxisLabelController _axisLabelController;
        private ControlPointDisplayPositionPopup _controlPointDisplayPositionPopup;
        private ControlPointEditPositionPopup _controlPointEditPositionPopup;

        private PreviewTransform _previewTransform;
        
        private int _mouseButton;
        private Vector2 _prevPointerPosition;
        private long _lastPointerDownTime;
        private bool _prevSnapX = false;
        private bool _prevSnapY = false;
        
        
        #region ModalEditor

        protected override AnimationCurve CopiedValue
        {
            get => AnimationCurveHelper.Clone(_curveController.Curve);
            set => SetCurve(AnimationCurveHelper.Clone(value));
        }

        #endregion
        

        public AnimationCurveEditor() : base(VisualTreeAssetName, true)
        {
            AddToClassList(USSClassName);
            InitUI();
            
            _curveController = new CurveController(_curvePreviewElement, (holder) => new ControlPoint(holder, _previewTransform, _controlPointDisplayPositionPopup, _controlPointEditPositionPopup, OnControlPointSelected, OnControlPointMoved));
            _curveController.onCurveChanged += () =>
            {
                UpdateView();
                NotifyEditorValueChanged();
            };
        }
        
        /// <summary>
        /// Set the curve to be edited.
        /// </summary>
        public void SetCurve(AnimationCurve curve)
        {
            _curveController.SetCurve(curve);
            UnselectAllControlPoint();
            FitViewToCurve();
        }
        
        public void Dispose()
        {
            if (_curveEditorTexture != null)
            {
                Object.DestroyImmediate(_curveEditorTexture);
                _curveEditorTexture = null;
            }
        }
        
        #region Initialization
        
        private void InitUI()
        {
            // Key Bind
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
                if (evt == KeyEventType.KeyDown) { FitViewToCurve(); }
            });
            _keyEventHelper.RegisterKeyAction(KeyCode.Delete, _ =>
            {
                _curveController.RemoveSelectedControlPoint();
            });
            
            // Buttons
            var fitButton = this.Q<Button>("fit-curve-button");
            fitButton.clicked += FitViewToCurve;
            _snapXButton = this.Q<ToggleButton>("time-snapping-button");
            _snapYButton = this.Q<ToggleButton>("value-snapping-button");
            
            // Curve Preview
            _curvePreviewElement = this.Q("preview-front");
            _curvePreviewElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _curvePreviewElement.RegisterCallback<GeometryChangedEvent>(_ => UpdateView());
            _curvePreviewElement.RegisterCallback<WheelEvent>(OnWheel);

            // Curve Preview Transform
            _previewTransform = new PreviewTransform(() => _curvePreviewElement.resolvedStyle.width, () => _curvePreviewElement.resolvedStyle.height);
            
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
            
            // Parameter Popup
            var curveGroup = this.Q("curve-group");
            _controlPointDisplayPositionPopup = new ControlPointDisplayPositionPopup(_previewTransform);
            curveGroup.Add(_controlPointDisplayPositionPopup);
            
            // Edit Key Popup
            _controlPointEditPositionPopup = new ControlPointEditPositionPopup();
            curveGroup.Add(_controlPointEditPositionPopup);
        }
        
        #endregion
        
        #region View Update
        private void FitViewToCurve()
        {
            var rect = _curveController.Curve.GetCurveRect();
            _previewTransform.FitToRect(rect, FitViewPaddingPixel);
            UpdateView();
        }
        
        private void UpdateView()
        {
            var rect = _previewTransform.PreviewRect;
            _scrollerController.UpdateScroller(rect, _curveController.Curve.GetCurveRect());
            _axisLabelController.UpdateAxisLabel(rect);
            
            _curveController.UpdateView();
            UpdateCurvePreview();
        }
        
        private void UpdateCurvePreview()
        {
            var gridViewport = _previewTransform.PreviewGridViewport;
            var viewInfo = new AnimationCurvePreviewRenderer.CurvePreviewViewInfo()
            {
                offsetZoom = _previewTransform.OffsetZoom,
                gridEnabled = true,
                gridParams = new Vector4(gridViewport.XOrder, gridViewport.YOrder, gridViewport.XTick, gridViewport.YTick),
            };
            
            AnimationCurveVisualElementHelper.UpdatePreviewToBackgroundImage(_curveController.Curve, _curvePreviewElement, viewInfo);
        }
        
        #endregion
        
        #region Control Point Process
        
        private void OnControlPointSelected(ControlPoint controlPoint)
        {
            _curveController.SelectControlPoint(controlPoint);
        }
        
        private void OnControlPointMoved(ControlPoint controlPoint, Vector2 desireKeyframePosition)
        {
            var gridViewport = _previewTransform.PreviewGridViewport;

            var position = new Vector2(
                _snapXButton.value ? gridViewport.RoundX(desireKeyframePosition.x, 0.05f) : desireKeyframePosition.x,
                _snapYButton.value ? gridViewport.RoundY(desireKeyframePosition.y, 0.05f) : desireKeyframePosition.y
            );

            controlPoint.KeyframePosition = position;
        }
        
        private void AddControlPoint(Vector2 keyFramePosition)
        {
            UnselectAllControlPoint();
            _curveController.AddKey(keyFramePosition);
        }
        
        private void UnselectAllControlPoint()
        {
            _curveController.UnselectAllControlPoints();
            _controlPointDisplayPositionPopup.Hide();
            _controlPointEditPositionPopup.Hide();
        }
        
        #endregion
        
        #region Pointer Events
        private void OnPointerDown(PointerDownEvent evt)
        {
            _mouseButton = evt.button;
            if (_mouseButton == 2 || (_mouseButton == 0 && (evt.ctrlKey || evt.commandKey || evt.altKey)))
            {
                _curvePreviewElement.CaptureMouse();
                _curvePreviewElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                _curvePreviewElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
                _prevPointerPosition = evt.localPosition;

                evt.StopPropagation();
            }
            else if (_mouseButton == 0)
            {
                UnselectAllControlPoint();

                // Add control point if double click
                if (DateTime.Now.Ticks - _lastPointerDownTime < 5000000)
                {
                    AddControlPoint(_previewTransform.GetCurvePosFromScreenPos(evt.localPosition));
                    _lastPointerDownTime = DateTime.Now.Ticks;
                }
                _lastPointerDownTime = DateTime.Now.Ticks;
            }
            else if (_mouseButton == 1)
            {
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
            }
        }
        
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_mouseButton == 2 || (_mouseButton == 0 && (evt.ctrlKey || evt.commandKey || evt.altKey)))
            {
                _previewTransform.AdjustOffsetByScreenDelta((Vector2)evt.localPosition - _prevPointerPosition);
                UpdateView();
                evt.StopPropagation();
            }
            _prevPointerPosition = evt.localPosition;
        }
        
        private void OnPointerUp(PointerUpEvent evt)
        {
            _curvePreviewElement.ReleaseMouse();
            _curvePreviewElement.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _curvePreviewElement.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            evt.StopPropagation();
        }
        
        private void OnWheel(WheelEvent evt)
        {
            var mouseUv = new Vector2(evt.localMousePosition.x / _curvePreviewElement.resolvedStyle.width, 1f - evt.localMousePosition.y / _curvePreviewElement.resolvedStyle.height);
            mouseUv = _previewTransform.GetCurvePosFromScreenUv(mouseUv);
                
            Vector2 amount = Vector2.one * (1f + Mathf.Clamp(evt.delta.y, -0.1f, 0.1f));
                
            if (evt.ctrlKey || evt.commandKey) { amount.y = 1f; }
            else if (evt.shiftKey) { amount.x = 1f; }
                
            var zoom = _previewTransform.Zoom / amount;
            if (zoom.x < ZoomRange.x || zoom.y < ZoomRange.x || zoom.x > ZoomRange.y || zoom.y > ZoomRange.y) return;
                
            _previewTransform.AdjustZoom(amount, mouseUv);
            UpdateView();
        }
        
        #endregion
    }
}