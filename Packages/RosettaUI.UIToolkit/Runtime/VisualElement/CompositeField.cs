using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// 複数のField要素からなる複合Field
    /// Rowとは異なり最初のラベルがプレフィックスレベルになり子供のFieldのサイズが固定される
    /// </summary>
    public class CompositeField : VisualElement
    {
        private static class UssClassName
        {
            public static readonly string UnityBaseField = BaseField<int>.ussClassName;
            public static readonly string UnityBaseFieldLabel = BaseField<int>.labelUssClassName;
            public static readonly string UnityBaseFieldNoLabelVariant = BaseField<int>.noLabelVariantUssClassName;
            
            public static readonly string RosettaUI = "rosettaui";
            public static readonly string CompositeField = RosettaUI + "-composite-field";
            public static readonly string CompositeFieldContentContainer = CompositeField + "__content-container";
        }
        
        
        #region Label ref: BaseField<TValue>
        
        // ReSharper disable once InconsistentNaming
        public Label labelElement { get; private set; }
        
        // ReSharper disable once InconsistentNaming
        public string label
        {
            get => labelElement.text;
            set
            {
                if (labelElement.text != value)
                {
                    labelElement.text = value;

                    if (string.IsNullOrEmpty(labelElement.text))
                    {
                        AddToClassList(UssClassName.UnityBaseFieldNoLabelVariant);
                        labelElement.RemoveFromHierarchy();
                    }
                    else
                    {
                        if (!Contains(labelElement))
                        {
                            // Insert(0, labelElement);
                            hierarchy.Insert(0, labelElement);
                            RemoveFromClassList(UssClassName.UnityBaseFieldNoLabelVariant);
                        }
                    }
                }
            }
        }

        private void InitLabelElement()
        {
            labelElement = new Label() { focusable = true, tabIndex = -1 };
            labelElement.AddToClassList(UssClassName.UnityBaseFieldLabel);
        }
        
        #endregion

        private readonly VisualElement _contentContainer;
        public override VisualElement contentContainer => _contentContainer;
        
        public CompositeField()
        {
            AddToClassList(UssClassName.UnityBaseField);
            AddToClassList(UssClassName.CompositeField);

            InitLabelElement();
            
            _contentContainer = new VisualElement();
            _contentContainer.AddToClassList(UssClassName.CompositeFieldContentContainer);
            hierarchy.Add(_contentContainer);
        }
    }
}