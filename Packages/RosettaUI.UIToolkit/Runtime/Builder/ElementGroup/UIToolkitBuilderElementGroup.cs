using System;
using System.Linq;
using RosettaUI.Builder;
using RosettaUI.Reactive;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private static void ApplyMinusIndentIfPossible(VisualElement ve, Element element)
        {
            // Indentがあるなら１レベルキャンセル
            if (element.CanMinusIndent())
            {
                ve.style.marginLeft = -LayoutSettings.IndentSize;
            }
        }

        private static void ApplyIndent(VisualElement ve, int indentLevel = 1)
        {
            ve.style.marginLeft = LayoutSettings.IndentSize * indentLevel;
        }

        private VisualElement Build_ElementGroupContents(VisualElement container, Element element,
            Action<VisualElement, int> setupContentsVe = null)
        {
            var group = (ElementGroup) element;

            container.name = group.DisplayName;

            var i = 0;
            foreach (var ve in Build_ElementGroupContents(group))
            {
                setupContentsVe?.Invoke(ve, i);
                container.Add(ve);
                i++;
            }

            return container;
        }
        
        // ElementGroupのBind
        // 型チェックあり
        private bool Bind_ElementGroup<TElementGroup, TVisualElement>(Element element, VisualElement visualElement)
            where TElementGroup : ElementGroup
            where TVisualElement : VisualElement
        {
            if (element is not TElementGroup elementGroup || visualElement is not TVisualElement) return false;
            return Bind_ElementGroupContents(elementGroup, visualElement);
        }

        // ElementGroupのBind
        // visualElementの子供にBindできない場合はBuildで生成する
        private bool Bind_ElementGroupContents(ElementGroup elementGroup, VisualElement visualElement,
            Action<Element, VisualElement, int> bindChild = null)
        {
            visualElement.name = elementGroup.DisplayName;
            
            var contentCount = elementGroup.Contents.Count();
            var visualElementCount = visualElement.childCount;

            for (var i = visualElementCount - 1; i >= contentCount; --i)
            {
                visualElement.RemoveAt(i);
            }

            using var pool = ListPool<VisualElement>.Get(out var veList);
            veList.AddRange(visualElement.Children());

            foreach (var (e,idx) in elementGroup.Contents.Select((e,idx) => (e,idx)))
            {
                if (idx < veList.Count)
                {
                    var ve = veList[idx];
                    var success = Bind(e, ve);
                    if (success) continue;

                    visualElement.RemoveAt(idx);
                }

                var newVe = Build(e);
                Assert.IsNotNull(newVe);
                
                visualElement.Insert(idx, newVe);
            }

            return true;
        }
    }
}