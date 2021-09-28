using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorPicker : VisualElement
    {
        #region static

        private static ModalWindow _window;
        private static ColorPicker _colorPicker;

        public static void Show(Vector2 position, VisualElement target, Color initialColor, Action<Color> onClose)
        {
            if (_window == null)
            {
                _window = new ModalWindow();
                _colorPicker = new ColorPicker();
                _window.Add(_colorPicker);
            }

            _colorPicker.PrevColor = initialColor;
            _colorPicker.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            
            _window.Show(position, target);
            
            void OnDetach(DetachFromPanelEvent _)
            {
                onClose(_colorPicker.Color);
                _colorPicker.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
            }
        }

        #endregion
        
        static readonly string ussClassName = "rosettaui-colorpicker";
        static readonly string ussClassNamePreview  = ussClassName + "__preview";
        static readonly string ussClassNamePreviewPrev = ussClassName + "__preview-prev";
        static readonly string ussClassNamePreviewCurrent = ussClassName + "__preview-curr";
        static readonly string ussClassNameHandler = ussClassName + "__handler";
        static readonly string ussClassNameHandlerSV = ussClassName + "__handler-sv";
        static readonly string ussClassNameHandlerH = ussClassName + "__handler-h";

        private static readonly Texture2D _svTexture;
        
        private readonly VisualElement _previewPrev;
        private readonly VisualElement _previewCurr;


        public Color PrevColor
        {
            get => _previewPrev.style.backgroundColor.value;
            set
            {
                if (PrevColor != value)
                {
                    _previewPrev.style.backgroundColor = value;
                }

                Color = value;
            }
        }


        public Color Color
        {
            get => _previewCurr.style.backgroundColor.value;
            set
            {
                if (Color == value) return;
                
                _previewCurr.style.backgroundColor = value;
                UpdateColor(value);
            }
        }



        private ColorPicker()
        {
            AddToClassList(ussClassName);

            var preview = CreateElement(ussClassNamePreview, this);
            _previewPrev = CreateElement(ussClassNamePreviewPrev, preview);
            _previewCurr = CreateElement(ussClassNamePreviewCurrent, preview);

            preview.style.backgroundImage = ColorPickerHelper.CheckerBoardTexture;
            
            var handler = CreateElement(ussClassNameHandler, this);
            CreateElement(ussClassNameHandlerSV, handler);
            CreateElement(ussClassNameHandlerH, handler);

            static VisualElement CreateElement(string className, VisualElement parent)
            {
                var element = new VisualElement();
                element.AddToClassList(className);
                parent.Add(element);

                return element;
            }
        }

        void UpdateColor(Color color)
        {
            
        }
    }
}