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
    public class AnimationCurveEditor : VisualElement, ICoordinateConverter,
        IDisposable
    {
        #region static 

        private static ModalWindow _window;
        private static AnimationCurveEditor _animationCurveEditorInstance;

        private static VisualTreeAsset _visualTreeAsset;
        
        static AnimationCurveEditor()
        {
            StaticResourceUtility.AddResetStaticResourceCallback(() =>
            {
                _window?.Hide();
                _window = null;
                _animationCurveEditorInstance?.Dispose();
                _animationCurveEditorInstance = null;
                _visualTreeAsset = null;
            });
        }

        public static void Show(Vector2 position, VisualElement target, AnimationCurve initialCurve,
            Action<AnimationCurve> onCurveChanged)
        {
            if (_window == null)
            {
                _window = new ModalWindow(true)
                {
                    style = {
                        width = 800,
                        height = 500,
                        minWidth = 500
                    }
                };
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


            // はみ出し抑制
            if (!float.IsNaN(_window.resolvedStyle.width) && !float.IsNaN(_window.resolvedStyle.height))
            {
                VisualElementExtension.CheckOutOfScreen(position, _window);
            }
            
            // Show()前はPanelが設定されていないのでコールバック系はShow()後
            _animationCurveEditorInstance.SetCurve(initialCurve);
            _animationCurveEditorInstance.onCurveChanged += onCurveChanged;
            _animationCurveEditorInstance.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            return;

            void OnDetach(DetachFromPanelEvent _)
            {
                _animationCurveEditorInstance.onCurveChanged -= onCurveChanged;
                _animationCurveEditorInstance.UnregisterCallback<DetachFromPanelEvent>(OnDetach);

                target?.Focus();
            }
        }
        
        #endregion
        
        public event Action<AnimationCurve> onCurveChanged;
        
        private RenderTexture PreviewTexture
        {
            get => _curveEditorTexture;
            set
            {
                _curveEditorTexture = value;
                _curvePreviewElement.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(value));
            }
        }
        
        private AnimationCurve _curve;
        private RenderTexture _curveEditorTexture = null;

        private VisualElement _curvePreviewElement;
        private List<ControlPoint> _controlPoints = new List<ControlPoint>();
        private ToggleButton _snapXButton;
        private ToggleButton _snapYButton;
        private ScrollerController _scrollerController;
        private AxisLabelController _axisLabelController;
        private CurvePreview _curvePreview;

        private Vector2 _zoom = Vector2.one;
        private Vector2 _offset = Vector2.zero;
        private int _selectedControlPointIndex = -1;
        
        private int _mouseButton;
        private Vector2 _mouseDownPosition;
        private long _lastMouseDownTime;
        private Vector2 _initialOffset;
        private bool _prevSnapX = false;
        private bool _prevSnapY = false;
        
        private Action _updatePropertyFields;

        private static readonly string USSClassName = "rosettaui-animation-curve-editor";
        private static readonly Vector2 ZoomRange = new Vector2(0.0001f, 10000f);

        private AnimationCurveEditor()
        {
            _curvePreview = new CurvePreview();

            AddToClassList(USSClassName);
            _visualTreeAsset ??= Resources.Load<VisualTreeAsset>("RosettaUI_AnimationCurveEditor");
            _visualTreeAsset.CloneTree(this);
            
            InitUI();
        }
        
        public void Dispose()
        {
            if (_curvePreview != null)
            {
                _curvePreview.Dispose();
                _curvePreview = null;
            }
            if (PreviewTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(PreviewTexture);
                PreviewTexture = null;
            }
        }
        
        /// <summary>
        /// Set the curve to be edited.
        /// </summary>
        public void SetCurve(AnimationCurve curve)
        {
            _curve = curve;
            InitControlPoints();
            FitViewToCurve();
            UpdateView();
            UnselectAllControlPoint();
        }
        
        
        private void MoveKey(Keyframe key)
        {
            if (_curve == null || _selectedControlPointIndex < 0) return;
            var rect = GetViewRectInCurve();
            float xUnit = Mathf.Pow(10f, (int)Mathf.Log10(rect.width)) * 0.05f;
            float yUnit = Mathf.Pow(10f, (int)Mathf.Log10(rect.height)) * 0.05f;
            if (_snapXButton.value) key.time = Mathf.Round(key.time / xUnit) * xUnit;
            if (_snapYButton.value) key.value = Mathf.Round(key.value / yUnit) * yUnit;

            for (var i = 0; i < _curve.keys.Length; i++)
            {
                if (i == _selectedControlPointIndex) continue;
                if (_curve.keys[i].time == key.time) return;
            }
            
            int newIdx = _curve.MoveKey(_selectedControlPointIndex, key);
            if (newIdx != _selectedControlPointIndex)
            {
                var temp = _controlPoints[_selectedControlPointIndex];
                _controlPoints.RemoveAt(_selectedControlPointIndex);
                _controlPoints.Insert(newIdx, temp);
                _selectedControlPointIndex = newIdx;
            }
        }
        
        #region Initialization
        private void InitUI()
        {
            focusable = true;
            
            // Key Bind
            bool isKeyPressed = false;
            RegisterCallback<KeyDownEvent>(evt =>
            {
                if (isKeyPressed) return;
                isKeyPressed = true;
                switch (evt.keyCode)
                {
                    case KeyCode.LeftControl:
                    case KeyCode.LeftCommand:
                    case KeyCode.RightControl:
                    case KeyCode.RightCommand:
                        _prevSnapX = _snapXButton.value;
                        _prevSnapY = _snapYButton.value;
                        _snapXButton.SetValueWithoutNotify(true);
                        _snapYButton.SetValueWithoutNotify(true);
                        break;
                }
            });
            RegisterCallback<KeyUpEvent>(evt =>
            {
                isKeyPressed = false;
                switch (evt.keyCode)
                {
                    case KeyCode.LeftControl:
                    case KeyCode.LeftCommand:
                    case KeyCode.RightControl:
                    case KeyCode.RightCommand:
                        _snapXButton.SetValueWithoutNotify(_prevSnapX);
                        _snapYButton.SetValueWithoutNotify(_prevSnapY);
                        break;
                    case KeyCode.A:
                        FitViewToCurve();
                        break;
                    case KeyCode.Delete when _selectedControlPointIndex >= 0:
                        RemoveControlPoint(_controlPoints[_selectedControlPointIndex]);
                        break;
                }
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
            _curvePreviewElement.RegisterCallback<WheelEvent>(evt =>
            {
                var mouseUv = new Vector2(evt.localMousePosition.x / _curvePreviewElement.resolvedStyle.width, 1f - evt.localMousePosition.y / _curvePreviewElement.resolvedStyle.height);
                mouseUv = GetCurvePosFromScreenUv(mouseUv);
                
                Vector2 amount = Vector2.one * (1f + Mathf.Clamp(evt.delta.y, -0.1f, 0.1f));
                
                if (evt.ctrlKey) amount.y = 1f;
                else if (evt.shiftKey) amount.x = 1f;
                
                var zoom = _zoom / amount;
                if (zoom.x < ZoomRange.x || zoom.y < ZoomRange.x || zoom.x > ZoomRange.y || zoom.y > ZoomRange.y) return;
                
                _zoom /= amount;
                _offset = mouseUv - (mouseUv - _offset) * amount;

                UpdateView();
            });
            
            _axisLabelController = new AxisLabelController(this);
            _scrollerController = new ScrollerController(this,
                yCenter =>
                {
                    _offset.y = yCenter - 0.5f / _zoom.y;
                    UpdateControlPoints();
                    UpdateCurvePreview();
                    var rect = GetViewRectInCurve();
                    _axisLabelController.UpdateAxisLabel(rect);
                },
                xCenter =>
                {
                    _offset.x = xCenter - 0.5f / _zoom.x;
                    UpdateControlPoints();
                    UpdateCurvePreview();
                    var rect = GetViewRectInCurve();
                    _axisLabelController.UpdateAxisLabel(rect);
                }
            );
            
            // Property Fields
            var propertyField = this.Q("property-group");
            
            var inTangentSlider = SetupKeyframeField<Slider, float>("in-tangent-slider", (evt, key, cp) =>
            {
                key.inTangent = AnimationCurveEditorUtility.GetTangentFromDegree(evt.newValue);
                if (cp.PointMode == PointMode.Smooth) key.outTangent = key.inTangent;
                MoveKey(key);
                _updatePropertyFields();
            });
            var inTangentModeField = SetupKeyframeEnumField<TangentMode>("in-tangent-mode-field", (evt, key, cp) =>
            {
                cp.SetTangentMode((TangentMode)evt.newValue, null);
                MoveKey(key);
            });
            var inWeightedButton = this.Q<ToggleButton>("in-weighted-toggle");
            inWeightedButton.toggledStateChanged += val =>
            {
                if (_curve == null || _selectedControlPointIndex < 0) return;
                var key = _curve.keys[_selectedControlPointIndex];
                key.SetWeightedFrag(WeightedMode.In, val);
                MoveKey(key);
                UpdateView();
                onCurveChanged?.Invoke(_curve);
            };
            var outTangentSlider = SetupKeyframeField<Slider, float>("out-tangent-slider", (evt, key, cp) =>
            {
                key.outTangent = AnimationCurveEditorUtility.GetTangentFromDegree(evt.newValue);
                if (cp.PointMode == PointMode.Smooth) key.inTangent = key.outTangent;
                MoveKey(key);
                _updatePropertyFields();
            });
            var outTangentModeField = SetupKeyframeEnumField<TangentMode>("out-tangent-mode-field", (evt, key, cp) =>
            {
                cp.SetTangentMode(null, (TangentMode)evt.newValue);
                MoveKey(key);
            });
            var outWeightedButton = this.Q<ToggleButton>("out-weighted-toggle");
            outWeightedButton.toggledStateChanged += val =>
            {
                if (_curve == null || _selectedControlPointIndex < 0) return;
                var key = _curve.keys[_selectedControlPointIndex];
                key.SetWeightedFrag(WeightedMode.Out, val);
                MoveKey(key);
                UpdateView();
                onCurveChanged?.Invoke(_curve);
            };
            var pointModeField = SetupKeyframeEnumField<PointMode>("point-mode-field", (evt, key, cp) =>
            {
                PointMode mode = (PointMode)evt.newValue;
                key.SetPointMode(mode);
                cp.SetPointMode(mode);
                MoveKey(key);
                _updatePropertyFields();
            });
            
            var timeField = SetupKeyframeField<FloatField, float>("time-field", (evt, key, _) =>
            {
                key.time = evt.newValue;
                MoveKey(key);
            });
            var valueField = SetupKeyframeField<FloatField, float>("value-field", (evt, key, _) =>
            {
                key.value = evt.newValue;
                MoveKey(key);
            });

            _updatePropertyFields = () =>
            {
                if (_selectedControlPointIndex < 0)
                {
                    propertyField.SetEnabled(false);
                    timeField.SetValueWithoutNotify(0f);
                    valueField.SetValueWithoutNotify(0f);
                    pointModeField.SetValueWithoutNotify(PointMode.Smooth);
                    inTangentSlider.SetValueWithoutNotify(0f);
                    inTangentModeField.SetValueWithoutNotify(TangentMode.Free);
                    inWeightedButton.SetValueWithoutNotify(false);
                    outTangentSlider.SetValueWithoutNotify(0f);
                    outTangentModeField.SetValueWithoutNotify(TangentMode.Free);
                    outWeightedButton.SetValueWithoutNotify(false);
                    return;
                }
                propertyField.SetEnabled(true);
                timeField.SetValueWithoutNotify(_curve.keys[_selectedControlPointIndex].time);
                valueField.SetValueWithoutNotify(_curve.keys[_selectedControlPointIndex].value);
                pointModeField.SetValueWithoutNotify(_controlPoints[_selectedControlPointIndex].PointMode);
                inTangentSlider.SetValueWithoutNotify(Mathf.Atan(_curve.keys[_selectedControlPointIndex].inTangent) * Mathf.Rad2Deg);
                inTangentModeField.SetValueWithoutNotify(_controlPoints[_selectedControlPointIndex].InTangentMode);
                inWeightedButton.SetValueWithoutNotify(_curve.keys[_selectedControlPointIndex].weightedMode is WeightedMode.In or WeightedMode.Both);
                outTangentSlider.SetValueWithoutNotify(Mathf.Atan(_curve.keys[_selectedControlPointIndex].outTangent) * Mathf.Rad2Deg);
                outTangentModeField.SetValueWithoutNotify(_controlPoints[_selectedControlPointIndex].OutTangentMode);
                outWeightedButton.SetValueWithoutNotify(_curve.keys[_selectedControlPointIndex].weightedMode is WeightedMode.Out or WeightedMode.Both);

            };
            
            _updatePropertyFields();
            return;

            TField SetupKeyframeField<TField, T>(string elementName, Action<ChangeEvent<T>, Keyframe, ControlPoint> updateKey)
                where TField : BaseField<T>
            {
                var field = this.Q<TField>(elementName);
                field.RegisterValueChangedCallback(evt =>
                {
                    if (_curve == null || _selectedControlPointIndex < 0) return;
                    updateKey(evt, _curve.keys[_selectedControlPointIndex], _controlPoints[_selectedControlPointIndex]);
                    UpdateView();
                    onCurveChanged?.Invoke(_curve);
                });
                return field;
            }
            
            EnumField SetupKeyframeEnumField<TEnum>(string elementName, Action<ChangeEvent<Enum>, Keyframe, ControlPoint> updateKey)
                where TEnum : Enum
            {
                var field = this.Q<EnumField>(elementName);
                field.Init(default(TEnum));
                field.RegisterValueChangedCallback(evt =>
                {
                    if (_curve == null || _selectedControlPointIndex < 0) return;
                    updateKey(evt, _curve.keys[_selectedControlPointIndex], _controlPoints[_selectedControlPointIndex]);
                    UpdateView();
                    onCurveChanged?.Invoke(_curve);
                });
                return field;
            }
        }
        
        private void InitControlPoints()
        {
            if (_curve == null) return;
            foreach (var cp in _controlPoints)
            {
                _curvePreviewElement.Remove(cp);
            }
            _controlPoints.Clear();
            for (var i = 0; i < _curve.keys.Length; i++)
            {
                var key = _curve.keys[i];
                var controlPoint = new ControlPoint(this, OnControlPointSelected, OnControlPointMoved, RemoveControlPoint);
                _controlPoints.Add(controlPoint);
                _curvePreviewElement.Insert(0, controlPoint);
                controlPoint.SetKeyframe(_curve, i);
                controlPoint.SetPointMode(key.GetPointMode());
                controlPoint.SetTangentMode(_curve.GetInTangentMode(i), _curve.GetOutTangentMode(i));
                controlPoint.SetWeightedMode(key.weightedMode);
            }
        }
        #endregion
        
        #region View Update
        private void FitViewToCurve()
        {
            if (_curve == null) return;
            var rect = _curve.GetCurveRect();
            _offset = rect.min;
            _zoom = new Vector2(1f / rect.width, 1f / rect.height);
            UpdateView();
        }
        
        private void UpdateView()
        {
            UpdateControlPoints();
            UpdateCurvePreview();
            var rect = GetViewRectInCurve();
            _scrollerController.UpdateScroller(rect, _curve.GetCurveRect());
            _axisLabelController.UpdateAxisLabel(rect);
        }

        private void UpdateControlPoints()
        {
            if (_curve == null) return;
            
            AnimationCurveEditorUtility.ApplyTangentMode(ref _curve, _controlPoints);
            for (var i = 0; i < _curve.keys.Length; i++)
            {
                _controlPoints[i].SetKeyframe(_curve, i);
            }
        }
        
        private void UpdateCurvePreview()
        {
            int width = (int)_curvePreviewElement.resolvedStyle.width;
            int height = (int)_curvePreviewElement.resolvedStyle.height;
            if (width <= 0 || height <= 0) return;
            
            if (PreviewTexture == null || PreviewTexture.width != width || PreviewTexture.height != height)
            {
                PreviewTexture?.Release();
                PreviewTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            }
            
            var rect = GetViewRectInCurve();
            var gridViewport = new GridViewport(rect.width, rect.height);
            _curvePreview.Render(_curve, new CurvePreview.CurvePreviewViewInfo {
                Resolution = new Vector4(width, height, 1f / width, 1f / height),
                OffsetZoom = new Vector4(_offset.x, _offset.y, _zoom.x, _zoom.y),
                GridParams = new Vector4(gridViewport.XOrder, gridViewport.YOrder, gridViewport.XTick, gridViewport.YTick),
                OutputTexture = PreviewTexture,
            });
        }
        
        #endregion
        
        #region Control Point Process
        
        private void OnControlPointSelected(ControlPoint controlPoint)
        {
            _selectedControlPointIndex = _controlPoints.IndexOf(controlPoint);
            _updatePropertyFields();
            foreach (var c in _controlPoints)
            {
                c.SetActive(c == controlPoint);
            }
        }
        
        private int OnControlPointMoved(Keyframe keyframe)
        {
            MoveKey(keyframe);
            UpdateView();
            _updatePropertyFields();
            onCurveChanged?.Invoke(_curve);
            return _selectedControlPointIndex;
        }
        
        private void RemoveControlPoint(ControlPoint controlPoint)
        {
            int targetIndex = _controlPoints.IndexOf(controlPoint);
            if (targetIndex < 0) return;
            _curve.RemoveKey(targetIndex);
            _controlPoints.RemoveAt(targetIndex);
            _curvePreviewElement.Remove(controlPoint);
            _selectedControlPointIndex = -1;
            UpdateView();
            _updatePropertyFields();
            onCurveChanged?.Invoke(_curve);
        }
        
        private void AddControlPoint(Vector2 positionInCurve)
        {
            UnselectAllControlPoint();
            
            // Add key
            var key = new Keyframe(positionInCurve.x, positionInCurve.y);
            int idx = _curve.AddKey(key);
            
            // Add control point
            var controlPoint = new ControlPoint(this, OnControlPointSelected, OnControlPointMoved, RemoveControlPoint);
            _curvePreviewElement.Add(controlPoint);
            controlPoint.SetKeyframe(_curve, idx);
            controlPoint.SetActive(true);
            _controlPoints.Insert(idx, controlPoint);
            
            _selectedControlPointIndex = idx;
            UpdateView();
            _updatePropertyFields();
            onCurveChanged?.Invoke(_curve);
        }
        
        private void UnselectAllControlPoint()
        {
            _selectedControlPointIndex = -1;
            _updatePropertyFields();
            foreach (var c in _controlPoints)
            {
                c.SetActive(false);
            }
        }
        
        #endregion
        
        #region Pointer Events
        private void OnPointerDown(PointerDownEvent evt)
        {
            _mouseButton = evt.button;
            if (_mouseButton == 2 || (_mouseButton == 0 && evt.ctrlKey))
            {
                _curvePreviewElement.CaptureMouse();
                _curvePreviewElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                _curvePreviewElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
                _mouseDownPosition = evt.localPosition;
                _initialOffset = _offset;

                evt.StopPropagation();
            }
            else if (_mouseButton == 0)
            {
                UnselectAllControlPoint();

                // Add control point if double click
                if (DateTime.Now.Ticks - _lastMouseDownTime < 5000000)
                {
                    AddControlPoint(this.GetCurvePosFromScreenPos(evt.localPosition));
                    _lastMouseDownTime = DateTime.Now.Ticks;
                }
                _lastMouseDownTime = DateTime.Now.Ticks;
            }
            else if (_mouseButton == 1)
            {
                UnselectAllControlPoint();
                
                var pos = this.GetCurvePosFromScreenPos(evt.localPosition);
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
            if (_mouseButton == 2 || (_mouseButton == 0 && evt.ctrlKey))
            {
                _offset = _initialOffset + ((Vector2)evt.localPosition - _mouseDownPosition) / new Vector2(-_curvePreviewElement.resolvedStyle.width, _curvePreviewElement.resolvedStyle.height) / _zoom;
                UpdateView();
                evt.StopPropagation();
            }
        }
        
        private void OnPointerUp(PointerUpEvent evt)
        {
            _curvePreviewElement.ReleaseMouse();
            _curvePreviewElement.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _curvePreviewElement.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            evt.StopPropagation();
        }
        
        #endregion

        #region Coordinate Convert

        public Vector2 GetScreenUvFromScreenPos(Vector2 screenPos)
        {
            return new Vector2(screenPos.x / _curvePreviewElement.resolvedStyle.width, 1f - screenPos.y / _curvePreviewElement.resolvedStyle.height);
        }
        
        public Vector2 GetScreenPosFromScreenUv(Vector2 screenUv)
        {
            return new Vector2(screenUv.x * _curvePreviewElement.resolvedStyle.width, (1f - screenUv.y) * _curvePreviewElement.resolvedStyle.height);
        }
        
        public Vector2 GetCurvePosFromScreenUv(Vector2 screenUv)
        {
            return screenUv / _zoom + _offset;
        }
        
        public Vector2 GetScreenUvFromCurvePos(Vector2 curvePos)
        {
            return (curvePos - _offset) * _zoom;
        }

        public float GetCurveTangentFromScreenTangent(float tangent)
        {
            float aspect = _curvePreviewElement.resolvedStyle.width / _curvePreviewElement.resolvedStyle.height;
            tangent *= aspect;
            return tangent;
        }
        
        public float GetScreenTangentFromCurveTangent(float tangent)
        {
            float aspect = _curvePreviewElement.resolvedStyle.width / _curvePreviewElement.resolvedStyle.height;
            tangent /= aspect;
            return tangent;
        }
        
        public Rect GetViewRectInCurve()
        {
            var rect = new Rect
            {
                min = GetCurvePosFromScreenUv(Vector2.zero),
                max = GetCurvePosFromScreenUv(Vector2.one)
            };
            return rect;
        }

        #endregion
    }
}