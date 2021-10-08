using RosettaUI.UIToolkit.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class RosettaUI_UIToolkitRoot : MonoBehaviour
    {
        UIDocument _uiDocument;
        Element _rootElement;
        UIToolkitBuilder _builder;

        void Update()
        {
            _rootElement.Update();
        }

        public void Build(Element element)
        {
            _rootElement = element;

            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            var root = _uiDocument.rootVisualElement;
            _builder ??= new UIToolkitBuilder();
            var window = _builder.Build(element);

            root.Add(window);
        }
    }
}