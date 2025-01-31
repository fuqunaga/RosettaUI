using System;
using System.Collections.Generic;
using System.Linq;
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
                mouseUv = ScreenToGraphCoordinate(mouseUv);
                
                float amount = 1f + Mathf.Clamp(evt.delta.y, -0.1f, 0.1f);
                _zoom /= amount;
                _offset = mouseUv - (mouseUv - _offset) * amount;
                UpdateView();
            });
            
            _curvePreviewElement.RegisterCallback<MouseDownEvent>(OnMouseDown);
            
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
                    var controlPoint = new AnimationCurveEditorControlPoint(cp =>
                        {
                            _selectedControlPointIndex = _controlPoints.IndexOf(cp);
                            UpdateFields();
                        },
                        (newPos, it, ot) =>
                        {
                            newPos = ScreenToGraphCoordinate(newPos);
                            MoveKey(new Keyframe(newPos.x, newPos.y, it, ot));
                            UpdateView();
                            UpdateFields();
                            onCurveChanged?.Invoke(_curve);
                            return _selectedControlPointIndex;
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

            // Update control points
            for (var i = 0; i < _curve.keys.Length; i++)
            {
                var controlPoint = _controlPoints[i];
                var key = _curve.keys[i];
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
            _timeField.SetValueWithoutNotify(_curve.keys[_selectedControlPointIndex].time);
            _valueField.SetValueWithoutNotify(_curve.keys[_selectedControlPointIndex].value);
        }
        
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