using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RosettaUI.UGUI.Builder
{
    public class Fold : UIBehaviour
    {
        public UnityEngine.UI.Button button;
        public GameObject foldArrow;
        public GameObject unfoldArrow;
        public GameObject contents;

        public UnityEvent onOpen;
        public UnityEvent onClose;

        protected bool isOpen = true;

        public bool IsOpen {
            get => isOpen;
            set
            {
                if (isOpen != value)
                {
                    isOpen = value;
                    ApplyOpenClose();
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            button.onClick.AddListener(() => IsOpen = !IsOpen);
        }

        void ApplyOpenClose()
        {
            foldArrow.SetActive(!IsOpen);
            unfoldArrow.SetActive(IsOpen);

            contents.SetActive(IsOpen);

            if (isOpen) onOpen.Invoke();
            else onClose.Invoke();
        }


        public void SetTitleContents(Transform trans)
        {
            trans.SetParent(button.transform);
        }

        public void SetContents(Transform trans)
        {
            contents = trans.gameObject;
            trans.SetParent(transform);
        }
    }
}
