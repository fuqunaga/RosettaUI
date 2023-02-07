using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        public class ClickEventUIToolkit : IClickEvent
        {
            #region Pool
            
            private static readonly ObjectPool<ClickEventUIToolkit> ObjectPool = new(
                createFunc: () => new ClickEventUIToolkit(),
                actionOnGet: evt => evt.Clear()
            );

            public static PooledObject<ClickEventUIToolkit> GetPool(out ClickEventUIToolkit evt) =>
                ObjectPool.Get(out evt);
            
            #endregion

            public int Button => ClickEvent.button;
            public Vector2 Position => ClickEvent.position;

            public IPointerEvent ClickEvent { get; protected set; }

            public void Init(IPointerEvent clickEvent) => ClickEvent = clickEvent;
            public void Clear() => ClickEvent = null;
        }
        
        private bool Bind_ClickableElement(Element element, VisualElement visualElement)
        {
            // (visualElement is not VisualElement) is not work
            // visualElementがVisualElementを継承したクラスでもtrueになってしまうのでGetType()で厳密にチェックする
            if (element is not ClickableElement clickableElement || visualElement.GetType() != typeof(VisualElement)) return false;

            visualElement.RegisterCallback<PointerDownEvent>(OnClick);

            clickableElement.GetViewBridge().onUnsubscribe += () =>
            {
                visualElement.UnregisterCallback<PointerDownEvent>(OnClick);
            };

            void OnClick(PointerDownEvent evt)
            {
                using var _ = ClickEventUIToolkit.GetPool(out var clickEvent);
                clickEvent.Init(evt);
                clickableElement.OnClick(clickEvent);
                
                evt.StopPropagation();
            }

            return Bind_ElementGroupContents(clickableElement, visualElement);;
        }
    }
}