using System;
using System.Diagnostics;
using System.Linq;
using RosettaUI.Reactive;
using RosettaUI.UIToolkit.UnityInternalAccess;
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
            window.CloseButton.clicked += () => windowElement.Enable = !windowElement.Enable;

            windowElement.enableRx.SubscribeAndCallOnce(isOpen =>
            {
                if (isOpen) window.Show();
                else window.Hide();
            });

            Build_ElementGroupContents(window.contentContainer, element);
            return window;
        }

        private VisualElement Build_Fold(Element element)
        {
            var foldElement = (FoldElement) element;
            var fold = new Foldout();
            
            var toggle = fold.Q<Toggle>();
            toggle.Add(Build(foldElement.bar));
            
            // disable 中でもクリック可能
            UIToolkitUtility.SetAcceptClicksIfDisabled(toggle);
            
            // Foldout 直下の Toggle は marginLeft が default.uss で書き換わるので上書きしておく
            // セレクタ例： .unity-foldout--depth-1 > .unity-fold__toggle
            toggle.style.marginLeft = 0;
            
            foldElement.IsOpenRx.SubscribeAndCallOnce(isOpen => fold.value = isOpen);
            fold.RegisterValueChangedCallback(evt =>
            {
                if (evt.target == fold)
                {
                    foldElement.IsOpen = evt.newValue;
                }
            });

            return Build_ElementGroupContents(fold, foldElement);
        }

        private VisualElement Build_WindowLauncher(Element element)
        {
            var launcherElement = (WindowLauncherElement) element;
            var windowElement = launcherElement.Window;
            var window = (Window) Build(windowElement);

            var toggle = Build_Field<bool, Toggle>(launcherElement, false);
            toggle.AddToClassList(UssClassName.WindowLauncher);
            toggle.RegisterCallback<PointerUpEvent>(evt =>
            {
                // panel==null（初回）はクリックした場所に出る
                // 以降は以前の位置に出る
                // Toggleの値が変わるのはこのイベントの後
                if (!windowElement.Enable && window.panel == null) window.Show(evt.originalMousePosition, toggle);
            });

            var labelElement = launcherElement.Label;
            labelElement.SubscribeValueOnUpdateCallOnce(v => toggle.text = v);

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
        
        private VisualElement Build_HelpBox(Element element)
        {
            var helpBoxElement = (HelpBoxElement) element;
        
            var helpBox = new HelpBox(null, GetHelpBoxMessageType(helpBoxElement.helpBoxType));
            helpBoxElement.label.SubscribeValueOnUpdateCallOnce(str => helpBox.text = str);

            return helpBox;

            static HelpBoxMessageType GetHelpBoxMessageType(HelpBoxType helpBoxType)
            {
                return helpBoxType switch
                {
                    HelpBoxType.None => HelpBoxMessageType.None,
                    HelpBoxType.Info => HelpBoxMessageType.Info,
                    HelpBoxType.Warning => HelpBoxMessageType.Warning,
                    HelpBoxType.Error => HelpBoxMessageType.Error,
                    _ => throw new ArgumentOutOfRangeException(nameof(helpBoxType), helpBoxType, null)
                };
            }
        }
        

        VisualElement Build_ScrollView(Element element)
        {
            var scrollView = new ScrollView(); // TODO: support horizontal. ScrollViewMode.VerticalAndHorizontal may not work correctly 
            return Build_ElementGroupContents(scrollView, element);
        }

        VisualElement Build_Indent(Element element)
        {
            var ve = new VisualElement();
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

        private VisualElement Build_ElementGroupContents(VisualElement container, Element element, Action<VisualElement, int> setupContentsVe = null)
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

    }
}