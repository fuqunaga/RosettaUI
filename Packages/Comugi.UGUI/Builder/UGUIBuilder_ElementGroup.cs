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
            return Build_ElementGroup(element, null, true, (go) => AddLayoutGroup<VerticalLayoutGroup>(go));
        }


        static GameObject Build_Row(Element element)
        {
            return Build_ElementGroup(element, null, true, (go) => AddHorizontalLayoutGroup(element, go));
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
            var foldElement = element as FoldElement;
            var contentsElement = foldElement.contents;

            var go = Instantiate(foldElement.title.GetInitialValue(), resource.fold);
            var trans = go.transform;

            // build title row
            var fold = go.GetComponentInChildren<Fold>();
            fold.foldArrow.GetComponent<Image>().color = settings.theme.foldArrowColor;
            fold.unfoldArrow.GetComponent<Image>().color = settings.theme.foldArrowColor;

            var titleLayoutGroup = fold.button.GetComponent<HorizontalOrVerticalLayoutGroup>();
            titleLayoutGroup.padding = CalcPadding(true, CalcSelfIndent(foldElement), false);
            var titleGo = Build_Label(foldElement.title);
            fold.SetTitleContents(titleGo.transform);

            // build contents
            var contentsGo = impl.Build(contentsElement);
            contentsGo.transform.SetParent(trans);
            fold.SetContents(contentsGo.transform);

            // set callback
            fold.onOpen.AddListener(() => foldElement.isOpen = true);
            fold.onClose.AddListener(() => foldElement.isOpen = false);

            //RegisterSetFoldOpen(foldElement, (isOpen) => fold.IsOpen = isOpen);
            foldElement.isOpenRx.Subscribe((isOpen) => fold.IsOpen = isOpen);


            return go;
        }


        static GameObject Build_ElementGroup(Element element, GameObject prefab, bool useDisplayName = false, Action<GameObject> addComponentFunc = null)
        {
            var elementGroup = (ElementGroup)element;

            var name = useDisplayName ? elementGroup.displayName : element.GetType().Name;

            var go = Instantiate(name, prefab);
            addComponentFunc?.Invoke(go);

            var trans = go.transform;
            //elementGroup.BuildChildElements();
            foreach (var e in elementGroup.Elements)
            {
                var childGo = impl.Build(e);
                childGo.transform.SetParent(trans);
            }

            return go;
        }

        static int CalcSelfIndent(Element element)
        {
            var indent = element.GetIndent();
            var parent = element.parentGroup;
            while (parent != null)
            {
                if (parent is Row)
                {
                    indent -= parent.GetIndent();
                    break;
                }

                parent = parent.parentGroup;
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