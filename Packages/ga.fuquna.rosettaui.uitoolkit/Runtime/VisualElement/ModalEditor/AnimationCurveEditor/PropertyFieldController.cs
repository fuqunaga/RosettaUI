using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public class PropertyFieldController
    {
        private readonly VisualElement _parent;
        private readonly Func<(Keyframe key, ControlPoint point)> _getSelectedKeyPoint;
        private readonly Action<Keyframe> _onModifyKeyAndUpdateView;
        private Action _updatePropertyFields = delegate { };
        
        private const string PropertyFieldName = "property-group";
        private const string InTangentSliderName = "in-tangent-slider";
        private const string InTangentModeFieldName = "in-tangent-mode-field";
        private const string InWeightedToggleName = "in-weighted-toggle";
        private const string OutTangentSliderName = "out-tangent-slider";
        private const string OutTangentModeFieldName = "out-tangent-mode-field";
        private const string OutWeightedToggleName = "out-weighted-toggle";
        private const string PointModeFieldName = "point-mode-field";
        private const string TimeFieldName = "time-field";
        private const string ValueFieldName = "value-field";

        public PropertyFieldController(VisualElement parent, Func<(Keyframe, ControlPoint)> getSelectedKeyPoint, Action<Keyframe> onModifyKeyAndUpdateView)
        {
            _parent = parent;
            _getSelectedKeyPoint = getSelectedKeyPoint;
            _onModifyKeyAndUpdateView = onModifyKeyAndUpdateView;
            
            InitUI();
        }
        
        public void UpdatePropertyFields()
        {
            _updatePropertyFields();
        }

        private void InitUI()
        {
            // Property Fields
            var propertyField = _parent.Q(PropertyFieldName);
              
            var inTangentSlider = SetupKeyframeField<Slider, float>(InTangentSliderName, (evt, key, cp) =>
            {
                key.inTangent = AnimationCurveEditorUtility.GetTangentFromDegree(evt.newValue);
                if (cp.PointMode == PointMode.Smooth) key.outTangent = key.inTangent;
                _onModifyKeyAndUpdateView(key);
                _updatePropertyFields();
            });
            var inTangentModeField = SetupKeyframeEnumField<TangentMode>(InTangentModeFieldName, (evt, key, cp) =>
            {
                cp.SetInTangentMode((TangentMode)evt.newValue);
                _onModifyKeyAndUpdateView(key);
            });
            var inWeightedButton = _parent.Q<ToggleButton>(InWeightedToggleName);
            inWeightedButton.toggledStateChanged += val =>
            {
                var keyPoint = _getSelectedKeyPoint();
                if (keyPoint.point == null) return;
                keyPoint.key.SetWeightedFrag(WeightedMode.In, val);
                _onModifyKeyAndUpdateView(keyPoint.key);
            };
            var outTangentSlider = SetupKeyframeField<Slider, float>(OutTangentSliderName, (evt, key, cp) =>
            {
                key.outTangent = AnimationCurveEditorUtility.GetTangentFromDegree(evt.newValue);
                if (cp.PointMode == PointMode.Smooth) key.inTangent = key.outTangent;
                _onModifyKeyAndUpdateView(key);
                _updatePropertyFields();
            });
            var outTangentModeField = SetupKeyframeEnumField<TangentMode>(OutTangentModeFieldName, (evt, key, cp) =>
            {
                cp.SetOutTangentMode((TangentMode)evt.newValue);
                _onModifyKeyAndUpdateView(key);
            });
            var outWeightedButton = _parent.Q<ToggleButton>(OutWeightedToggleName);
            outWeightedButton.toggledStateChanged += val =>
            {
                var keyPoint = _getSelectedKeyPoint();
                if (keyPoint.point == null) return;
                keyPoint.key.SetWeightedFrag(WeightedMode.Out, val);
                _onModifyKeyAndUpdateView(keyPoint.key);
            };
            var pointModeField = SetupKeyframeEnumField<PointMode>(PointModeFieldName, (evt, key, cp) =>
            {
                PointMode mode = (PointMode)evt.newValue;
                key.SetPointMode(mode);
                cp.SetPointMode(mode);
                _onModifyKeyAndUpdateView(key);
                _updatePropertyFields();
            });
            
            var timeField = SetupKeyframeField<FloatField, float>(TimeFieldName, (evt, key, _) =>
            {
                key.time = evt.newValue;
                _onModifyKeyAndUpdateView(key);
            });
            var valueField = SetupKeyframeField<FloatField, float>(ValueFieldName, (evt, key, _) =>
            {
                key.value = evt.newValue;
                _onModifyKeyAndUpdateView(key);
            });

            _updatePropertyFields = () =>
            {
                var keyPoint = _getSelectedKeyPoint();
                bool isSelectionValid = keyPoint.point != null;
                
                propertyField.SetEnabled(isSelectionValid);
                timeField.SetValueWithoutNotify(isSelectionValid ? keyPoint.key.time : 0f);
                valueField.SetValueWithoutNotify(isSelectionValid ? keyPoint.key.value : 0f);
                pointModeField.SetValueWithoutNotify(isSelectionValid ? keyPoint.point.PointMode : PointMode.Smooth);
                inTangentSlider.SetValueWithoutNotify(isSelectionValid ? Mathf.Atan(keyPoint.key.inTangent) * Mathf.Rad2Deg : 0f);
                inTangentModeField.SetValueWithoutNotify(isSelectionValid ? keyPoint.point.InTangentMode : TangentMode.Free);
                inWeightedButton.SetValueWithoutNotify(isSelectionValid && keyPoint.key.weightedMode is WeightedMode.In or WeightedMode.Both);
                outTangentSlider.SetValueWithoutNotify(isSelectionValid ? Mathf.Atan(keyPoint.key.outTangent) * Mathf.Rad2Deg : 0f);
                outTangentModeField.SetValueWithoutNotify(isSelectionValid ? keyPoint.point.OutTangentMode : TangentMode.Free);
                outWeightedButton.SetValueWithoutNotify(isSelectionValid && keyPoint.key.weightedMode is WeightedMode.Out or WeightedMode.Both);
            };
            
            _updatePropertyFields();
        }
        
        private TField SetupKeyframeField<TField, T>(string elementName, Action<ChangeEvent<T>, Keyframe, ControlPoint> updateKey)
            where TField : BaseField<T>
        {
            var field = _parent.Q<TField>(elementName);
            field.RegisterValueChangedCallback(evt =>
            {
                var keyPoint = _getSelectedKeyPoint();
                if (keyPoint.point == null) return;
                updateKey(evt, keyPoint.key, keyPoint.point);
            });
            return field;
        }
            
        private EnumField SetupKeyframeEnumField<TEnum>(string elementName, Action<ChangeEvent<Enum>, Keyframe, ControlPoint> updateKey)
            where TEnum : Enum
        {
            var field = _parent.Q<EnumField>(elementName);
            field.Init(default(TEnum));
            field.RegisterValueChangedCallback(evt =>
            {
                var keyPoint = _getSelectedKeyPoint();
                if (keyPoint.point == null) return;
                updateKey(evt, keyPoint.key, keyPoint.point);
            });
            return field;
        }
    }
}