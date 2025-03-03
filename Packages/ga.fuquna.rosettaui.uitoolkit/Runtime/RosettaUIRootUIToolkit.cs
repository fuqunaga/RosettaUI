using RosettaUI.UIToolkit.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    [RequireComponent(typeof(UIDocument))]
    public class RosettaUIRootUIToolkit : RosettaUIRoot
    {
        public const string USSRootClassName = "rosettaui-root";

        public KeyCode closeWindowKey = KeyCode.Escape;
        public EventModifiers closeWindowKeyModifiers;
        
        protected UIDocument uiDocument;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (uiDocument != null && uiDocument.rootVisualElement is { } ve)
            {
                ve.visible = true;
            }
            
            ApplyCloseWindowKey();
        }

        protected override void OnDisable()
        {
            base.OnEnable();
            if (uiDocument != null && uiDocument.rootVisualElement is { } ve)
            {
                ve.visible = false;
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            ApplyCloseWindowKey();
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

        protected void ApplyCloseWindowKey()
        {
            Window.closeKey = closeWindowKey;
            Window.closeKeyModifiers = closeWindowKeyModifiers;
        }
    }
}