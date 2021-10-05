using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// Referring to UIElement's Popup.
    /// </summary>
    public class ModalWindow : Window
    {
        readonly VisualElement _eventBlockerElement;
        KeyboardNavigationManipulator _navigationManipulator;

        static readonly string ussClassNameEventBlocker = "rosettaui-modal-window-eventblocker";
        static readonly string ussClassName = "rosettaui-modal-window";

        public ModalWindow() : base(false)
        {
            closeButton = new CloseButton();
            closeButton.clicked += Hide;
            closeButton.visible = false;

            _eventBlockerElement = new VisualElement();
            _eventBlockerElement.AddToClassList(ussClassNameEventBlocker);
            _eventBlockerElement.Add(this);

            AddToClassList(ussClassName);


            /*
            m_OuterContainer = new VisualElement();
            m_OuterContainer.AddToClassList(containerOuterUssClassName);
            m_MenuContainer.Add(m_OuterContainer);

            m_ScrollView = new ScrollView();
            m_ScrollView.AddToClassList(containerInnerUssClassName);
            m_ScrollView.pickingMode = PickingMode.Position;
            m_ScrollView.contentContainer.focusable = true;
            m_ScrollView.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            m_OuterContainer.hierarchy.Add(m_ScrollView);

            */

            /*
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            */


            _eventBlockerElement.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            _eventBlockerElement.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel == null)
                return;

            contentContainer.AddManipulator(_navigationManipulator = new KeyboardNavigationManipulator(Apply));
            _eventBlockerElement.RegisterCallback<PointerDownEvent>(OnPointerDownOnBlocker);
            //evt.destinationPanel.visualTree.RegisterCallback<GeometryChangedEvent>(OnParentResized);
            //m_ScrollView.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
            //RegisterCallback<FocusOutEvent>(OnFocusOut);
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (evt.originPanel == null)
                return;

            UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            contentContainer.RemoveManipulator(_navigationManipulator);

            _eventBlockerElement.UnregisterCallback<PointerDownEvent>(OnPointerDownOnBlocker);

            //evt.originPanel.visualTree.UnregisterCallback<GeometryChangedEvent>(OnParentResized);
        }

        public override void Hide()
        {
            UnregisterPanelCallback();

            _eventBlockerElement.RemoveFromHierarchy();

            /*
            if (m_TargetElement != null)
                m_TargetElement.pseudoStates ^= PseudoStates.Active;
            m_TargetElement = null;
            */
        }

        void Apply(KeyboardNavigationOperation op, EventBase sourceEvent)
        {
            if (Apply(op))
            {
                sourceEvent.StopPropagation();
                sourceEvent.PreventDefault();
            }
        }

        bool Apply(KeyboardNavigationOperation op)
        {
            /*
            var selectedIndex = GetSelectedIndex();

            
            void UpdateSelectionDown(int newIndex)
            {
                while (newIndex < m_Items.Count)
                {
                    if (m_Items[newIndex].element.enabledSelf)
                    {
                        ChangeSelectedIndex(newIndex, selectedIndex);
                        break;
                    }

                    ++newIndex;
                }
            }

            void UpdateSelectionUp(int newIndex)
            {
                while (newIndex >= 0)
                {
                    if (m_Items[newIndex].element.enabledSelf)
                    {
                        ChangeSelectedIndex(newIndex, selectedIndex);
                        break;
                    }

                    --newIndex;
                }
            }
            */

            switch (op)
            {
                case KeyboardNavigationOperation.Cancel:
                    Hide();
                    return true;
                    
                case KeyboardNavigationOperation.Submit:
                    /*
                    var item = m_Items[selectedIndex];
                    if (selectedIndex >= 0 && item.element.enabledSelf)
                    {
                        item.action?.Invoke();
                        item.actionUserData?.Invoke(item.element.userData);
                    }
                    */
                    Hide();
                    
                    return true;

                    /*
                case KeyboardNavigationOperation.Previous:
                    UpdateSelectionUp(selectedIndex < 0 ? m_Items.Count - 1 : selectedIndex - 1);
                    return true;
                case KeyboardNavigationOperation.Next:
                    UpdateSelectionDown(selectedIndex + 1);
                    return true;
                case KeyboardNavigationOperation.PageUp:
                case KeyboardNavigationOperation.Begin:
                    UpdateSelectionDown(0);
                    return true;
                case KeyboardNavigationOperation.PageDown:
                case KeyboardNavigationOperation.End:
                    UpdateSelectionUp(m_Items.Count - 1);
                    return true;
                    */
            }

            return false;
        }



        void OnPointerDownOnBlocker(PointerDownEvent evt)
        {
            /*
            m_MousePosition = m_ScrollView.WorldToLocal(evt.position);
            UpdateSelection(evt.target as VisualElement);

            if (evt.pointerId != PointerId.mousePointerId)
            {
                m_MenuContainer.panel.PreventCompatibilityMouseEvents(evt.pointerId);
            }
            */

            Hide();
        }

#if false
        void OnPointerMove(PointerMoveEvent evt)
        {
            /*
            m_MousePosition = m_ScrollView.WorldToLocal(evt.position);
            UpdateSelection(evt.target as VisualElement);

            if (evt.pointerId != PointerId.mousePointerId)
            {
                m_MenuContainer.panel.PreventCompatibilityMouseEvents(evt.pointerId);
            }
            */

            evt.StopPropagation();
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            /*
            var selectedIndex = GetSelectedIndex();
            if (selectedIndex != -1)
            {
                var item = m_Items[selectedIndex];
                item.action?.Invoke();
                item.actionUserData?.Invoke(item.element.userData);

                Hide();
            }

            if (evt.pointerId != PointerId.mousePointerId)
            {
                m_MenuContainer.panel.PreventCompatibilityMouseEvents(evt.pointerId);
            }
         

            evt.StopPropagation();
            */
            Hide();
            evt.StopPropagation();
        }


        void OnFocusOut(FocusOutEvent evt)
        {
            /*
            if (!m_ScrollView.ContainsPoint(m_MousePosition))
            {
                Hide();
            }
            else
            {
                // Keep the focus in.
                m_MenuContainer.schedule.Execute(contentContainer.Focus);
            }
            */
            Hide();
        }


        void OnParentResized(GeometryChangedEvent evt)
        {
            Hide();
        }
#endif


        protected override VisualElement SelfRoot => _eventBlockerElement;

        public override void Show(Vector2 position, VisualElement target)
        {
            base.Show(position, target);

            var root = target.panel.visualTree;

            _eventBlockerElement.style.left = root.layout.x;
            _eventBlockerElement.style.top = root.layout.y;
            _eventBlockerElement.style.width = root.layout.width;
            _eventBlockerElement.style.height = root.layout.height;
        }
    }
}