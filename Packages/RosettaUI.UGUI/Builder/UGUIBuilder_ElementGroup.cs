using RosettaUI.Reactive;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RosettaUI.UGUI.Builder
{
    public static partial class UGUIBuilder
    {
        static GameObject Build_Column(Element element)
        {
            return impl.Build_ElementGroup(element, null, true, (go) => AddLayoutGroup<VerticalLayoutGroup>(go));
        }


        static GameObject Build_Row(Element element)
        {
            return impl.Build_ElementGroup(element, null, true, (go) => AddHorizontalLayoutGroup(element, go));
        }

        static void AddHorizontalLayoutGroup(Element element, GameObject go)
        {
            var layoutGroup = AddLayoutGroup<HorizontalLayoutGroup>(go);

            layoutGroup.spacing = settings.rowSpacing;
            layoutGroup.padding = CalcPadding(element.IsTopLevelRow(), CalcSelfIndent(element), element.IsTopLevelRow());
        }

        /// <summary>
        /// element用のRowを生成
        /// elementはRowでなくてもよい
        /// </summary>
        static GameObject Build_RowAt(string displayName, Element element)
        {
            var go = Instantiate(displayName);
            AddHorizontalLayoutGroup(element, go);

            return go;
        }

        static GameObject Build_Fold(Element element)
        {
            var foldElement = (FoldElement)element;

            // TODO: supports foldElement.bar
            var go = Instantiate(foldElement.bar.FirstLabel()?.Value, resource.fold);
            var trans = go.transform;

            // build title row
            var fold = go.GetComponentInChildren<Fold>();
            fold.foldArrow.GetComponent<Image>().color = settings.theme.foldArrowColor;
            fold.unfoldArrow.GetComponent<Image>().color = settings.theme.foldArrowColor;

            var titleLayoutGroup = fold.button.GetComponent<HorizontalOrVerticalLayoutGroup>();
            titleLayoutGroup.padding = CalcPadding(true, CalcSelfIndent(foldElement), false);
            var titleGo = Build_Label(foldElement.bar);
            fold.SetTitleContents(titleGo.transform);

            // build contents
            var column = Instantiate(nameof(FoldElement.Contents));
            AddLayoutGroup<VerticalLayoutGroup>(column);
            fold.SetContents(column.transform);

            foreach (var content in foldElement.Contents)
            {
                var contentsGo = impl.Build(content);
                contentsGo.transform.SetParent(column.transform);
            }

            // set callback
            fold.onOpen.AddListener(() => foldElement.IsOpen = true);
            fold.onClose.AddListener(() => foldElement.IsOpen = false);

            //RegisterSetFoldOpen(foldElement, (isOpen) => fold.IsOpen = isOpen);
            foldElement.IsOpenRx.Subscribe((isOpen) => fold.IsOpen = isOpen);


            return go;
        }


        static int CalcSelfIndent(Element element)
        {
            var indent = element.GetIndent();
            var parent = element.Parent;
            while (parent != null)
            {
                if (parent is Row)
                {
                    indent -= parent.GetIndent();
                    break;
                }

                parent = parent.Parent;
            }

            return indent;
        }


        static RectOffset CalcPadding(bool isTopLevel, int indentLevel, bool useFoldIconPadding)
        {

            int toplevel = isTopLevel ? settings.paddingTopLevel : 0;
            int foldIconLeft = useFoldIconPadding ? settings.paddingFoldIconLeft : 0;
            int standardLeft = toplevel + foldIconLeft + indentLevel * settings.paddingIndent;
            return new RectOffset(standardLeft, toplevel, 0, 0);
        }

        static HorizontalOrVerticalLayoutGroup AddLayoutGroup<T>(GameObject go)
            where T : HorizontalOrVerticalLayoutGroup
        {
            var layoutGroup = go.AddComponent<T>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            return layoutGroup;
        }

    }
}