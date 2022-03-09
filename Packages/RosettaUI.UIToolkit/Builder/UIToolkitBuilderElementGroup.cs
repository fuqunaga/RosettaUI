using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Builder;
using RosettaUI.Reactive;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public partial class UIToolkitBuilder
    {
        private VisualElement Build_DynamicElement(Element element)
        {
            var ve = new VisualElement();
            return Build_ElementGroupContents(ve, element);
        }

        private VisualElement Build_Window(Element element)
        {
            var windowElement = (WindowElement) element;
            var window = new Window();
            window.TitleBarContainerLeft.Add(Build(windowElement.header));
            window.CloseButton.clicked += () => windowElement.Enable = !windowElement.Enable;

            windowElement.IsOpenRx.SubscribeAndCallOnce(isOpen =>
            {
                if (isOpen)
                {
                    window.BringToFront();
                    window.Show();
                }
                else
                {
                    window.Hide();
                }
            });
            
            
            // Focusable.ExecuteDefaultEvent() 内の this.focusController?.SwitchFocusOnEvent(evt) で
            // NavigationMoveEvent 方向にフォーカスを移動しようとする
            // キー入力をしている場合などにフォーカスが移ってしまうのは避けたいのでWindow単位で抑制しておく
            // UnityデフォルトでもTextFieldは抑制できているが、IntegerField.inputFieldでは出来ていないなど挙動に一貫性がない
            window.RegisterCallback<NavigationMoveEvent>(evt => evt.PreventDefault());

            Build_ElementGroupContents(window.contentContainer, element);

            // add box shadow
            window.AddBoxShadow();
            
            return window;
        }

        private VisualElement Build_Fold(Element element)
        {
            var foldElement = (FoldElement) element;
            var fold = new Foldout();
            
            var toggle = fold.Q<Toggle>();
            toggle.Add(Build(foldElement.header));
            
            // disable 中でもクリック可能
            UIToolkitUtility.SetAcceptClicksIfDisabled(toggle);
            
            // Foldout 直下の Toggle は marginLeft が default.uss で書き換わるので上書きしておく
            // セレクタ例： .unity-foldout--depth-1 > .unity-fold__toggle
            toggle.style.marginLeft = 0;
            
            // Indentがあるなら１レベルキャンセル
            ApplyMinusIndentIfPossible(fold, foldElement);
            
            foldElement.IsOpenRx.SubscribeAndCallOnce(isOpen => fold.value = isOpen);
            fold.RegisterValueChangedCallback(evt =>
            {
                if (evt.target == fold)
                {
                    foldElement.IsOpen = evt.newValue;
                }
            });

            var ret =  Build_ElementGroupContents(fold, foldElement);
            return ret;
        }


        private static readonly Dictionary<WindowElement, List<WindowElement>> WindowLauncherOpenedWindowTable = new();

        private VisualElement Build_WindowLauncher(Element element)
        {
            var launcherElement = (WindowLauncherElement) element;
            var windowElement = launcherElement.window;
            var window = (Window) Build(windowElement);

            var toggle = Build_Field<bool, Toggle>(launcherElement, false);
            toggle.AddToClassList(UssClassName.WindowLauncher);
            toggle.RegisterCallback<PointerUpEvent>(OnPointUpEventFirst);

            var labelElement = launcherElement.label;
            labelElement.SubscribeValueOnUpdateCallOnce(v => toggle.text = v);

            

            return toggle;
            
            

            // ほかのWindowにかぶらない位置を計算する
            // 一度ドラッグしたWindowはその位置を覚えてこの処理の対象にはならない
            void OnPointUpEventFirst(PointerUpEvent evt)
            {
                // Toggleの値が変わるのはこのイベントの後
                if (windowElement.Enable) return;
                
                var pos = evt.originalMousePosition;
                var parentWindowElement = launcherElement.Parents().OfType<WindowElement>().FirstOrDefault();

                // Auto layout
                if (parentWindowElement != null && GetUIObj(parentWindowElement) is Window parentWindow)
                {
                    const float delta = 5f;
                    var area = parentWindow.panel.visualTree.layout;
                    
                    var rect = parentWindow.layout;
                    var x = rect.xMax + delta;
                    if (x < area.xMax)
                    {
                        pos = new Vector2(x, rect.yMin);
                    }

                    if (!WindowLauncherOpenedWindowTable.TryGetValue(parentWindowElement, out var openedWindows))
                    {
                        openedWindows = WindowLauncherOpenedWindowTable[parentWindowElement] = new List<WindowElement>();
                    }

                    var lastIndex = openedWindows.FindLastIndex(w => w is {IsOpen: true} && GetUIObj(w) is Window {IsMoved: false});
                    if (lastIndex >= 0)
                    {
                        var removeIndex = lastIndex + 1;
                        openedWindows.RemoveRange(removeIndex, openedWindows.Count - removeIndex);
                    }

                    var lastOpenedWindowElement = openedWindows.LastOrDefault();
                    if (lastOpenedWindowElement != null &&  GetUIObj(lastOpenedWindowElement) is Window targetWindow)
                    {
                        var targetWindowRect = targetWindow.layout;

                        if ( Vector2.Distance(pos,targetWindowRect.position) < delta)
                        {
                            
                            var areaYHalf = area.center.y;

                            var yMax = targetWindowRect.yMax;
                            if (yMax < areaYHalf)
                            {
                                pos.y = yMax + delta;
                            }
                            else
                            {
                                var newX = targetWindowRect.xMax + delta; 
                                if (newX < area.xMax)
                                {
                                    pos.x = newX;
                                }
                            }
                        }
                    }
                    
                    openedWindows.Add(windowElement);
                }


                window.Show(pos, toggle);
            }
        }

        private VisualElement Build_Row(Element element)
        {
            var row = CreateRowVisualElement();

            return Build_ElementGroupContents(row, element);
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
            column.AddToClassList(UssClassName.Column);

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
            var scrollViewElement = (ScrollViewElement) element;
            var scrollViewMode = GetScrollViewMode(scrollViewElement.type);
            
            var scrollView = new ScrollView(scrollViewMode); 
            return Build_ElementGroupContents(scrollView, element);
            
            
            static ScrollViewMode GetScrollViewMode(ScrollViewType type)
            {
                return type switch
                {
                    ScrollViewType.Vertical => ScrollViewMode.Vertical,
                    ScrollViewType.Horizontal => ScrollViewMode.Horizontal,
                    ScrollViewType.VerticalAndHorizontal => ScrollViewMode.VerticalAndHorizontal,
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
            }
        }

        VisualElement Build_Indent(Element element)
        {
            var indentElement = (IndentElement) element;

            var ve = new VisualElement();
            ApplyIndent(ve, indentElement.level);

            return Build_ElementGroupContents(ve, element);
        }

        private VisualElement Build_CompositeField(Element element)
        {
            var compositeFieldElement = (CompositeFieldElement) element;

            var field = new VisualElement();
            field.AddToClassList(UssClassName.UnityBaseField);
            field.AddToClassList(UssClassName.CompositeField);

            var labelElement = compositeFieldElement.header;
            if (labelElement != null)
            {
                var label = Build(labelElement);
                label.AddToClassList(UssClassName.UnityBaseFieldLabel);
                field.Add(label);
            }

            var contentContainer = new VisualElement();
            contentContainer.AddToClassList(UssClassName.CompositeFieldContents);
            field.Add(contentContainer);
            Build_ElementGroupContents(contentContainer, element);

            return field;
        }


        private VisualElement Build_ListView(Element element)
        {
            var listViewElement = (ListViewElement) element;

            var listView = new ListViewCustom(listViewElement.GetIList(),
                makeItem: () => new VisualElement(),
                bindItem: BindItem
            )
            {
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showFoldoutHeader = true,
                showAddRemoveFooter = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                unbindItem = UnbindItem
            };
            
            
            ApplyMinusIndentIfPossible(listView, listViewElement);
            listView.ScheduleToUseResolvedLayoutBeforeRendering(() => ApplyIndent(listView.Q<Foldout>().contentContainer));
            
            listViewElement.Label.SubscribeValueOnUpdateCallOnce(str => listView.headerTitle = str);

            var lastListItemCount = listViewElement.GetListItemCount();
            
            listView.itemsSourceChanged += () =>
            {
                listViewElement.SetIList(listView.itemsSource);
                lastListItemCount = listViewElement.GetListItemCount();
            };
            
            listViewElement.onUpdate += _=>
            {
                var listItemCount = listViewElement.GetListItemCount();
                if ( lastListItemCount != listItemCount)
                {
                    lastListItemCount = listItemCount;
                    listView.OnListSizeChangedExternal();
                }
            };


            return listView;

            
            void BindItem(VisualElement ve, int idx)
            {
                ve.Clear();

                var e = listViewElement.GetOrCreateItemElement(idx);
                e.SetEnable(true);

                var itemVe = GetUIObj(e);
                if (itemVe == null)
                {
                    itemVe = Build(e);
                    ApplyIndent(itemVe);
                }
                else
                {
                    e.Update();　// 表示前に最新の値をUIに通知
                }

                ve.Add(itemVe);
            }
            
            void UnbindItem(VisualElement _, int idx)
            {
                var e = listViewElement.GetOrCreateItemElement(idx);
                e.SetEnable(false);
            }
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


        static void ApplyMinusIndentIfPossible(VisualElement ve, Element element)
        {
            // Indentがあるなら１レベルキャンセル
            if (element.CanMinusIndent())
            {
                ve.style.marginLeft = -LayoutSettings.IndentSize;
            }
        }

        static void ApplyIndent(VisualElement ve, int indentLevel = 1)
        {
            ve.style.marginLeft = LayoutSettings.IndentSize * indentLevel;
        }
    }
}