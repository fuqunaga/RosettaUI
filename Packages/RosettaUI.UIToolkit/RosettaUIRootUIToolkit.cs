using RosettaUI.UIToolkit.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class RosettaUIRootUIToolkit : RosettaUIRoot
    {
        protected UIDocument uiDocument;
        UIToolkitBuilder _builder;

        protected override void OnEnable()
        {
            if (uiDocument != null && uiDocument.rootVisualElement is {} ve)
            {
                ve.visible = true;
            }
        }

        protected override void OnDisable()
        {
            if (uiDocument != null && uiDocument.rootVisualElement is {} ve)
            {
                ve.visible = false;
            }
        }

        protected override void BuildInternal(Element element)
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }
            var root = uiDocument.rootVisualElement;
            _builder ??= new UIToolkitBuilder();
            var visualElement = _builder.Build(element);

            root.Add(visualElement);
        }
    }
}