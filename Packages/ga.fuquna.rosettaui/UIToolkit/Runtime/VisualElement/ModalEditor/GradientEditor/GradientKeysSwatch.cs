using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientKeysSwatch
    {
        public static readonly string CursorUSSClassName = $"{GradientEditor.USSClassName}__cursor";
        public static readonly string AlphaCursorUSSClassName = $"{GradientEditor.USSClassName}__cursor-alpha";
        public static readonly string ColorCursorUSSClassName = $"{GradientEditor.USSClassName}__cursor-color";
     
        public static readonly string CursorOutlineUSSClassName = $"{GradientEditor.USSClassName}__cursor_outline";
        
        private static int _uniqueIdSeed = 0;
        
        
        public static int TextDigit { get; set; } = 3;
        
        public static GradientKeysSwatch CreateFromSnapshot(Snapshot snapshot)
        {
            var swatch = new GradientKeysSwatch(snapshot.uniqueId)
            {
                TimePercent = snapshot.timePercent,
                Color = snapshot.color,
                IsAlpha = snapshot.isAlpha
            };
            
            return swatch;
        }
        
        private static int GenerateUniqueId() => _uniqueIdSeed++;

        private static float Round(float value, int digit)
        {
            var scale = Mathf.Pow(10f, digit);
            return Mathf.Round(value * scale) / scale;
        }
        
        private static VisualElement CreateVisualElement()
        {
            var cursor = new VisualElement();
            cursor.AddToClassList(CursorUSSClassName);
            cursor.focusable = true;
            cursor.tabIndex = -1;

            var outline = new VisualElement();
            outline.AddToClassList(CursorOutlineUSSClassName);
            cursor.Add(outline);
                
            return cursor;
        }
        
        
        public readonly VisualElement visualElement = CreateVisualElement();
        
        private bool _isAlpha;

        
        /// <summary>
        /// Swatchの一意なID
        /// Undo時に復元するので実は別インスタンスになっても同じIDを持つことがある
        /// </summary>
        public int UniqueId { get; private set; }
        
        public bool IsAlpha
        {
            get => _isAlpha; 
            private set
            {
                _isAlpha = value;
                if (_isAlpha)
                {
                    visualElement.RemoveFromClassList(ColorCursorUSSClassName);
                    visualElement.AddToClassList(AlphaCursorUSSClassName);
                }
                else
                {
                    visualElement.RemoveFromClassList(AlphaCursorUSSClassName);
                    visualElement.AddToClassList(ColorCursorUSSClassName);
                }
            }
        }

        public float Time
        {
            get => TimePercent * 0.01f;
            set => TimePercent = value * 100f;
        }

        public float TimePercent
        {
            get => visualElement.style.left.value.value;
            set => visualElement.style.left = Length.Percent(Round(value, 1));
        }

        public Color Color
        {
            get => visualElement.style.unityBackgroundImageTintColor.value;
            set
            {
                visualElement.style.unityBackgroundImageTintColor = value;
                IsAlpha = false;
            }
        }

        public float Alpha
        {
            get => visualElement.style.unityBackgroundImageTintColor.value.r;
            set
            {
                var v = Round(value, TextDigit);
                visualElement.style.unityBackgroundImageTintColor = new Color(v,v,v,1f);
                IsAlpha = true;
            }
        }


        public GradientKeysSwatch() : this(GenerateUniqueId())
        {
        }

        private GradientKeysSwatch(int uniqueId) => UniqueId = uniqueId;

        
        public void Focus()
        {
            visualElement.Focus();
            visualElement.BringToFront();
        }
        
        public void Blur()
        {
            visualElement.Blur();
        }

        
        public Snapshot GetSnapshot()
        {
            return new Snapshot(this);
        }
        

        public readonly struct Snapshot
        {
            public readonly int uniqueId;
            public readonly float timePercent;
            public readonly Color color;
            public readonly bool isAlpha;
            
            public Snapshot(GradientKeysSwatch swatch)
            {
                uniqueId = swatch.UniqueId;
                timePercent = swatch.TimePercent;
                color = swatch.Color;
                isAlpha = swatch.IsAlpha;
            }
        }
    }
}