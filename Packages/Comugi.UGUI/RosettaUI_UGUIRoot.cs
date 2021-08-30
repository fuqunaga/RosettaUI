using RosettaUI.UGUI.Builder;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RosettaUI.UGUI
{
    [RequireComponent(typeof(Canvas))]
    public class RosettaUI_UGUIRoot : MonoBehaviour
    {
        public UGUIResource resource;
        public UGUISettings settings;
        public bool selectFirstElementOnEnable;

        public Vector2 rootObjPositionOnScreen = new Vector2(50f, -50f);
        public Vector3 rootObjPoistionOnWorldView = new Vector3(-0.5f, 0.2f, 1f);

        Element rootElement;
        RectTransform rootElementRect;
        Canvas _canvas;
        Canvas canvas => (_canvas != null) ? _canvas : (_canvas = GetComponent<Canvas>());
        bool needAdjust;


        #region Unity

        void OnEnable()
        {
            if (selectFirstElementOnEnable)
            {
                StartCoroutine(SelectCoroutine());
            }

            needAdjust = true; // このときはまだcanvas.worldCameraの位置が正しくないケースがある
        }

        Selectable lastSelected;

        void OnDisable()
        {
            var go = EventSystem.current?.currentSelectedGameObject;
            if (go != null)
            {
                lastSelected = go.GetComponent<Selectable>();
            }
        }

        private void Update()
        {
            rootElement?.Update();
        }

        void LateUpdate()
        {
            if (needAdjust)
            {
                AdjustCanvasAndRootElement();
                needAdjust = false;
            }
        }

        #endregion


        IEnumerator SelectCoroutine()
        {
            yield return null;

            var selectable = lastSelected != null
                ? lastSelected
                : GetComponentInChildren<Selectable>();

            if (selectable != null)
            {
                selectable.Select();
                selectable.OnSelect(null);
            }
        }

        public void Build(Element element)
        {
            UGUIBuilder.resource = resource;
            UGUIBuilder.settings = settings;

            var go = UGUIBuilder.Build(element, transform);
            rootElementRect = go.transform as RectTransform;
            rootElement = element;

            needAdjust = true;
        }

        void AdjustCanvasAndRootElement()
        {
            var worldCamera = canvas.worldCamera;

            // Adjust canvas position,rotation
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                var camTrans = worldCamera.transform;
                var pos = camTrans.TransformPoint(Vector3.forward * rootObjPoistionOnWorldView.z);
                var rot = Quaternion.LookRotation(camTrans.forward); // force upvector = Vector3.Up;

                transform.SetPositionAndRotation(pos, rot);


                // Adjust window position
                if (worldCamera != null)
                {
                    var worldCameraTrans = worldCamera.transform;
                    rootElementRect.position = worldCameraTrans.TransformPoint(rootObjPoistionOnWorldView);
                }
            }
            else
            {
                rootElementRect.anchoredPosition = rootObjPositionOnScreen;
            }
        }
    }
}