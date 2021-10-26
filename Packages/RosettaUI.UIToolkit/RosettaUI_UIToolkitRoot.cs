using RosettaUI.UIToolkit.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    [RequireComponent(typeof(UIDocument), typeof(IElementCreator))]
    public class RosettaUI_UIToolkitRoot : MonoBehaviour
    {
        protected UIDocument uiDocument;
        protected Element rootElement;
        UIToolkitBuilder _builder;

        protected virtual void Start()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            var elementCreator = GetComponent<IElementCreator>();

            var element = elementCreator.CreateElement();
            Build(element);
        }

        protected virtual void Update()
        {
            rootElement?.Update();
        }

        public void Build(Element element)
        {
            rootElement = element;

            var root = uiDocument.rootVisualElement;
            _builder ??= new UIToolkitBuilder();
            var visualElement = _builder.Build(element);

            root.Add(visualElement);
        }
    }
}