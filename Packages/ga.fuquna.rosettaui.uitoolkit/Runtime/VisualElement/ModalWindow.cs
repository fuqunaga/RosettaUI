using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// Referring to UIElement's Popup.
    /// </summary>
    public class ModalWindow : Window
    {
        private const string USSClassNameEventBlocker = "rosettaui-modal-window-event-blocker";
        private const string USSClassName = "rosettaui-modal-window";

        private readonly VisualElement _eventBlockerElement;

        protected override VisualElement SelfRoot => _eventBlockerElement;

        public ModalWindow(bool resizable = false) : base(resizable, true)
        {
            _eventBlockerElement = new VisualElement();
            _eventBlockerElement.AddToClassList(USSClassNameEventBlocker);
            _eventBlockerElement.Add(this);

            AddToClassList(USSClassName);

            _eventBlockerElement.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            _eventBlockerElement.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }
        
        
        public override void Hide()
        {
            base.Hide();
            _eventBlockerElement.RemoveFromHierarchy();
        }

        
        protected virtual void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel == null)
                return;
            
            _eventBlockerElement.RegisterCallback<PointerDownEvent>(OnPointerDownOnBlocker);
        }

        protected virtual  void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (evt.originPanel == null)
                return;

            _eventBlockerElement.UnregisterCallback<PointerDownEvent>(OnPointerDownOnBlocker);
        }

        protected virtual  void OnPointerDownOnBlocker(PointerDownEvent evt)
        {
            if (worldBound.Contains(evt.position)) return;
            Hide();
            evt.StopPropagation();
        }
    }
}