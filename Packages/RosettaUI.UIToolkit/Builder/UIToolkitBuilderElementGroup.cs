using System;
using RosettaUI.Reactive;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        
        private VisualElement Build_DynamicElement(Element element)
        {
            var ve = new VisualElement();
            ve.AddToClassList(UssClassName.DynamicElement);
            return Build_ElementGroupContents(ve, element);
        }

        private VisualElement Build_Window(Element element)
        {
            var windowElement = (WindowElement) element;
            var window = new Window();
            window.TitleBarContainerLeft.Add(Build(windowElement.bar));
            window.closeButton.clicked += () => windowElement.Enable = !windowElement.Enable;

            windowElement.isOpenRx.SubscribeAndCallOnce(isOpen =>
            {
                if (isOpen) window.Show();
                else window.Hide();
            });

            return Build_ElementGroupContents(window, element);
        }

        private VisualElement Build_Fold(Element element)
        {
            var foldElement = (FoldElement) element;
            var fold = new Foldout();

            var toggle = fold.Q<Toggle>();
            toggle.Add(Build(foldElement.bar));
 
            foldElement.isOpenRx.SubscribeAndCallOnce(isOpen => fold.value = isOpen);

            return Build_ElementGroupContents(fold, foldElement);
        }

        private VisualElement Build_WindowLauncher(Element element)
        {
            var launcherElement = (WindowLauncherElement) element;
            var windowElement = launcherElement.Window;
            var window = (Window) Build(windowElement);

            //var toggle = Build_Field<bool, Toggle>(element, false);
            var toggle = CreateField<bool, Toggle>(launcherElement);
            toggle.AddToClassList(UssClassName.WindowLauncher);
            toggle.RegisterCallback<PointerUpEvent>(evt =>
            {
                // panel==null（初回）はクリックした場所に出る
                // 以降は以前の位置に出る
                // Toggleの値が変わるのはこのイベントの後
                if (!windowElement.Enable && window.panel == null) window.Show(evt.originalMousePosition, toggle);
            });

            launcherElement.label.valueRx.SubscribeAndCallOnce((v) => toggle.text = v);

            return toggle;
        }

        private VisualElement Build_Row(Element element)
        {
            var row = CreateRowVisualElement();

            return Build_ElementGroupContents(row, element, (ve, i) =>
            {
                //ve.AddToClassList(UssClassName.RowContents);
                if (i == 0) ve.AddToClassList(UssClassName.RowContentsFirst);
            });
        }

        private static VisualElement CreateRowVisualElement()
        {
            var row = new VisualElement();
            row.AddToClassList(UssClassName.Row);
            return row;
        }

        private VisualElement Build_Column(Element element)
        {
            var column = new VisualElement();
            //column.AddToClassList(FieldClassName.Column);

            return Build_ElementGroupContents(column, element);
        }

        private VisualElement Build_Box(Element element)
        {
            var box = new Box();
            return Build_ElementGroupContents(box, element);
        }

        VisualElement Build_ScrollView(Element element)
        {
            var scrollView = new ScrollView(); // TODO: support horizontal. ScrollViewMode.VerticalAndHorizontal may not work correctly 
            return Build_ElementGroupContents(scrollView, element);
        }

        VisualElement Build_Indent(Element element)
        {
            var ve = new VisualElement();
            ve.AddToClassList(UssClassName.Indent);
            ve.style.marginLeft = LayoutSettings.IndentSize;
            return Build_ElementGroupContents(ve, element);
        }

        private VisualElement Build_CompositeField(Element element)
        {
            var compositeFieldElement = (CompositeFieldElement) element;

            var field = new VisualElement();
            field.AddToClassList(UssClassName.UnityBaseField);
            field.AddToClassList(UssClassName.CompositeField);

            var labelElement = compositeFieldElement.bar;
            if (labelElement != null)
            {
                var label = Build(labelElement);
                label.AddToClassList(UssClassName.UnityBaseFieldLabel);
                field.Add(label);
            }

            var contentContainer = new VisualElement();
            contentContainer.AddToClassList(UssClassName.CompositeFieldContents);
            field.Add(contentContainer);
            Build_ElementGroupContents(contentContainer, element, (ve, idx) =>
            {
                if (idx == 0)
                {
                    ve.AddToClassList(UssClassName.CompositeFieldFirstChild);
                }
            });

            return field;
        }

        private VisualElement Build_ElementGroupContents(VisualElement container, Element element,
            Action<VisualElement, int> setupContentsVe = null)
        {
            var i = 0;
            foreach (var ve in Build_ElementGroupContents((ElementGroup) element))
            {
                setupContentsVe?.Invoke(ve, i);
                container.Add(ve);
                i++;
            }

            return container;
        }

    }
}