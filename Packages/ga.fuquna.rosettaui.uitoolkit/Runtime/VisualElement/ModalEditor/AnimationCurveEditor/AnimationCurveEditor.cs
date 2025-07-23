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
            
            _instance.SetCurve(initialCurve);
            
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
        
        private readonly CurvePointContainer _curvePointContainer;
        
        private RenderTexture _curveEditorTexture = null;

        private VisualElement _curvePreviewElement;
        private VisualElementKeyEventHelper _keyEventHelper;
        private ToggleButton _snapXButton;
        private ToggleButton _snapYButton;
        private ScrollerController _scrollerController;
        private AxisLabelController _axisLabelController;
        private PropertyFieldController _propertyFieldController;

        private PreviewTransform _previewTransform;
        private int _selectedControlPointIndex = -1;
        
        private int _mouseButton;
        private Vector2 _prevPointerPosition;
        private long _lastPointerDownTime;
        private bool _prevSnapX = false;
        private bool _prevSnapY = false;
        
 

        public AnimationCurveEditor() : base(VisualTreeAssetName, true)
        {
            AddToClassList(USSClassName);
            
            InitUI();
            _curvePointContainer = new CurvePointContainer(_curvePreviewElement, () => new ControlPoint(_previewTransform, OnControlPointSelected, OnControlPointMoved, RemoveControlPoint));
        }
        
        /// <summary>
        /// Set the curve to be edited.
        /// </summary>
        public void SetCurve(AnimationCurve curve)
        {
            _curvePointContainer.SetCurve(curve);
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
        
        private void MoveKey(Keyframe key)
        {
            if (_curvePointContainer.IsEmpty || _selectedControlPointIndex < 0) return;
            var gridViewport = new GridViewport(_previewTransform.GetPreviewRect());
            if (_snapXButton.value) { key.time = gridViewport.RoundX(key.time, 0.05f); }
            if (_snapYButton.value) { key.value = gridViewport.RoundY(key.value, 0.05f); }

            _selectedControlPointIndex = _curvePointContainer.MoveKey(_selectedControlPointIndex, key);
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
                if (evt == KeyEventType.KeyUp) { FitViewToCurve(); }
            });
            _keyEventHelper.RegisterKeyAction(KeyCode.Delete, evt =>
            {
                if (evt != KeyEventType.KeyUp || _selectedControlPointIndex < 0) return;
                RemoveControlPoint(_curvePointContainer.ControlPoints[_selectedControlPointIndex]);
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
                    _curvePointContainer.UpdateControlPoints();
                    UpdateCurvePreview();
                    _axisLabelController.UpdateAxisLabel(_previewTransform.GetPreviewRect());
                },
                xCenter =>
                {
                    _previewTransform.SetXCenter(xCenter);
                    _curvePointContainer.UpdateControlPoints();
                    UpdateCurvePreview();
                    _axisLabelController.UpdateAxisLabel(_previewTransform.GetPreviewRect());
                }
            );

            // Property Field
            _propertyFieldController = new PropertyFieldController(this,
                () => _selectedControlPointIndex < 0 ? default : _curvePointContainer[_selectedControlPointIndex],
                key =>
                {
                    MoveKey(key);
                    UpdateView();
                    NotifyEditorValueChanged(_curvePointContainer.Curve);
                }
            );
        }
        
        #endregion
        
        #region View Update
        private void FitViewToCurve()
        {
            if (_curvePointContainer.IsEmpty) return;
            var rect = _curvePointContainer.Curve.GetCurveRect(true, true);
            _previewTransform.FitToRect(rect, FitViewPaddingPixel);
            UpdateView();
        }
        
        private void UpdateView()
        {
            _curvePointContainer.UpdateControlPoints();
            UpdateCurvePreview();
            var rect = _previewTransform.GetPreviewRect();
            _scrollerController.UpdateScroller(rect, _curvePointContainer.Curve.GetCurveRect(true, true));
            _axisLabelController.UpdateAxisLabel(rect);
        }
        
        private void UpdateCurvePreview()
        {
            var rect = _previewTransform.GetPreviewRect();
            var gridViewport = new GridViewport(rect.width, rect.height);
            var viewInfo = new AnimationCurvePreviewRenderer.CurvePreviewViewInfo()
            {
                offsetZoom = _previewTransform.OffsetZoom,
                gridEnabled = true,
                gridParams = new Vector4(gridViewport.XOrder, gridViewport.YOrder, gridViewport.XTick, gridViewport.YTick),
            };
            
            AnimationCurveVisualElementHelper.UpdatePreviewToBackgroundImage(_curvePointContainer.Curve, _curvePreviewElement, viewInfo);
        }
        
        #endregion
        
        #region Control Point Process
        
        private void OnControlPointSelected(ControlPoint controlPoint)
        {
            _selectedControlPointIndex = _curvePointContainer.ControlPoints.IndexOf(controlPoint);
            _propertyFieldController.UpdatePropertyFields();
            _curvePointContainer.SelectControlPoint(_selectedControlPointIndex);
        }
        
        private int OnControlPointMoved(Keyframe keyframe)
        {
            MoveKey(keyframe);
            UpdateView();
            _propertyFieldController.UpdatePropertyFields();
            NotifyEditorValueChanged(_curvePointContainer.Curve);
            return _selectedControlPointIndex;
        }
        
        private void RemoveControlPoint(ControlPoint cp)
        {
            if (cp == null) return;
            var targetIndex = _curvePointContainer.ControlPoints.IndexOf(cp);
            if (targetIndex < 0 || _curvePointContainer.Count <= 1) return;
            _curvePointContainer.RemoveKey(targetIndex);
            _selectedControlPointIndex = -1;
            UpdateView();
            _propertyFieldController.UpdatePropertyFields();
            NotifyEditorValueChanged(_curvePointContainer.Curve);
        }
        
        private void AddControlPoint(Vector2 positionInCurve)
        {
            UnselectAllControlPoint();
            
            // Add key
            var key = new Keyframe(positionInCurve.x, positionInCurve.y);
            var idx = _curvePointContainer.AddKey(key);
            if (idx < 0) return;
            _selectedControlPointIndex = idx;
            
            // Match the tangent mode to neighborhood point one
            var cp = _curvePointContainer.ControlPoints[_selectedControlPointIndex];
            if (0 < idx)
            {
                cp.SetTangentMode(_curvePointContainer.ControlPoints[_selectedControlPointIndex - 1].OutTangentMode, null);
            }
            if (idx < _curvePointContainer.Count - 1)
            {
                cp.SetTangentMode(null, _curvePointContainer.ControlPoints[_selectedControlPointIndex + 1].InTangentMode);
            }
            
            UpdateView();
            _propertyFieldController.UpdatePropertyFields();
            NotifyEditorValueChanged(_curvePointContainer.Curve);
        }
        
        private void UnselectAllControlPoint()
        {
            _selectedControlPointIndex = -1;
            _propertyFieldController.UpdatePropertyFields();
            _curvePointContainer.UnselectAllControlPoints();
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