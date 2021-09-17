using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class ColorPicker : VisualElement
    {
        #region CheckerBoard

        static Texture2D _checkerBoardTexture;
        static Texture2D checkerBoardTexture => _checkerBoardTexture ??= CreateChekcerBoardTexture(new Vector2Int(200, 18), 4, Color.white, Color.HSVToRGB(0f, 0f, 0.8f));
        
        static Texture2D CreateChekcerBoardTexture(Vector2Int size, int gridSize, Color col0, Color col1)
        {
            var tex = new Texture2D(size.x, size.y);
            for (var y = 0; y < size.y; y++)
            {
                var flagY = ((y / gridSize) % 2 == 0);
                for (var x = 0; x < size.x; x++)
                {
                    var flagX = ((x / gridSize) % 2 == 0);
                    tex.SetPixel(x, y, (flagX ^ flagY) ? col0 : col1);
                }
            }

            tex.wrapMode = TextureWrapMode.Repeat;
            tex.Apply();
            return tex;
        }

        #endregion


        static readonly string ussClassName = "rosettaui-colorpicker";
        static readonly string ussClassNamePreview  = ussClassName + "__preview";
        static readonly string ussClassNamePreviewPrev = ussClassName + "__preview-prev";
        static readonly string ussClassNamePreviewCurrent = ussClassName + "__preview-current";
        static readonly string ussClassNameHandler = ussClassName + "__handler";
        static readonly string ussClassNameHandlerSV = ussClassName + "__handler-sv";
        static readonly string ussClassNameHandlerH = ussClassName + "__handler-h";
        


        public ColorPicker()
        {
            AddToClassList(ussClassName);


            var preview = CreateElement(ussClassNamePreview, this);
            preview.style.backgroundImage = checkerBoardTexture;
            CreateElement(ussClassNamePreviewPrev, preview);
            CreateElement(ussClassNamePreviewCurrent, preview);

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
    }
}