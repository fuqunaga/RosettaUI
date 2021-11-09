using RosettaUI.UIToolkit.Builder;
using RosettaUI.UIToolkit.PackageInternal;
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
            base.OnEnable();
            if (uiDocument != null && uiDocument.rootVisualElement is { } ve)
            {
                ve.visible = true;
            }
        }

        protected override void OnDisable()
        {
            base.OnEnable();
            if (uiDocument != null && uiDocument.rootVisualElement is { } ve)
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

        public override bool WillUseKeyInput()
        {
            return uiDocument != null && UIToolkitUtility.WillUseKeyInput(uiDocument.rootVisualElement?.panel);
        } 
    }
}