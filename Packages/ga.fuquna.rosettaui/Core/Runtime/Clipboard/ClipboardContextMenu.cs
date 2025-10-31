using System;
using System.Collections.Generic;
using RosettaUI.UndoSystem;
using UnityEngine.Pool;
using Undo = RosettaUI.UndoSystem.Undo;

namespace RosettaUI
{
    public static class ClipboardUtility
    {
        private static readonly ObjectPool<ElementHierarchyPath> ElementHierarchyPathPool = new(
            () => new ElementHierarchyPath(),
            actionOnRelease: hierarchyPath => hierarchyPath.Clear()
        );
        
        public static Func<IEnumerable<IMenuItem>> GenerateContextMenuItemsFunc<TValue>(Func<TValue> getter, Action<TValue> setter, Element targetElement = null)
        {
            return () =>
            {
                var valueString = Clipboard.GetRawString();
                var success = Clipboard.TryGet(out TValue value);

                return new[]
                {
                    new MenuItem("Copy", () => Clipboard.Set(getter())),
                    new MenuItem("Paste", () => SetValueAndRecordUndo(value, valueString))
                    {
                        isEnable = success
                    }
                };
            };
            
            
            void SetValueAndRecordUndo(TValue value, string valueString)
            {
                // TValueがclassの場合、UndoRecordに保存しても外部から変更されてしまう可能性があるためシリアライズして保存する
                var beforeString = ClipboardParser.Serialize(getter());

                setter(value);

                if (valueString == beforeString) return;
                
                var record = Undo.RecordValueChange("Paste from Clipboard", beforeString, valueString, str =>
                {
                    var (success, v) = ClipboardParser.Deserialize<TValue>(str);
                    if (!success) return;

                    setter(v);
                });
                record.CanMargeFunc = _ => false;
                
                if ( targetElement == null) return;
                
                // targetElementが指定されている場合、Elementの存在チェックを行うようにし、
                // レコード解放のタイミングでHierarchyPathも解放する
                var hierarchyPath = ElementHierarchyPathPool.Get();
                hierarchyPath.Initialize(targetElement);

                record.IsAvailableFunc = () => hierarchyPath.TryGetExistingElement(out _);
                record.onDispose += () => ElementHierarchyPathPool.Release(hierarchyPath);
            }
        }
    }
}
