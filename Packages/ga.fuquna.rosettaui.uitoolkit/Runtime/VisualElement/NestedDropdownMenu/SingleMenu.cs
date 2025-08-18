#nullable  enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.NestedDropdownMenuSystem
{
    /// <summary>
    /// Enhanced GenericDropdownMenu with support for hover-delayed submenu popup functionality.
    /// </summary>
    /// <remarks>
    /// GenericDropdownMenuを部分的に使用してサブメニューを表示するための機能を追加したクラス
    /// ルートメニューのみMenuContainer（画面全体を覆うコンテナ）を使用し、
    /// サブメニューはOuterContainerをルートメニューのMenuContainerにAddする
    ///
    /// イベントコールバック
    /// - GenericDropdownMenuのイベントコールバックは解除し独自のコールバックを読んでいる
    ///
    /// ポインターイベント 
    /// - GenericDropdownMenuはポインターのイベントをMenuContainerで受けるがここでは各メニューのOuterContainerで受けるようにしている
    /// - ルートメニューのMenuContainerに独自のOnPointerDownOnRoot,OnPointerMoveOnRootを登録している
    /// -以上の設定で次の挙動を実現している
    /// -- サブメニュー上にポインターがあればそのポインターイベントが呼ばれイベント終了
    /// -- すべての子要素上にポインターがない場合、OnPointerDownOnRoot,OnPointerMoveOnRootが呼ばれる
    /// --- OnPointerDownOnRoot：ルートメニューを閉じる
    /// --- OnPointerMoveOnRoot：CurrentLeafItemのOnPointerMove()を呼ぶ（アイテムの選択が外れる）
    ///
    /// ナビゲーションイベント
    /// - 各メニューのKeyboardNavigationManipulatorを使用している
    /// -- GenericDropdownMenuのApplyメソッドを呼び出し、追加処理をしている
    /// </remarks>
    public class SingleMenu : GenericDropdownMenu
    {
        #region Static

        // ReSharper disable InconsistentNaming
        // ReSharper disable MemberCanBePrivate.Global
        
        public const string subMenuItemUssClassName = "nested-dropdown__submenu-item";
        public const string rightArrowUssClassName = "nested-dropdown__right-arrow";
        
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore InconsistentNaming

        
        #region Private access
        
        private static readonly Func<GenericDropdownMenu, KeyboardNavigationOperation, bool> ApplyFunc;
        private static readonly Action<GenericDropdownMenu, PointerDownEvent> OnPointerDownFunc;
        private static readonly Action<GenericDropdownMenu, PointerMoveEvent> OnPointerMoveFunc;
        private static readonly Action<GenericDropdownMenu, PointerUpEvent> OnPointerUpFunc;
        private static readonly Action<GenericDropdownMenu, GeometryChangedEvent> OnContainerGeometryChangedFunc;
#if UNITY_6000_0_OR_NEWER
        private static readonly Action<GenericDropdownMenu, GeometryChangedEvent> OnInitialDisplayFunc;
#endif

        
        private static readonly Func<GenericDropdownMenu, int> GetSelectedIndexFunc;
        private static readonly Action<GenericDropdownMenu, bool> HideFunc;

        private static readonly MethodInfo OnAttachToPanelMethodInfo;
        private static readonly MethodInfo OnDetachFromPanelMethodInfo;
        
        static SingleMenu()
        {
            var methodInfos = typeof(GenericDropdownMenu).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            
            RegisterPrivateMethodToDelegate("Apply", out ApplyFunc);
            RegisterPrivateMethodToDelegate("OnPointerDown", out OnPointerDownFunc);
            RegisterPrivateMethodToDelegate("OnPointerMove", out OnPointerMoveFunc);
            RegisterPrivateMethodToDelegate("OnPointerUp", out OnPointerUpFunc);
            RegisterPrivateMethodToDelegate("OnContainerGeometryChanged", out OnContainerGeometryChangedFunc);
#if UNITY_6000_0_OR_NEWER
            RegisterPrivateMethodToDelegate("OnInitialDisplay", out OnInitialDisplayFunc);
#endif
            RegisterPrivateMethodToDelegate("GetSelectedIndex", out GetSelectedIndexFunc);
            RegisterPrivateMethodToDelegate("Hide", out HideFunc);

            OnAttachToPanelMethodInfo = GetPrivateMethodInfo("OnAttachToPanel");
            OnDetachFromPanelMethodInfo = GetPrivateMethodInfo("OnDetachFromPanel");
            
            return;


            void RegisterPrivateMethodToDelegate<TFunc>(string methodName, out TFunc func) where TFunc : Delegate
            {
                using var _ = ListPool<Type>.Get(out var typeList);
                
                // TFuncの2番目以降の引数の型を取得
                // ただしFunc<T>の場合は最後の一つは返り値なので無視
                typeList.AddRange(typeof(TFunc).GetGenericArguments().Skip(1));
                if (typeof(TFunc).Name.StartsWith("Func`"))
                {
                    typeList.RemoveAt(typeList.Count - 1);
                }
                
                //TFuncの2番目以降とMethodInfoのParametersの型が一致するものを選ぶ
                var methodInfo = methodInfos.Where(m => m.Name == methodName)
                    .FirstOrDefault(m => m.GetParameters().Select(p => p.ParameterType).SequenceEqual(typeList));

                Assert.IsNotNull(methodInfo, $"Method '{methodName}' not found in GenericDropdownMenu with parameters {string.Join(", ", typeList.Select(t => t.Name))}.");
                
                func = (TFunc)methodInfo.CreateDelegate(typeof(TFunc));
            }

            static MethodInfo GetPrivateMethodInfo(string name)
            {
                var methodInfo = typeof(GenericDropdownMenu).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.IsNotNull(methodInfo, $"Method '{name}' not found in GenericDropdownMenu.");
                return methodInfo;
            }
        }
        
        #endregion
        
        #endregion
        
        private readonly Dictionary<VisualElement, SingleMenu> _itemToSubMenuTable = new();
        private readonly VisualElement _outerContainer;
        private readonly VisualElement _scrollView;
        private KeyboardNavigationManipulator? _keyboardNavigationManipulator;

        private SingleMenu? _parentMenu;
        
        private bool IsRootMenu { get; set; }
        private bool IsActiveSubMenu => _parentMenu != null;
        private SingleMenu RootMenu => _parentMenu?.RootMenu ?? this;
        private VisualElement RootMenuContainer => RootMenu._outerContainer.GetFirstAncestorByClassName(ussClassName);
        private bool IsCurrentLeafMenu => _itemToSubMenuTable.Values.All(subMenu => subMenu._parentMenu == null);
        private SingleMenu CurrentLeafMenu
        {
            get
            {
                return _itemToSubMenuTable.Values
                    .Where(subMenu => subMenu.IsActiveSubMenu)
                    .Select(subMenu => subMenu.CurrentLeafMenu)
                    .FirstOrDefault() ?? this;
            }
        }
        
        
        public SingleMenu()
        {
            _outerContainer = contentContainer.GetFirstAncestorByClassName(containerOuterUssClassName);
            _scrollView = contentContainer.GetFirstAncestorByClassName(containerInnerUssClassName);
            
            // Unity2022だとスクロールバーが表示されてしまうので非表示にする
            if (_scrollView is ScrollView sv)
            {
                sv.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            }
            
            _outerContainer.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            _outerContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            // AttachToPanel,DetachFromPanelのコールバックをGenericDropdownMenuから削除する
            // ルートメニューだけでいいがルートメニューかどうかは表示されるまで確定せず、確定後コールバックが呼ばれるまでに削除するのは難しそうなのでここで一律削除する
            var menuContainer = _outerContainer.GetFirstAncestorByClassName(ussClassName);
            var onAttachToPanelDelegate = (EventCallback<AttachToPanelEvent>)OnAttachToPanelMethodInfo.CreateDelegate(typeof(EventCallback<AttachToPanelEvent>), this);
            var onDetachFromPanelDelegate = (EventCallback<DetachFromPanelEvent>)OnDetachFromPanelMethodInfo.CreateDelegate(typeof(EventCallback<DetachFromPanelEvent>), this);
            menuContainer.UnregisterCallback(onAttachToPanelDelegate);
            menuContainer.UnregisterCallback(onDetachFromPanelDelegate);
        }

        #region Callbacks
        
        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            IsRootMenu = _parentMenu == null;
            
            _keyboardNavigationManipulator = new KeyboardNavigationManipulator(OnNavigation);
            contentContainer.AddManipulator(_keyboardNavigationManipulator);
            
            _outerContainer.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _outerContainer.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _outerContainer.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _outerContainer.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);

            _scrollView.RegisterCallback<FocusOutEvent>(OnFocusOut);

            if (IsRootMenu)
            {
                var rootMenuContainer = _outerContainer.GetFirstAncestorByClassName(ussClassName);
                rootMenuContainer.RegisterCallback<PointerMoveEvent>(OnPointerMoveOnRoot);
                rootMenuContainer.RegisterCallback<PointerDownEvent>(OnPointerDownOnRoot);

                // GenericDropdownMenuのOnAttachToPanelで登録されるコールバックのうち必要なものをセット
                _scrollView.RegisterCallback<GeometryChangedEvent>(e => OnContainerGeometryChangedFunc(this, e));
#if UNITY_6000_0_OR_NEWER
                _scrollView.RegisterCallbackOnce<GeometryChangedEvent>(e => OnInitialDisplayFunc(this, e));
#endif
                
            }
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            contentContainer.RemoveManipulator(_keyboardNavigationManipulator);
            
            _outerContainer.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            _outerContainer.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _outerContainer.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            _outerContainer.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            
            _scrollView.UnregisterCallback<FocusOutEvent>(OnFocusOut);

            if (IsRootMenu)
            {
                var rootMenuContainer = _outerContainer.GetFirstAncestorByClassName(ussClassName);;
                rootMenuContainer.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                rootMenuContainer.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                
                // GenericDropdownMenuのOnDetachFromPanelで登録されるコールバックのうち必要なものをセット
                _scrollView.UnregisterCallback<GeometryChangedEvent>(e => OnContainerGeometryChangedFunc(this, e));
#if UNITY_6000_0_OR_NEWER
                _scrollView.RegisterCallbackOnce<GeometryChangedEvent>(e => OnInitialDisplayFunc(this, e));
#endif
            }
        }

        private void OnNavigation(KeyboardNavigationOperation operation, EventBase evt)
        {
            // イベントを消費しないとMoveRightなどでフォーカスが移動していまうので基本消費する
            // ApplyFunc()のみ実装に任せる
            var eventUsed = true;
            
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (operation)
            {
                // サブメニュー時、GenericDropdownMenuのApplyメソッドではルートメニューは閉じないのでここで閉じる
                case KeyboardNavigationOperation.Cancel:
                    HideRootMenu(true);
                    break;
                
                case KeyboardNavigationOperation.Submit:
                    var subMenuShown = ShowSubMenuIfSelectedItemIsSubMenu();
                    if(!subMenuShown)
                    {
                        ApplyFunc(this, operation);
                        HideRootMenu(true);
                    }
                    break;
                
                case KeyboardNavigationOperation.MoveRight:
                    ShowSubMenuIfSelectedItemIsSubMenu();
                    break;
                
                case KeyboardNavigationOperation.MoveLeft:
                    if (IsActiveSubMenu)
                    {
                        HideAsSubMenu();
                    }
                    break;
                
                default:
                    eventUsed = ApplyFunc(this, operation);
                    break;
            }

            if (eventUsed)
            {
                evt.StopPropagation();
            }

            return;
                
            bool ShowSubMenuIfSelectedItemIsSubMenu()
            {
                var selectedItem = GetSelectedItem();
                if (selectedItem != null && _itemToSubMenuTable.TryGetValue(selectedItem, out var submenu) && submenu._parentMenu == null)
                {
                    submenu.ShowAsSubMenu(this, selectedItem, true);
                    return true;
                }
                return false;
            }
        }
        
        private void OnPointerDown(PointerDownEvent evt)
        {
            OnPointerDownFunc(this, evt);
            PostPointerDownOrMove();
        }
        
        private void OnPointerMove(PointerMoveEvent evt)
        {
            OnPointerMoveFunc(this, evt);
            PostPointerDownOrMove();
        }

        private void PostPointerDownOrMove()
        {
            HideSubMenusForUnselectedItems();
            
            // どのアイテムも選択されなかった場合でアクティブなサブメニューがある場合は
            // そのアイテムの選択状態をキープ（どのメニューにも属さない微妙な隙間とかがありえる）
            SelectItem(GetActiveSubMenuItem());
        }
        
        private void OnPointerUp(PointerUpEvent evt)
        {
            // OnPointerUpFuncはSelectedItemがあればそのアクションを行ってHide()するが、
            // サブメニューアイテムとDisabledアイテムの場合は閉じないで欲しいのでOnPointerUpFuncを呼ばない
            var selectedItem = GetSelectedItem();
            var isActionable = selectedItem != null && 
                !_itemToSubMenuTable.ContainsKey(selectedItem) &&
                selectedItem.enabledSelf;
            
            if (isActionable)
            {
                OnPointerUpFunc(this, evt);

                if (IsActiveSubMenu)
                {
                    HideRootMenu(true);
                }
            }
            
            evt.StopPropagation();
        }
        
        
        // PointerLeaveEventのPreDispatchでホバー状態を解除されてしまう
        // アクティブなサブメニューがある場合は対応するアイテムをホバー（選択状態）にする
        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            var item = GetActiveSubMenuItem();
            if (item != null)
            {
                SelectItem(item);
            }
        }

        
        // GenericDropdownMenuのFocusOutEventを参照
        // フォーカスがメニューアイテムに移動しても強制的にスクロールビューに戻す
        private void OnFocusOut(FocusOutEvent evt)
        {
            if (IsCurrentLeafMenu && GetSelectedIndexFunc(this) >= 0)
            {
                _scrollView.schedule.Execute(() => contentContainer.Focus());
            }
        }

        
        private void OnPointerDownOnRoot(PointerDownEvent evt)
        {
            HideFunc(this, true);
        }
        
        private void OnPointerMoveOnRoot(PointerMoveEvent evt)
        {
            OnPointerMoveFunc(CurrentLeafMenu, evt);
            HideSubMenusForUnselectedItems();
        }
        
        #endregion
        
        private void HideRootMenu(bool giveFocusBack = false)
        {
            HideFunc(RootMenu, giveFocusBack);
        }
        
        private void HideSubMenusForUnselectedItems()
        {
            var selectedItem = GetSelectedItem();
            if (selectedItem == null) return;
            
            foreach(var (item, submenu) in _itemToSubMenuTable)
            {
                if ( item != selectedItem )
                {
                    submenu.HideAsSubMenu();
                }
            }
        }


        private static void SelectItem(VisualElement? element)
        {
            element?.AddPseudoStatesHover();
        }

        private VisualElement? GetActiveSubMenuItem()
        {
            return _itemToSubMenuTable.Where(pair => pair.Value.IsActiveSubMenu)
                .Select(pair => pair.Key)
                .FirstOrDefault();
        }
        
        private VisualElement? GetSelectedItem()
        {
            var selectedIndex = GetSelectedIndexFunc(this);
            if (selectedIndex < 0 || selectedIndex >= _scrollView.childCount)
            {
                return null;
            }
            
            return GetItems().ElementAt(selectedIndex);
        }

        private IEnumerable<VisualElement> GetItems()
        {
            return _scrollView.Children()
                .Where(ve => ve.ClassListContains(itemUssClassName));
        }

        public void AddSubMenuItem(string itemName, long delayMs, SingleMenu subMenu)
        {
            AddItem(itemName, false, null);
            var item = contentContainer.Children().Last();
            item.AddToClassList(subMenuItemUssClassName);

            var rightArrow = new VisualElement();
            rightArrow.AddToClassList(rightArrowUssClassName);
            rightArrow.pickingMode = PickingMode.Ignore;
            item.Add(rightArrow);
            
            _itemToSubMenuTable[item] = subMenu;
            
            // PointerEnterして一定時間経過後にサブメニューを表示する
            // PointerLeaveしたら時間計測をストップ
            IVisualElementScheduledItem? scheduledItem = null;

            item.RegisterCallback<PointerEnterEvent>(_ =>
            {
                scheduledItem ??= item.schedule.Execute(() => subMenu.ShowAsSubMenu(this, item));
                scheduledItem.ExecuteLater(delayMs);
            });

            item.RegisterCallback<PointerLeaveEvent>(_ => { scheduledItem?.Pause(); });

            item.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        private void ShowAsSubMenu(SingleMenu parentMenu, VisualElement targetElement, bool selectFirstItem = false)
        {
            if (_parentMenu != null) return;
            
            _parentMenu = parentMenu;
            
            _outerContainer.RegisterCallbackOnce<GeometryChangedEvent>(_ => EnsurePositionAsSubMenu(targetElement));
            
            RootMenuContainer.Add(_outerContainer);
            
            contentContainer.schedule.Execute(() => contentContainer.Focus());

            if (selectFirstItem)
            {
                SelectItem(GetItems().FirstOrDefault());
            }
        }
        
        /// <summary>
        /// RootMenuContainerローカル座標系ではみ出ないようにサブメニューの位置を調整する
        /// </summary>
        /// <param name="targetElement"></param>
        private void EnsurePositionAsSubMenu(VisualElement targetElement)
        {
            var targetWorldBound = targetElement.worldBound;
            
            var rootMenuContainer = _outerContainer.parent;
            var targetLocalBound = rootMenuContainer.WorldToLocal(targetWorldBound);
            var position = new Vector2(targetLocalBound.xMax, targetLocalBound.yMin);

            // firstItemとtargetElementのYの位置を揃える
            var firstItem = _scrollView.Children().FirstOrDefault();
            if (firstItem != null)
            {
                var fistItemWorldPosition = firstItem.worldBound.position;
                var firstItemPositionOnOuterContainer = _outerContainer.WorldToLocal(fistItemWorldPosition);
                position.y -= firstItemPositionOnOuterContainer.y;
            }
            
            
            var rootRect = rootMenuContainer.layout;
            var outerContainerRect = _outerContainer.layout;
            
            // 右側がルートからはみ出るようなら親メニューの左側に表示する
            // ただし左側がルートからはみ出すようならルートの左側に揃える
            if (position.x + outerContainerRect.width > rootRect.width)
            {
                position.x = Mathf.Max(0f, targetLocalBound.xMin - outerContainerRect.width);
            }
            
            // 下端がルートからはみ出るようならルートの下端に揃える
            // ただし上端がルートからはみ出すようならルートの上端に揃える
            if (position.y + outerContainerRect.height > rootRect.height)
            {
                position.y = Mathf.Max(0f, rootRect.height - outerContainerRect.height);
            }

            // OuterContainerの位置を調整
            // 実際の表示位置は、Style.left,topで指定した位置からMarginを含めて計算されるのであらかじめMarginを引いておく
            var resolvedStyle = _outerContainer.resolvedStyle;
            var style = _outerContainer.style;
            style.left = position.x - resolvedStyle.marginLeft;
            style.top = position.y - resolvedStyle.marginTop;
            
            // サブメニューがルートメニューより長い場合は縮めてスクロールビューに頼る
            if (outerContainerRect.height > rootRect.height)
            {
                style.maxHeight = rootRect.height;
            }
        }
        

        private void HideAsSubMenu(bool giveFocusParent = true)
        {
            if (IsRootMenu) return;
            
            foreach(var submenu in _itemToSubMenuTable.Values)
            {
                submenu.HideAsSubMenu(false);
            }
            
            _outerContainer.RemoveFromHierarchy();

            if (giveFocusParent)
            {
                var focusTarget = _parentMenu?.contentContainer;
                focusTarget?.schedule.Execute(() => focusTarget.Focus());
            }
            
            _parentMenu = null;
        }
    }
}
        
