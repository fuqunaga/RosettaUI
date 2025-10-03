using RosettaUI.UIToolkit.Builder;
using RosettaUI.UndoSystem;
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
            
            // RegisterCallbackが同じコールバックを複数回登録しても１つ分しか受け付けないのを当て込んでいる
            // 複数回BuildInternal()してもOnBlurは一回しか登録されない
            root.RegisterCallback<BlurEvent>(OnBlur, TrickleDown.TrickleDown);
            root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);

            return;

            void OnBlur(BlurEvent e)
            {
                UndoHistory.FixLastUndoRecord();
            }
            
            void OnPointerDown(PointerDownEvent e)
            {
                UndoHistory.FixLastUndoRecord();
            }
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