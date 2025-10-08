using System;
using System.Collections.Generic;
using System.Reflection;
using RosettaUI.Utilities;
using UnityEngine.Assertions;

namespace RosettaUI.UndoSystem
{
    /// <summary>
    /// <see cref="FieldBaseElementRestoreRecord{TValue}"/> を生成するためのヘルパークラス
    /// </summary>    
    public static class FieldBaseElementRestoreRecord
    {
        private static readonly Dictionary<Type, Func<Element, IElementRestoreRecord>> CreateFuncTable = new();
        
        public static bool TryCreate(Element element, out IElementRestoreRecord record)
        {
            var elementType = element.GetType();
            if (!CreateFuncTable.TryGetValue(elementType, out var createFunc))
            {
                var valueType = GetFieldBaseElementValueType(elementType);
                if (valueType != null)
                {
                    var recordType = typeof(FieldBaseElementRestoreRecord<>).MakeGenericType(valueType);
                    var methodInfo = recordType.GetMethod(nameof(FieldBaseElementRestoreRecord<int>.Create), BindingFlags.Public | BindingFlags.Static);
                    Assert.IsNotNull(methodInfo, $"Method not found: {recordType.FullName}.{nameof(FieldBaseElementRestoreRecord<int>.Create)}");
                    
                    var func = (Func<Element, IElementRestoreRecord>)methodInfo.CreateDelegate(typeof(Func<Element, IElementRestoreRecord>));
                    createFunc = func;
                }

                CreateFuncTable[elementType] = createFunc;
            }


            if (createFunc == null)
            {
                record = null;
                return false;
            }

            record = createFunc(element);
            return true;


            static Type GetFieldBaseElementValueType(Type type)
            {
                var targetType = typeof(FieldBaseElement<>);

                while (type != null && type != typeof(object))
                {
                    var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                    if (cur == targetType){
                        return type.GetGenericArguments()[0];
                    }
                         
                    type = type.BaseType;
                }

                return null;
            }
        }
    }
        
    /// <summary>
    /// FieldBaseElementのValueを記録し復元するUndo機能向けのレコード
    /// UndoRecordListItemRemoveで利用される
    /// </summary>
    public class FieldBaseElementRestoreRecord<TValue> : ObjectPoolItem<FieldBaseElementRestoreRecord<TValue>>, IElementRestoreRecord
    {
        public static IElementRestoreRecord Create(Element element)
        {
            if (element is not FieldBaseElement<TValue> fieldBaseElement)
            {
                throw new ArgumentException($"element must be FieldBaseElement<{typeof(TValue).Name}>");
            }
            
            var record = GetPooled();
            record.Initialize(fieldBaseElement);
            return record;
        }
        
        
        private TValue _value;

        private void Initialize(FieldBaseElement<TValue>  fieldBaseElement)
        {
            _value = UndoHelper.Clone(fieldBaseElement.Value);
        }

        public bool TryRestore(Element element)
        {
            if (element is not FieldBaseElement<TValue> fieldBaseElement) return false;
            
            Restore(fieldBaseElement);
            return true;
        }

        private void Restore(FieldBaseElement<TValue> fieldBaseElement)
        {
            fieldBaseElement.GetViewBridge().SetValueFromView(UndoHelper.Clone(_value));
        }
    }
}