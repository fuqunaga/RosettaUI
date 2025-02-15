using System;
using System.Collections.Generic;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    /// <summary>
    /// A visual element that allows you to edit an <see cref="AnimationCurve"/>.
    /// </summary>
    public class AnimationCurveEditor : VisualElement, IDisposable
    {
        #region Static Window Management
        private static ModalWindow _window;
        private static AnimationCurveEditor _animationCurveEditorInstance;
        
        static AnimationCurveEditor()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(ReleaseStaticResource);
            
            void ReleaseStaticResource()
            {
                _window?.Hide();
                _window = null;
                _animationCurveEditorInstance?.Dispose();
                _animationCurveEditorInstance = null;
            }
        }

        public static void Show(Vector2 position, VisualElement target, AnimationCurve initialCurve,
            Action<AnimationCurve> onCurveChanged)
        {
            if (_window == null)
            {
                _window = new ModalWindow(true) { style = { width = 800, height = 500, minWidth = 500 } };
                _animationCurveEditorInstance = new AnimationCurveEditor();
                _window.Add(_animationCurveEditorInstance);

                _window.RegisterCallback<NavigationSubmitEvent>(_ => _window.Hide());
                _window.RegisterCallback<NavigationCancelEvent>(_ =>
                {
                    onCurveChanged?.Invoke(initialCurve);
                    _window.Hide();
                });
            }

            _window.Show(position, target);

            // Preventing Overflow
            if (!float.IsNaN(_window.resolvedStyle.width) && !float.IsNaN(_window.resolvedStyle.height))
            {
                VisualElementExtension.CheckOutOfScreen(position, _window);
            }

            // Panel is not set before Show(), so callback registration and other operations are performed after Show().
            _animationCurveEditorInstance.SetCurve(initialCurve);
            _animationCurveEditorInstance.OnCurveChanged += onCurveChanged;
            _animationCurveEditorInstance.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            return;

            void OnDetach(DetachFromPanelEvent _)
            {
                _animationCurveEditorInstance.OnCurveChanged -= onCurveChanged;
                _animationCurveEditorInstance.UnregisterCallback<DetachFromPanelEvent>(OnDetach);

                target?.Focus();
            }
        }

        #endregion
        
        public event Action<AnimationCurve> OnCurveChanged;
        
        private CurvePointContainer _curvePointContainer;
        
        private CurvePreview _curvePreview;
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
        
        private static readonly string USSClassName = "rosettaui-animation-curve-editor";
        private static readonly Vector2 ZoomRange = new(0.0001f, 10000f);
        private static readonly float FitViewPadding = 0.05f;

        public AnimationCurveEditor()
        {
            var visualTreeAsset = Resources.Load<VisualTreeAsset>("RosettaUI_AnimationCurveEditor");
            visualTreeAsset.CloneTree(this);
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
            FitViewToCurve();
            UpdateView();
            UnselectAllControlPoint();
        }
        
        public void Dispose()
        {
            if (_curvePreview != null)
            {
                _curvePreview.Dispose();
                _curvePreview = null;
            }
            if (_curveEditorTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(_curveEditorTexture);
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
                if (evt == KeyEventType.KeyDown) { FitViewToCurve(); }
            });
            _keyEventHelper.RegisterKeyAction(KeyCode.Delete, evt =>
            {
                if (evt != KeyEventType.KeyDown || _selectedControlPointIndex < 0) return;
                RemoveControlPoint(_curvePointContainer.ControlPoints[_selectedControlPointIndex]);
            });
            
            // Buttons
            var fitButton = this.Q<Button>("fit-curve-button");
            fitButton.clicked += FitViewToCurve;
            _snapXButton = this.Q<ToggleButton>("time-snapping-button");
            _snapYButton = this.Q<ToggleButton>("value-snapping-button");
            
            // Curve Preview
            _curvePreview = new CurvePreview();
            _curvePreviewElement = this.Q("preview-front");
            _curvePreviewElement.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _curvePreviewElement.RegisterCallback<GeometryChangedEvent>(_ => UpdateView());
            _curvePreviewElement.RegisterCallback<WheelEvent>(OnWheel);

            // Curve Preview Transform
            _previewTransform = new PreviewTransform(() => _curvePreviewElement.resolvedStyle.width, () => _curvePreviewElement.resolvedStyle.height);
            
            // Axis Labels & Scrollers
            _axisLabelController = new AxisLabelController(this);
            _scrollerController = new ScrollerController(this, yCenter =>
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
            });
            
            // Property Field
            _propertyFieldController = new PropertyFieldController(this, () =>
            {
                if (_selectedControlPointIndex < 0) return default;
                return _curvePointContainer[_selectedControlPointIndex];
            }, 
            key =>
            {
                MoveKey(key);
                UpdateView();
                OnCurveChanged?.Invoke(_curvePointContainer.Curve);
            });
        }
        
        #endregion
        
        #region View Update
        private void FitViewToCurve()
        {
            if (_curvePointContainer.IsEmpty) return;
            var rect = _curvePointContainer.Curve.GetCurveRect();
            var padding = rect.size * FitViewPadding;
            rect.xMin -= padding.x;
            rect.xMax += padding.x;
            rect.yMin -= padding.y;
            rect.yMax += padding.y;
            _previewTransform.FitToRect(rect);
            UpdateView();
        }
        
        private void UpdateView()
        {
            _curvePointContainer.UpdateControlPoints();
            UpdateCurvePreview();
            var rect = _previewTransform.GetPreviewRect();
            _scrollerController.UpdateScroller(rect, _curvePointContainer.Curve.GetCurveRect());
            _axisLabelController.UpdateAxisLabel(rect);
        }
        
        private void UpdateCurvePreview()
        {
            int width = (int)_curvePreviewElement.resolvedStyle.width;
            int height = (int)_curvePreviewElement.resolvedStyle.height;
            if (width <= 0 || height <= 0) return;
            
            if (_curveEditorTexture == null || _curveEditorTexture.width != width || _curveEditorTexture.height != height)
            {
                _curveEditorTexture?.Release();
                _curveEditorTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                _curvePreviewElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_curveEditorTexture));
            }
            
            var rect = _previewTransform.GetPreviewRect();
            var gridViewport = new GridViewport(rect.width, rect.height);
            _curvePreview.Render(_curvePointContainer.Curve, new CurvePreview.CurvePreviewViewInfo {
                Resolution = new Vector4(width, height, 1f / width, 1f / height),
                OffsetZoom = _previewTransform.OffsetZoom,
                GridParams = new Vector4(gridViewport.XOrder, gridViewport.YOrder, gridViewport.XTick, gridViewport.YTick),
                OutputTexture = _curveEditorTexture,
            });
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
            OnCurveChanged?.Invoke(_curvePointContainer.Curve);
            return _selectedControlPointIndex;
        }
        
        private void RemoveControlPoint(ControlPoint cp)
        {
            if (cp == null) return;
            int targetIndex = _curvePointContainer.ControlPoints.IndexOf(cp);
            if (targetIndex < 0) return;
            _curvePointContainer.RemoveKey(targetIndex);
            _selectedControlPointIndex = -1;
            UpdateView();
            _propertyFieldController.UpdatePropertyFields();
            OnCurveChanged?.Invoke(_curvePointContainer.Curve);
        }
        
        private void AddControlPoint(Vector2 positionInCurve)
        {
            UnselectAllControlPoint();
            
            // Add key
            var key = new Keyframe(positionInCurve.x, positionInCurve.y);
            int idx = _curvePointContainer.AddKey(key);
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
            OnCurveChanged?.Invoke(_curvePointContainer.Curve);
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