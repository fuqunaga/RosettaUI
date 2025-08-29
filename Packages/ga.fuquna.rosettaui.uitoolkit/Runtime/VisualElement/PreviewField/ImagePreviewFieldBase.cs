using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// Base class for image preview fields using BackgroundImage.
    /// Used in GradientField and AnimationCurveField.
    ///
    /// 参照型の値は外部で変更出来てしまうのでコピーして保持する
    /// SetValueWithoutNotify()で比較してから更新する
    /// </summary>
    public abstract class ImagePreviewFieldBase<TValue, TInput> : PreviewFieldBase<TValue, TInput>
        where TValue : class, new()
        where TInput : VisualElement, ImagePreviewFieldBase<TValue, TInput>.IPreviewElement
    {
        public interface IPreviewElement
        {
            VisualElement Preview{ get; }
        }
        
        
        private TValue _lastAppliedValue;
         
        
        protected ImagePreviewFieldBase(string label, TInput visualInput) : base(label, visualInput)
        {
            RegisterCallback<GeometryChangedEvent>(_ => UpdateTexture());
        }
     
        public override void SetValueWithoutNotify(TValue newValue)
        {
            if (Equals(_lastAppliedValue, newValue))
            {
                return;
            }
            
            _lastAppliedValue ??= new TValue();
            Copy(newValue, _lastAppliedValue);
            
            base.SetValueWithoutNotify(newValue);
            
            UpdateTexture();
        }

        private void UpdateTexture()
        {
            var preview = inputField.Preview;

            if (value == null)
            {
                preview.style.backgroundImage = StyleKeyword.Undefined;
            }
            else
            {
                UpdatePreviewToBackgroundImage(value, preview);
            }
        }

        protected virtual bool Equals(TValue a, TValue b)
        {
            return EqualityComparer<TValue>.Default.Equals(a, b);
        }
        
        protected abstract void Copy(TValue source, TValue destination);
        protected abstract void UpdatePreviewToBackgroundImage(TValue currentValue, VisualElement element);
    }
}