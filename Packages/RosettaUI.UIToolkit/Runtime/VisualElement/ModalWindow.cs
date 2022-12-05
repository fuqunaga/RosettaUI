#define AvoidCompileError

using System;
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

        private const string USSClassNameEventBlocker = "rosettaui-modal-window-event-blocker";
        private const string USSClassName = "rosettaui-modal-window";

        protected override VisualElement SelfRoot => _eventBlockerElement;

        public ModalWindow() : base(false)
        {
            CloseButton = new WindowTitleButton();
            CloseButton.clicked += Hide;
            CloseButton.visible = false;

            _eventBlockerElement = new VisualElement();
            _eventBlockerElement.AddToClassList(USSClassNameEventBlocker);
            _eventBlockerElement.Add(this);

            AddToClassList(USSClassName);

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
    }

#if AvoidCompileError
    /// <summary>
    /// Represents an operation that the user is trying to accomplish through a specific input mechanism.
    /// </summary>
    /// <remarks>
    /// Tests the received callback value for <see cref="KeyboardNavigationManipulator"/> against the values of this enum to implement the operation in your UI.
    /// </remarks>
    public enum KeyboardNavigationOperation
    {
        /// <summary>
        /// Default value. Indicates an uninitialized enum value.
        /// </summary>
        None,

        /// <summary>
        /// Selects all UI selectable elements or text.
        /// </summary>
        SelectAll,

        /// <summary>
        /// Cancels the current UI interaction.
        /// </summary>
        Cancel,

        /// <summary>
        /// Submits or concludes the current UI interaction.
        /// </summary>
        Submit,

        /// <summary>
        /// Selects the previous item.
        /// </summary>
        Previous,

        /// <summary>
        /// Selects the next item.
        /// </summary>
        Next,

        /// <summary>
        /// Moves the selection up one page (in a list which has scrollable area).
        /// </summary>
        PageUp,

        /// <summary>
        /// Moves the selection down one page (in a list which has scrollable area).
        /// </summary>
        PageDown,

        /// <summary>
        /// Selects the first element.
        /// </summary>
        Begin,

        /// <summary>
        /// Selects the last element.
        /// </summary>
        End,
    }

    /// <summary>
    /// Provides a default implementation for translating input device specific events to higher level navigation operations as commonly possible with a keyboard.
    /// </summary>
    public class KeyboardNavigationManipulator : Manipulator
    {
        readonly Action<KeyboardNavigationOperation, EventBase> m_Action;

        /// <summary>
        /// Initializes and returns an instance of KeyboardNavigationManipulator, configured to invoke the specified callback.
        /// </summary>
        /// <param name="action">This action is invoked when specific low level events are dispatched to the target <see cref="VisualElement"/>,
        /// with a specific value of <see cref="KeyboardNavigationOperation"/> and a reference to the original low level event.</param>
        public KeyboardNavigationManipulator(Action<KeyboardNavigationOperation, EventBase> action)
        {
            m_Action = action;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            /*
            target.RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
            target.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            target.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
            */
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            /*
            target.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove);
            target.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            target.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
            */
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        internal void OnKeyDown(KeyDownEvent evt)
        {
            if (target.panel?.contextType == ContextType.Editor)
                OnEditorKeyDown(evt);
            else
                OnRuntimeKeyDown(evt);
        }

        void OnRuntimeKeyDown(KeyDownEvent evt)
        {
            // At the moment these actions are not mapped dynamically in the InputSystemEventSystem component.
            // When that becomes the case in the future, remove the following and use corresponding Navigation events.
            KeyboardNavigationOperation GetOperation()
            {
                switch (evt.keyCode)
                {
                    case KeyCode.A when evt.actionKey: return KeyboardNavigationOperation.SelectAll;
                    case KeyCode.Home: return KeyboardNavigationOperation.Begin;
                    case KeyCode.End: return KeyboardNavigationOperation.End;
                    case KeyCode.PageUp: return KeyboardNavigationOperation.PageUp;
                    case KeyCode.PageDown: return KeyboardNavigationOperation.PageDown;
                }

                // TODO why do we want to invoke the callback in this case? Looks weird.
                return KeyboardNavigationOperation.None;
            }

            Invoke(GetOperation(), evt);
        }

        void OnEditorKeyDown(KeyDownEvent evt)
        {
            KeyboardNavigationOperation GetOperation()
            {
                switch (evt.keyCode)
                {
                    case KeyCode.A when evt.actionKey: return KeyboardNavigationOperation.SelectAll;
                    case KeyCode.Escape: return KeyboardNavigationOperation.Cancel;
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter: return KeyboardNavigationOperation.Submit;
                    case KeyCode.UpArrow: return KeyboardNavigationOperation.Previous;
                    case KeyCode.DownArrow: return KeyboardNavigationOperation.Next;
                    case KeyCode.Home: return KeyboardNavigationOperation.Begin;
                    case KeyCode.End: return KeyboardNavigationOperation.End;
                    case KeyCode.PageUp: return KeyboardNavigationOperation.PageUp;
                    case KeyCode.PageDown: return KeyboardNavigationOperation.PageDown;
                }

                return KeyboardNavigationOperation.None;
            }

            Invoke(GetOperation(), evt);
        }

#if false
        void OnNavigationCancel(NavigationCancelEvent evt)
        {
            Invoke(KeyboardNavigationOperation.Cancel, evt);
        }

        void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            Invoke(KeyboardNavigationOperation.Submit, evt);
        }

        void OnNavigationMove(NavigationMoveEvent evt)
        {
            switch (evt.direction)
            {
                case NavigationMoveEvent.Direction.Up:
                    Invoke(KeyboardNavigationOperation.Previous, evt);
                    break;
                case NavigationMoveEvent.Direction.Down:
                    Invoke(KeyboardNavigationOperation.Next, evt);
                    break;
            }
        }
#endif
        void Invoke(KeyboardNavigationOperation operation, EventBase evt)
        {
            if (operation == KeyboardNavigationOperation.None)
                return;

            m_Action?.Invoke(operation, evt);
        }
    }
}
#endif