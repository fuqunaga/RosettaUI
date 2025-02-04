using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class AnimationCurveEditor : VisualElement, IDisposable
    {
        public static readonly string USSClassName = "rosettaui-animation-curve-editor";
        
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
                        width = 600,
                        height = 400
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
            if(!float.IsNaN(_window.resolvedStyle.width) && !float.IsNaN(_window.resolvedStyle.height))
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

        private VisualElement _curvePreviewElement;
        private List<AnimationCurveEditorControlPoint> _controlPoints = new List<AnimationCurveEditorControlPoint>();
        private FloatField _timeField;
        private FloatField _valueField;
        private EnumField _tangentModeField;
        private Slider _inTangentSlider;
        private Slider _outTangentSlider;
        private VisualElement _propertyField;

        private float _zoom = 1.0f;
        private Vector2 _offset = Vector2.zero;
        private int _selectedControlPointIndex = -1;
        
        private int _mouseButton;
        private Vector2 _mouseDownPosition;
        private long _lastMouseDownTime;
        private Vector2 _initialOffset;
        private AnimationCurve _curve;
        private RenderTexture _curveEditorTexture = null;

        private AnimationCurveEditorCurvePreview _curvePreview;

        private AnimationCurveEditor()
        {
            _curvePreview = new AnimationCurveEditorCurvePreview();

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
        

        private void InitUI()
        {
            style.flexGrow = 1;
            
            // Curve Preview
            _curvePreviewElement = this.Q("preview-front");
            _curvePreviewElement.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                UpdateView();
            });
            
            _curvePreviewElement.RegisterCallback<WheelEvent>(evt =>
            {
                var mouseUv = new Vector2(evt.localMousePosition.x / _curvePreviewElement.resolvedStyle.width, 1f - evt.localMousePosition.y / _curvePreviewElement.resolvedStyle.height);
                mouseUv = ScreenUvToGraphCoordinate(mouseUv);
                
                float amount = 1f + Mathf.Clamp(evt.delta.y, -0.1f, 0.1f);
                _zoom /= amount;
                _offset = mouseUv - (mouseUv - _offset) * amount;
                UpdateView();
            });
            
            _curvePreviewElement.RegisterCallback<MouseDownEvent>(OnMouseDown);
            
            _propertyField = this.Q("property-group");
            
            // Tangent Field
            _inTangentSlider = this.Q<Slider>("in-tangent-slider");
            _inTangentSlider.RegisterValueChangedCallback(evt =>
            {
                if (_curve == null || _selectedControlPointIndex < 0) return;
                var key = _curve.keys[_selectedControlPointIndex];
                key.inTangent = AnimationCurveEditorUtility.GetTangentFromDegree(evt.newValue);
                MoveKey(key);
                UpdateView();
                onCurveChanged?.Invoke(_curve);
            });
            _outTangentSlider = this.Q<Slider>("out-tangent-slider");
            _outTangentSlider.RegisterValueChangedCallback(evt =>
            {
                if (_curve == null || _selectedControlPointIndex < 0) return;
                var key = _curve.keys[_selectedControlPointIndex];
                key.outTangent = AnimationCurveEditorUtility.GetTangentFromDegree(evt.newValue);
                MoveKey(key);
                UpdateView();
                onCurveChanged?.Invoke(_curve);
            });
            _tangentModeField = this.Q<EnumField>("tangent-mode-field");
            _tangentModeField.Init(AnimationCurveEditorControlPoint.TangentMode.Smooth);
            _tangentModeField.RegisterValueChangedCallback(evt =>
            {
                if (_curve == null || _selectedControlPointIndex < 0) return;
                AnimationCurveEditorControlPoint.TangentMode mode = (AnimationCurveEditorControlPoint.TangentMode)evt.newValue;
                var key = _curve.keys[_selectedControlPointIndex];
                AnimationCurveEditorUtility.SetTangentMode(ref key, mode);
                var point = _controlPoints[_selectedControlPointIndex];
                point.SetTangentMode(mode);
                MoveKey(key);
                if (mode != AnimationCurveEditorControlPoint.TangentMode.Broken) UpdateView();
                onCurveChanged?.Invoke(_curve);
            });
            
            // Time Field
            _timeField = this.Q<FloatField>("time-field");
            _timeField.RegisterValueChangedCallback(evt =>
            {
                if (_curve == null || _selectedControlPointIndex < 0) return;
                var key = _curve.keys[_selectedControlPointIndex];
                key.time = evt.newValue;
                MoveKey(key);
                UpdateView();
                onCurveChanged?.Invoke(_curve);
            });
            
            // Value Field
            _valueField = this.Q<FloatField>("value-field");
            _valueField.RegisterValueChangedCallback(evt =>
            {
                if (_curve == null || _selectedControlPointIndex < 0) return;
                var key = _curve.keys[_selectedControlPointIndex];
                key.value = evt.newValue;
                MoveKey(key);
                UpdateView();
                onCurveChanged?.Invoke(_curve);
            });
            
            UpdateFields();
        }
        
        /// <summary>
        /// AnimationCurveEditor外部から値をセットする
        /// (AnimationCurveEditorを開いたとき, Presetが選択されたとき)
        /// </summary>
        public void SetCurve(AnimationCurve curve)
        {
            _curve = curve;
            UpdateView();
            
        }
        
        /// <summary>
        /// AnimationCurveEditorのUIから値が変更されたとき
        /// </summary>
        private void OnCurveChanged()
        {
            // UpdateGradientPreview();
            // _presetSet.SetValue(_gradient);
            // onGradientChanged?.Invoke(_gradient);
        }
        
        private void UpdateCurve()
        {
            // var colorSwatches = _colorKeysEditor.ShowedSwatches;
            // var colorKeys = colorSwatches.Select(swatch => new GradientColorKey(swatch.Color, swatch.Time)).ToArray();
            //
            // var alphaSwatches = _alphaKeysEditor.ShowedSwatches;
            // var alphaKeys = alphaSwatches.Select(swatch => new GradientAlphaKey(swatch.Alpha, swatch.Time)).ToArray();
            //
            // _gradient.SetKeys(colorKeys, alphaKeys);
            // _gradient.mode = _modeEnum.value as GradientMode? ?? GradientMode.Blend;
            //
            // OnGradientChanged();
        }
        
        private void UpdateView()
        {
            UpdateCurvePreview();
            UpdateControlPoints();
        }

        private void UpdateControlPoints()
        {
            if (_curve == null) return;
            
            // Add or Remove control points
            if (_curve.keys.Length > _controlPoints.Count)
            {
                for (int i = _controlPoints.Count; i < _curve.keys.Length; i++)
                {
                    var controlPoint = new AnimationCurveEditorControlPoint(OnControlPointSelected,OnControlPointMoved, RemoveControlPoint);
                    _controlPoints.Add(controlPoint);
                    _curvePreviewElement.Add(controlPoint);
                }
            }
            else if (_curve.keys.Length < _controlPoints.Count)
            {
                for (int i = _controlPoints.Count - 1; i >= _curve.keys.Length; i--)
                {
                    _curvePreviewElement.Remove(_controlPoints[i]);
                    _controlPoints.RemoveAt(i);
                }
            }

            // Update control points
            for (var i = 0; i < _curve.keys.Length; i++)
            {
                var controlPoint = _controlPoints[i];
                var key = _curve.keys[i];
                _curvePreviewElement.Add(controlPoint);
                var position = GraphToScreenUvCoordinate(new Vector2(key.time, key.value));
                controlPoint.SetPosition(
                    position.x * _curvePreviewElement.resolvedStyle.width, 
                    (1f - position.y) * _curvePreviewElement.resolvedStyle.height,
                    key.inTangent, 
                    key.outTangent
                );
                controlPoint.SetTangentMode(key.GetTangentMode());
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

            _curvePreview.Render(_curve, new AnimationCurveEditorCurvePreview.CurvePreviewViewInfo {
                resolution = new Vector4(width, height, 1f / width, 1f / height),
                offsetZoom = new Vector4(_offset.x, _offset.y, _zoom, 0f),
                outputTexture = PreviewTexture,
            });
        }
        
        private void MoveKey(Keyframe key)
        {
            int newIdx = _curve.MoveKey(_selectedControlPointIndex, key);

            if (newIdx != _selectedControlPointIndex)
            {
                var temp = _controlPoints[_selectedControlPointIndex];
                _controlPoints.RemoveAt(_selectedControlPointIndex);
                _controlPoints.Insert(newIdx, temp);
                _selectedControlPointIndex = newIdx;
            }
        }

        private void UpdateFields()
        {
            if (_selectedControlPointIndex < 0)
            {
                _propertyField.style.visibility = Visibility.Hidden;
                return;
            }
            _propertyField.style.visibility = Visibility.Visible;
            _timeField.SetValueWithoutNotify(_curve.keys[_selectedControlPointIndex].time);
            _valueField.SetValueWithoutNotify(_curve.keys[_selectedControlPointIndex].value);
            _tangentModeField.SetValueWithoutNotify(_controlPoints[_selectedControlPointIndex].CurrentTangentMode);
            _inTangentSlider.SetValueWithoutNotify(Mathf.Atan(_curve.keys[_selectedControlPointIndex].inTangent) * Mathf.Rad2Deg);
            _outTangentSlider.SetValueWithoutNotify(Mathf.Atan(_curve.keys[_selectedControlPointIndex].outTangent) * Mathf.Rad2Deg);
        }
        
        #region Control Point Process
        
        private void OnControlPointSelected(AnimationCurveEditorControlPoint controlPoint)
        {
            _selectedControlPointIndex = _controlPoints.IndexOf(controlPoint);
            UpdateFields();
            foreach (var c in _controlPoints)
            {
                c.SetActive(c == controlPoint);
            }
        }
        
        private int OnControlPointMoved(Vector2 position, float inTangent, float outTangent)
        {
            position = ScreenUvToGraphCoordinate(position);
            MoveKey(new Keyframe(position.x, position.y, inTangent, outTangent));
            UpdateView();
            UpdateFields();
            onCurveChanged?.Invoke(_curve);
            return _selectedControlPointIndex;
        }
        
        private void RemoveControlPoint(AnimationCurveEditorControlPoint controlPoint)
        {
            int targetIndex = _controlPoints.IndexOf(controlPoint);
            if (targetIndex < 0) return;
            _curve.RemoveKey(targetIndex);
            _controlPoints.RemoveAt(targetIndex);
            _curvePreviewElement.Remove(controlPoint);
            _selectedControlPointIndex = -1;
            UpdateView();
            UpdateFields();
            onCurveChanged?.Invoke(_curve);
        }
        
        private void AddControlPoint(Vector2 position)
        {
            position = ScreenUvToGraphCoordinate(position);
            var key = new Keyframe(position.x, position.y);
            int idx = _curve.AddKey(key);
            _selectedControlPointIndex = idx;
            UpdateView();
            UpdateFields();
            _controlPoints[idx].SetActive(true);
            onCurveChanged?.Invoke(_curve);
        }
        
        private void UnselectAllControlPoint()
        {
            _selectedControlPointIndex = -1;
            UpdateFields();
            foreach (var c in _controlPoints)
            {
                c.SetActive(false);
            }
        }
        
        #endregion
        
        #region Mouse Events
        private void OnMouseDown(MouseDownEvent evt)
        {
            _mouseButton = evt.button;
            if (_mouseButton == 2)
            {
                _curvePreviewElement.RegisterCallback<MouseMoveEvent>(OnMouseMove);
                _curvePreviewElement.RegisterCallback<MouseUpEvent>(OnMouseUp);
                _curvePreviewElement.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
                _mouseDownPosition = evt.localMousePosition;
                _initialOffset = _offset;

                evt.StopPropagation();
            }
            else if (_mouseButton == 0)
            {
                UnselectAllControlPoint();

                // Add control point if double click
                if (DateTime.Now.Ticks - _lastMouseDownTime < 5000000)
                {
                    AddControlPoint(ScreenToScreenUvCoordinate(evt.localMousePosition));
                    _lastMouseDownTime = DateTime.Now.Ticks;
                }
                _lastMouseDownTime = DateTime.Now.Ticks;
            }
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (_mouseButton == 2)
            {
                _offset = _initialOffset + (evt.localMousePosition - _mouseDownPosition) / new Vector2(-_curvePreviewElement.resolvedStyle.width, _curvePreviewElement.resolvedStyle.height) / _zoom;
                UpdateView();
                evt.StopPropagation();
            }
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            _curvePreviewElement.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            _curvePreviewElement.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            evt.StopPropagation();
        }
        
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _curvePreviewElement.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            _curvePreviewElement.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            evt.StopPropagation();
        }
        
        #endregion

        #region Coordomate Convert

        private Vector2 ScreenToScreenUvCoordinate(Vector2 screenPos)
        {
            return new Vector2(screenPos.x / _curvePreviewElement.resolvedStyle.width, 1f - screenPos.y / _curvePreviewElement.resolvedStyle.height);
        }
        
        private Vector2 ScreenUvToGraphCoordinate(Vector2 screenPos)
        {
            return screenPos / _zoom + _offset;
        }
        
        private Vector2 GraphToScreenUvCoordinate(Vector2 graphPos)
        {
            return (graphPos - _offset) * _zoom;
        }

        #endregion


    }
}