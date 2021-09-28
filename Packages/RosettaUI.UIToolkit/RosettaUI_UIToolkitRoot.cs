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
            var window = builder.Build(element);


            var modalWindow = new ModalWindow();
            var textField = new TextField("text");
            modalWindow.Add(textField);

            var button = new Button()
            {
                text = "Test ModalWindow"
            };
            button.clicked += () => modalWindow.Show(default, button);

            window.Add(button);

            root.Add(window);
        }
    }
}