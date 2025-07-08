using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// T がクラス型のPreviewBaseField
    /// valueで返した値が参照型なので外部で変更しても値が変わらないようにCloneする
    /// </summary>
    public abstract class PreviewBaseFieldOfClass<T, TInput> : PreviewBaseField<T, TInput>
        where TInput : VisualElement
        where T : class
    {
        public override T value
        {
            get => rawValue == null ? null : Clone(rawValue);
            set
            {
                if ((rawValue == null && value == null) 
                    || (rawValue != null && value != null && rawValue.Equals(value)))
                {
                    return;
                }

                using var evt = ChangeEvent<T>.GetPooled(rawValue, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
            }
        }
        
        protected PreviewBaseFieldOfClass(string label, TInput visualInput) : base(label, visualInput)
        {
        }
        
        protected abstract T Clone(T value);  
    }
}