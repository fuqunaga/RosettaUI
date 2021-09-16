using RosettaUI.UIToolkit.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class RosettaUI_UIToolkitRoot : MonoBehaviour
    {
        UIDocument uiDocument;
        Element rootElement;

        UIToolkitBuilder builder;

        void Update()
        {
            rootElement.Update();
        }

        public void Build(Element element)
        {
            rootElement = element;


            if ( uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }


            var root = uiDocument.rootVisualElement;
            if (builder == null) builder = new UIToolkitBuilder();
            root.Add(builder.Build(element));
        }
    }
}