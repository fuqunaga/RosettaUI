using UnityEngine;
using UnityEngine.EventSystems;


namespace RosettaUI.UGUI.Builder
{
    [RequireComponent(typeof(RectTransform))]
    public class DragObject : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        Vector2 originalLocalPointerPosition;
        Vector3 originalPanelLocalPosition;
        RectTransform panelRectTransform;
        protected RectTransform parentRectTransform;

        bool dragging;

        void Start()
        {
            panelRectTransform = transform as RectTransform;
            parentRectTransform = panelRectTransform.parent as RectTransform;
        }

        public virtual void OnPointerDown(PointerEventData data)
        {
            originalPanelLocalPosition = panelRectTransform.localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, data.position, data.pressEventCamera, out originalLocalPointerPosition);

            dragging = true;

            data.Use();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            dragging = false;
        }

        public void OnDrag(PointerEventData data)
        {
            if (panelRectTransform == null || parentRectTransform == null || !dragging)
                return;

            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, data.position, data.pressEventCamera, out localPointerPosition))
            {
                Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
                panelRectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;
            }

            ClampToWindow();

            data.Use();
        }

        // Clamp panel to area of parent
        void ClampToWindow()
        {
            Vector3 pos = panelRectTransform.localPosition;

            Vector3 minPosition = parentRectTransform.rect.min - panelRectTransform.rect.min;
            Vector3 maxPosition = parentRectTransform.rect.max - panelRectTransform.rect.max;

            pos.x = Mathf.Clamp(panelRectTransform.localPosition.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(panelRectTransform.localPosition.y, minPosition.y, maxPosition.y);

            panelRectTransform.localPosition = pos;
        }
    }
}