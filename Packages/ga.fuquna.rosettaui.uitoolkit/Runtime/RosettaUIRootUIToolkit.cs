using RosettaUI.UIToolkit.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class RosettaUIRootUIToolkit : RosettaUIRoot
    {
        public const string USSRootClassName = "rosettaui-root";

        protected UIDocument uiDocument;

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
            root.AddToClassList(USSRootClassName);
            var visualElement = UIToolkitBuilder.Build(element);

            root.Add(visualElement);
        }

        public override bool WillUseKeyInput()
        {
            return uiDocument != null && UIToolkitUtility.WillUseKeyInput(uiDocument.rootVisualElement?.panel);
        }

        public override bool IsFocusedInstance => uiDocument != null && uiDocument.rootVisualElement?.focusController.focusedElement != null;

        public override bool IsOverUIInstance(Vector2 screenPosition)
        {
            var panel = uiDocument != null ? uiDocument.rootVisualElement?.panel : null;
            if (panel == null) return false;
            
            screenPosition.y = Screen.height - screenPosition.y;
            
            var panelPosition = RuntimePanelUtils.ScreenToPanel(panel, screenPosition);
            
            return panel.Pick(panelPosition) != null;
        }
    }
}