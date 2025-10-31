using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// Color, Gradient, AnimationCurve などのプレビューを表示するフィールド。
    /// </summary>
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public abstract class PreviewFieldBase<TValue, TInput> : BaseField<TValue>
        where TInput : VisualElement
    {
        // ReSharper disable once ConvertToConstant.Global
        public new static readonly string ussClassName = "rosettaui-preview-base-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        protected readonly TInput inputField;
        
        protected PreviewFieldBase(string label, TInput visualInput) : base(label, visualInput)
        {
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
            visualInput.AddToClassList(inputUssClassName);
            
            visualInput.RegisterCallback<ClickEvent>(OnClickInput);
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            
            inputField = visualInput;
        }

        private void OnClickInput(ClickEvent evt)
        {
            ShowEditor(evt.position);
            evt.StopPropagation();
        }
        
        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            var mousePosition = Input.mousePosition;
            var position = new Vector2(
                mousePosition.x,
                Screen.height - mousePosition.y
            );

            var screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            if (!screenRect.Contains(position))
            {
                position = worldBound.center;
            }

            ShowEditor(position);

            evt.StopPropagation();
        }


        protected abstract void ShowEditor(Vector3 position);
    }
}