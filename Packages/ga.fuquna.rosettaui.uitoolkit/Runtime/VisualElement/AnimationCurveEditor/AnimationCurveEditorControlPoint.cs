using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// Control point of the animation curve editor.
    /// </summary>
    public class AnimationCurveEditorControlPoint : VisualElement
    {
        #region enums

        public enum TangentMode
        {
            Smooth,
            Flat,
            Broken
        }        

        #endregion
        
        public TangentMode CurrentTangentMode { get; private set; } = TangentMode.Smooth;
        
        private Action<AnimationCurveEditorControlPoint> _onPointSelected;
        private OnPointMoved _onPointMoved;
        private Action<AnimationCurveEditorControlPoint> _onPointRemoved;
        
        private Vector2 _elementPositionOnDown;
        private Vector2 _mouseDownPosition;
        
        private VisualElement _controlPoint;
        private AnimationCurveEditorControlPointHandle _leftHandle;
        private AnimationCurveEditorControlPointHandle _rightHandle;

        private Vector2 _position;
        private float _inTangent;
        private float _outTangent;
        
        private Dictionary<TangentMode, MenuItem> _tangentModeMenuItems = new Dictionary<TangentMode, MenuItem>();
        
        public delegate int OnPointMoved(Vector2 position, float inTan, float outTan);
        
        public AnimationCurveEditorControlPoint(Action<AnimationCurveEditorControlPoint> onPointSelected, OnPointMoved onPointMoved, Action<AnimationCurveEditorControlPoint> onPointRemoved)
        {
            _onPointSelected = onPointSelected;
            _onPointMoved = onPointMoved;
            _onPointRemoved = onPointRemoved;
            
            // Handles
            _leftHandle = new AnimationCurveEditorControlPointHandle(180f, () => _onPointSelected(this), angle =>
            {
                angle = Mathf.Clamp(Mathf.Abs(angle), 90f, 180f) * Mathf.Sign(angle);
                _inTangent = AnimationCurveEditorUtility.GetTangentFromDegree(angle);
                if (!AnimationCurveEditorUtility.GetKey(KeyCode.LeftAlt) && !AnimationCurveEditorUtility.GetKey(KeyCode.RightAlt))
                {
                    if (CurrentTangentMode == TangentMode.Flat) CurrentTangentMode = TangentMode.Smooth;
                    if (CurrentTangentMode == TangentMode.Smooth) _outTangent = _inTangent;
                }
                _onPointMoved.Invoke(_position, _inTangent, _outTangent);
            });
            Add(_leftHandle);
            
            _rightHandle = new AnimationCurveEditorControlPointHandle(0f, () => _onPointSelected(this), angle =>
            {
                angle = Mathf.Clamp(angle, -90f, 90f);
                _outTangent = AnimationCurveEditorUtility.GetTangentFromDegree(angle);
                if (!AnimationCurveEditorUtility.GetKey(KeyCode.LeftAlt) && !AnimationCurveEditorUtility.GetKey(KeyCode.RightAlt))
                {
                    if (CurrentTangentMode == TangentMode.Flat) CurrentTangentMode = TangentMode.Smooth;
                    if (CurrentTangentMode == TangentMode.Smooth) _inTangent = _outTangent;
                }
                _onPointMoved.Invoke(_position, _inTangent, _outTangent);
            });
            Add(_rightHandle);
            
            // Control point container
            AddToClassList("rosettaui-animation-curve-editor__control-point-container");
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            this.AddManipulator(new PopupMenuManipulator(() => new []
            {
                new MenuItem("Delete Key", () => _onPointRemoved(this)),
                MenuItem.Separator, 
                _tangentModeMenuItems[TangentMode.Smooth],
                _tangentModeMenuItems[TangentMode.Flat],
                _tangentModeMenuItems[TangentMode.Broken]
            }));
            
            // Control point
            _controlPoint = new VisualElement { name = "AnimationCurveEditorControlPoint" };
            _controlPoint.AddToClassList("rosettaui-animation-curve-editor__control-point");
            _tangentModeMenuItems[TangentMode.Smooth] = new MenuItem("Smooth", () => SetTangentModeAndUpdateView(TangentMode.Smooth));
            _tangentModeMenuItems[TangentMode.Flat] = new MenuItem("Flat", () => SetTangentModeAndUpdateView(TangentMode.Flat));
            _tangentModeMenuItems[TangentMode.Broken] = new MenuItem("Broken", () => SetTangentModeAndUpdateView(TangentMode.Broken));
            Add(_controlPoint);
            
            return;
            
            void SetTangentModeAndUpdateView(TangentMode mode)
            {
                SetTangentMode(mode);
                switch (mode)
                {
                    case TangentMode.Broken:
                        return;
                    case TangentMode.Flat:
                        _inTangent = 0f;
                        _outTangent = 0f;
                        break;
                    case TangentMode.Smooth:
                        _inTangent = _outTangent;
                        break;
                }
                _onPointMoved.Invoke(_position, _inTangent, _outTangent);
            }
        }
        
        public void SetActive(bool active)
        {
            if (active)
            {
                _controlPoint.style.backgroundColor = new StyleColor(Color.white);
                _leftHandle.style.visibility = Visibility.Visible;
                _rightHandle.style.visibility = Visibility.Visible;
            }
            else
            {
                _controlPoint.style.backgroundColor = new StyleColor(Color.green);
                _leftHandle.style.visibility = Visibility.Hidden;
                _rightHandle.style.visibility = Visibility.Hidden;
            }
        }
        
        public void SetTangentMode(TangentMode mode)
        {
            CurrentTangentMode = mode;
            foreach (var item in _tangentModeMenuItems)
            {
                item.Value.isChecked = item.Key == mode;
            }
        }
        
        public void SetPosition(float x, float y, float leftTan, float rightTan)
        {
            style.left = x;
            style.top = y;
            _position = Vector2.up + new Vector2(x, -y) / parent.layout.size;
            SetTangent(leftTan, rightTan);
        }
        
        private void SetTangent(float inTan, float outTan)
        {
            _inTangent = inTan;
            _outTangent = outTan;
            _leftHandle.SetAngle(AnimationCurveEditorUtility.GetDegreeFromTangent(inTan));
            _rightHandle.SetAngle(180f + AnimationCurveEditorUtility.GetDegreeFromTangent(outTan));
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            _mouseDownPosition = evt.mousePosition;
            _elementPositionOnDown = new Vector2(style.left.value.value, style.top.value.value);
            _onPointSelected?.Invoke(this);
            evt.StopPropagation();
            this.CaptureMouse();
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            SetMousePosition(evt.mousePosition);
        }
        
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            SetMousePosition(evt.mousePosition);
        }
        
        private void SetMousePosition(Vector2 mousePosition)
        {
            style.left = _elementPositionOnDown.x + (mousePosition.x - _mouseDownPosition.x);
            style.top = _elementPositionOnDown.y + (mousePosition.y - _mouseDownPosition.y);
            _position = Vector2.up + new Vector2(style.left.value.value, -style.top.value.value) / parent.layout.size;
            _onPointMoved.Invoke(
                _position,
                _inTangent,
                _outTangent
            );
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
            {
                UnregisterCallback<MouseMoveEvent>(OnMouseMove);
                UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
                UnregisterCallback<MouseUpEvent>(OnMouseUp);
                evt.StopPropagation();
                this.ReleaseMouse();
            }
        }
    }
}