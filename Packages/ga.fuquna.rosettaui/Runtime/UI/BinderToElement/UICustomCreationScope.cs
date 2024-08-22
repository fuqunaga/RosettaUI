using System;
using System.Collections.Generic;

namespace RosettaUI
{
    public static partial class BinderToElement
    {
        /// <summary>
        /// UICustom.CreationFunc()内でUI.Field()が呼ばれたときは標準のUI.Field()にする
        /// BinderToElement.CreateFieldElement()内で有効化され
        /// BinderToElement.CreateMemberFieldElement()でキャンセルされる
        /// </summary>
        private readonly ref struct UICustomCreationScope
        {
            private static readonly Stack<Type> TypeStack = new();

            public static bool IsIn(Type type) => TypeStack.TryPeek(out var value) && value == type;

            public UICustomCreationScope(Type type) => TypeStack.Push(type);
            
            public void Dispose() => TypeStack.Pop();
        }
    }
}