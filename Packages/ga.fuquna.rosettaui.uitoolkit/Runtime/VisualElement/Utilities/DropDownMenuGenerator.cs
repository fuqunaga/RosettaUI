using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public static class DropDownMenuGenerator
    {
        public static object Generate(IEnumerable<MenuItem> menuItems, Rect position, VisualElement targetElement = null, bool anchored = false)
        {
            if (menuItems == null) return null;
            
#if UNITY_2023_1_OR_NEWER
            var menu = new NestedDropdownMenu();
            
            // refs: DropdownUtility, EditorDelegateRegistration, GenericOSMenu
            // var menu = new GenericDropdownMenu();
#else
            // refs: BasePopupField
            // https://github.com/Unity-Technologies/UnityCsReference/blob/c84064be69f20dcf21ebe4a7bbc176d48e2f289c/ModuleOverrides/com.unity.ui/Core/Controls/BasePopupField.cs#L206
            var isPlayer = targetElement?.panel.contextType == ContextType.Player;
            
            GenericDropdownMenuWrapper menu;
            if (isPlayer)
            {
                var genericMenu = new GenericDropdownMenu();
                menu = new GenericDropdownMenuWrapper(genericMenu);
            }
            // GenericDropdownMenu だとエディターでエラーになる場合があるのでDropdownUtility経由でOSのメニューを使用する
            // RosettaUIEditorWindowExample > MiscExample > UI.Popup() でエラーになる
            // at Unity2022.3
            else
            {
                // DropdownUtility.CreateDropdown() は internal なのでリフレクションで取得
#if false
                menu = DropdownUtility.CreateDropdown();
#else
                _createDropdownFunc ??= (Func<object>)typeof(VisualElement).Assembly
                    .GetType("UnityEngine.UIElements.DropdownUtility")?
                    .GetMethod("CreateDropdown", BindingFlags.NonPublic | BindingFlags.Static)?
                    .CreateDelegate(typeof(Func<object>));

                if (_createDropdownFunc == null)
                {
                    return null;
                }

                menu = new GenericDropdownMenuWrapper(_createDropdownFunc());
#endif
            }
        
#endif

            foreach (var item in menuItems)
            {
                if ( item == MenuItem.Separator)
                {
                    menu.AddSeparator("");
                    continue;
                }
                
                if (item.isEnable)
                {
                    menu.AddItem(item.name, item.isChecked, item.action);
                }
                else
                {
                    menu.AddDisabledItem(item.name, item.isChecked);
                }
            }

            menu.DropDown(position, targetElement, anchored);
            return menu;
        }
        
#if !UNITY_2023_1_OR_NEWER
        private static Func<object> _createDropdownFunc;
        
        /// <summary>
        /// UnityEngine.UIElements.IGenericMenu が internal なので代替インターフェース
        /// </summary>
        private class GenericDropdownMenuWrapper
        {
            private class MethodInfoSet
            {
                private static readonly Type[] AddItemTypes = {typeof(string), typeof(bool), typeof(Action)};
                private static readonly Type[] AddDisabledItemTypes = {typeof(string), typeof(bool)};
                private static readonly Type[] AddSeparatorTypes = {typeof(string)};
                private static readonly Type[] DropDownTypes = {typeof(Rect), typeof(VisualElement), typeof(bool)};
                
                public readonly MethodInfo addItem;
                public readonly MethodInfo addDisabledItem;
                public readonly MethodInfo addSeparator;
                public readonly MethodInfo dropDown;
                
                public MethodInfoSet(Type type)
                {
                    addItem = type.GetMethod("AddItem", AddItemTypes);
                    addDisabledItem = type.GetMethod("AddDisabledItem", AddDisabledItemTypes);
                    addSeparator = type.GetMethod("AddSeparator", AddSeparatorTypes);
                    dropDown = type.GetMethod("DropDown", DropDownTypes);
                }
            }
            
            
            private static readonly Dictionary<Type, MethodInfoSet> TypeToMethodInfoSet = new();
            
            private static MethodInfoSet GetMethodInfoSet(Type type)
            {
                if (!TypeToMethodInfoSet.TryGetValue(type, out var methodInfoSet))
                {
                    methodInfoSet = new MethodInfoSet(type);
                    TypeToMethodInfoSet[type] = methodInfoSet;
                }

                return methodInfoSet;
            }
            
            
            private readonly Action<string, bool, Action> _addItemAction;
            private readonly Action<string, bool> _addDisabledItemAction;
            private readonly Action<string> _addSeparatorAction;
            private readonly Action<Rect, VisualElement, bool> _dropDownAction;
            
            public GenericDropdownMenuWrapper(object baseObject)
            {
                var methodInfoSet = GetMethodInfoSet(baseObject.GetType());
                _addItemAction = (Action<string, bool, Action>)Delegate.CreateDelegate(typeof(Action<string, bool, Action>), baseObject, methodInfoSet.addItem);
                _addDisabledItemAction = (Action<string, bool>)Delegate.CreateDelegate(typeof(Action<string, bool>), baseObject, methodInfoSet.addDisabledItem);
                _addSeparatorAction = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), baseObject, methodInfoSet.addSeparator);
                _dropDownAction = (Action<Rect, VisualElement, bool>)Delegate.CreateDelegate(typeof(Action<Rect, VisualElement, bool>), baseObject, methodInfoSet.dropDown);
            }

            public void AddItem(string name, bool isChecked, Action action) => _addItemAction(name, isChecked, action);
            public void AddDisabledItem(string name, bool isChecked) => _addDisabledItemAction(name, isChecked);
            public void AddSeparator(string path) => _addSeparatorAction(path);
            public void DropDown(Rect position, VisualElement targetElement, bool anchored) => _dropDownAction(position, targetElement, anchored);
        }
#endif
    }
}