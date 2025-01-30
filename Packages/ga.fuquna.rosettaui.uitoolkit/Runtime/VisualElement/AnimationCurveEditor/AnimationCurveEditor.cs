using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
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

        private float _zoom = 1.0f;
        private Vector2 _offset = Vector2.zero;
        private int _selectedControlPointIndex = -1;
        
        private int _mouseButton;
        private Vector2 _mouseDownPosition;
        private Vector2 _initialOffset;
        private AnimationCurve _curve;
        private Material _curveEditorMaterial;
        private RenderTexture _curveEditorTexture = null;
        private GraphicsBuffer _keyframesBuffer;

        private AnimationCurveEditorCurvePreview _curvePreview;

        private AnimationCurveEditor()
        {
            _curveEditorMaterial = new Material(Resources.Load<Shader>("RosettaUI_AnimationCurveEditorShader"));
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
            if (_curveEditorMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(_curveEditorMaterial);
                _curveEditorMaterial = null;
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
                UpdateCurvePreview();
                UpdateControlPoints();
            });
            
            _curvePreviewElement.RegisterCallback<WheelEvent>(evt =>
            {
                var mouseUv = new Vector2(evt.localMousePosition.x / _curvePreviewElement.resolvedStyle.width, 1f - evt.localMousePosition.y / _curvePreviewElement.resolvedStyle.height);
                mouseUv = ScreenToGraphCoordinate(mouseUv);
                
                float amount = 1f + Mathf.Clamp(evt.delta.y, -0.1f, 0.1f);
                _zoom /= amount;
                _offset = mouseUv - (mouseUv - _offset) * amount;
                UpdateCurvePreview();
                UpdateControlPoints();
            });
            
            _curvePreviewElement.RegisterCallback<MouseDownEvent>(OnMouseDown);
            
            // Time Field
            _timeField = this.Q<FloatField>("time-field");
            _timeField.RegisterValueChangedCallback(evt =>
            {
                if (_curve == null || _selectedControlPointIndex < 0) return;
                _curve.MoveKey(_selectedControlPointIndex, new Keyframe(evt.newValue, _curve.keys[_selectedControlPointIndex].value));
                UpdateCurvePreview();
                UpdateControlPoints();
                onCurveChanged?.Invoke(_curve);
            });
            
            // Value Field
            _valueField = this.Q<FloatField>("value-field");
            _valueField.RegisterValueChangedCallback(evt =>
            {
                if (_curve == null || _selectedControlPointIndex < 0) return;
                _curve.MoveKey(_selectedControlPointIndex, new Keyframe(_curve.keys[_selectedControlPointIndex].time, evt.newValue));
                UpdateCurvePreview();
                UpdateControlPoints();
                onCurveChanged?.Invoke(_curve);
            });
        }
        
        /// <summary>
        /// AnimationCurveEditor外部から値をセットする
        /// (AnimationCurveEditorを開いたとき, Presetが選択されたとき)
        /// </summary>
        public void SetCurve(AnimationCurve curve)
        {
            _curve = curve;
            
            // _gradient = gradient;
            // _modeEnum.SetValueWithoutNotify(_gradient.mode);
            UpdateCurvePreview();
            UpdateControlPoints();
            // UpdateAlphaKeysEditor();
            // UpdateColorKeysEditor();
            // UpdateSelectedSwatchField(_colorKeysEditor.ShowedSwatches.FirstOrDefault());
            //
            // _presetSet.SetValue(_gradient);
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

        private void UpdateControlPoints()
        {
            if (_curve == null) return;
            
            if (_curve.keys.Length > _controlPoints.Count)
            {
                for (int i = _controlPoints.Count; i < _curve.keys.Length; i++)
                {
                    int index = i;
                    var controlPoint = new AnimationCurveEditorControlPoint(
                        () =>
                        {
                            _selectedControlPointIndex = index;
                            _timeField.SetValueWithoutNotify(_curve.keys[index].time);
                            _valueField.SetValueWithoutNotify(_curve.keys[index].value);
                        },
                        (newPos, lt, rt) =>
                        {
                            newPos = ScreenToGraphCoordinate(newPos);
                            _curve.MoveKey(index, new Keyframe(newPos.x, newPos.y, lt, rt));
                            UpdateCurvePreview();
                            UpdateControlPoints();
                            _timeField.SetValueWithoutNotify(_curve.keys[index].time);
                            _valueField.SetValueWithoutNotify(_curve.keys[index].value);
                            onCurveChanged?.Invoke(_curve);
                        });
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

            for (var i = 0; i < _curve.keys.Length; i++)
            {
                var key = _curve.keys[i];
                var controlPoint = _controlPoints[i];
                _curvePreviewElement.Add(controlPoint);
                var position = GraphToScreenCoordinate(new Vector2(key.time, key.value));
                controlPoint.SetPosition(
                    position.x * _curvePreviewElement.resolvedStyle.width, 
                    (1f - position.y) * _curvePreviewElement.resolvedStyle.height,
                    key.inTangent, 
                    key.outTangent
                );
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
            
            if (_keyframesBuffer == null || _keyframesBuffer.count != _curve.keys.Length)
            {
                _keyframesBuffer?.Dispose();
                _keyframesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _curve.keys.Length, sizeof(float) * 4);
            }
            
            _keyframesBuffer.SetData(_curve.keys.Select(key => new Vector4(key.time, key.value, key.inTangent, key.outTangent)).ToArray());
            _curveEditorMaterial.SetBuffer("_Keyframes", _keyframesBuffer);
            _curveEditorMaterial.SetInt("_KeyframesCount", _curve.keys.Length);
            _curveEditorMaterial.SetVector("_Resolution", new Vector4(width, height, 1f / width, 1f / height));
            _curveEditorMaterial.SetVector("_OffsetZoom", new Vector4(_offset.x, _offset.y, _zoom, 0f));
            
            Graphics.Blit(null, PreviewTexture, _curveEditorMaterial);

            // _curvePreview.Render(_curve, new AnimationCurveEditorCurvePreview.CurvePreviewViewInfo {
            //     resolution = new Vector4(width, height, 1f / width, 1f / height),
            //     offsetZoom = new Vector4(_offset.x, _offset.y, _zoom, 0f),
            //     outputTexture = PreviewTexture,
            // });
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            _curvePreviewElement.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            _curvePreviewElement.RegisterCallback<MouseUpEvent>(OnMouseUp);
            _curvePreviewElement.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            _mouseButton = evt.button;
            _mouseDownPosition = evt.localMousePosition;
            _initialOffset = _offset;
            
            evt.StopPropagation();
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (_mouseButton == 2)
            {
                _offset = _initialOffset + (evt.localMousePosition - _mouseDownPosition) / new Vector2(-_curvePreviewElement.resolvedStyle.width, _curvePreviewElement.resolvedStyle.height) / _zoom;
                UpdateCurvePreview();
                UpdateControlPoints();
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
        
        private Vector2 ScreenToGraphCoordinate(Vector2 screenPos)
        {
            return screenPos / _zoom + _offset;
        }
        
        private Vector2 GraphToScreenCoordinate(Vector2 graphPos)
        {
            return (graphPos - _offset) * _zoom;
        }

    }
}