using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// Dropdown()の第3引数を無視するGenericDropdownMenu
    /// BasePopupFieldがDropdownMenuでanchor == trueで呼び出しているが、強制的falseにする
    ///
    ///     IGenericMenuがinternalなので次のようなクラスを動的に生成する
    ///
    /// public class GenericDropdownMenuIgnoreAnchored : GenericDropdownMenu, IGenericMenu
    /// {
    ///     void IGenericMenu.DropDown(Rect position, VisualElement targetElement, bool anchored)
    ///         => base.DropDown(position, targetElement, false);
    /// }
    /// </summary>
    public static class GenericDropdownMenuIgnoreAnchoredBuilder
    {
        private static readonly Type GenericDropdownMenuIgnoreAnchoredType;
        
        public static GenericDropdownMenu CreateGenericDropdownMenuIgnoreAnchored()
        {
            return (GenericDropdownMenu)Activator.CreateInstance(GenericDropdownMenuIgnoreAnchoredType);
        }
        
        static GenericDropdownMenuIgnoreAnchoredBuilder()
        {
            const string dropdownMethodName = "DropDown";
            
            var iGenericMenuType = typeof(GenericDropdownMenu).Assembly.GetType("UnityEngine.UIElements.IGenericMenu");
            var moduleBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("RosettaUI.UIToolkit.DynamicAssembly"), AssemblyBuilderAccess.Run).DefineDynamicModule("RosettaUI.UIToolkit.DynamicModule");
            var typeBuilder = moduleBuilder.DefineType("RosettaUI.UIToolkit.GenericDropdownMenuIgnoreAnchored", TypeAttributes.Public | TypeAttributes.Class, typeof(GenericDropdownMenu), new[]{iGenericMenuType});
            
            // インターフェイスの実装
            typeBuilder.AddInterfaceImplementation(iGenericMenuType);
            
            var dropDownMethodArgumentTypes = new[] { typeof(Rect), typeof(VisualElement), typeof(bool) };
            var methodBuilder = typeBuilder.DefineMethod(
                dropdownMethodName,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                null,
                dropDownMethodArgumentTypes
            );
            
            // IL生成 (メソッドの実装)
            var ilGenerator = methodBuilder.GetILGenerator();
        
            // 静的メソッドの呼び出しを行うILコードを生成
            ilGenerator.Emit(OpCodes.Ldarg_0); // this
            ilGenerator.Emit(OpCodes.Ldarg_1); // Rect position
            ilGenerator.Emit(OpCodes.Ldarg_2); // VisualElement targetElement
            ilGenerator.Emit(OpCodes.Ldc_I4_0); // bool anchored (false)
            ilGenerator.Emit(OpCodes.Call, typeof(GenericDropdownMenu).GetMethod(dropdownMethodName, dropDownMethodArgumentTypes));
            ilGenerator.Emit(OpCodes.Ret);
            
            // メソッドをインターフェイスのメソッドにマップする
            typeBuilder.DefineMethodOverride(methodBuilder, iGenericMenuType.GetMethod(dropdownMethodName));
            
            GenericDropdownMenuIgnoreAnchoredType = typeBuilder.CreateType();
        }
    }
}