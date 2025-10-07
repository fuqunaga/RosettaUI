using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using RosettaUI.Utilities;

namespace RosettaUI.UndoSystem
{
    public interface IRestoreToElementRecord : IDisposable
    {
        bool TryRestore(Element element);
    }


    /// <summary>
    /// <see cref="FieldBaseElementValueRecord{TValue}"/> を生成するためのヘルパークラス
    /// </summary>    
    public static class FieldBaseElementValueRecord
    {
        private static readonly Dictionary<Type, Func<Element, IRestoreToElementRecord>> CreateFuncTable = new();
        
        public static bool TryCreate(Element element, out IRestoreToElementRecord record)
        {
            var elementType = element.GetType();
            if (!CreateFuncTable.TryGetValue(elementType, out var createFunc))
            {
                var valueType = GetFieldBaseElementValueType(elementType);
                if (valueType != null)
                {
                    var recordType = typeof(FieldBaseElementValueRecord<>).MakeGenericType(valueType);
                    var methodInfo = recordType.GetMethod(nameof(FieldBaseElementValueRecord<int>.Create), BindingFlags.Public | BindingFlags.Static);
                    Assert.IsNotNull(methodInfo, $"Method not found: {recordType.FullName}.{nameof(FieldBaseElementValueRecord<int>.Create)}");
                    
                    var func = (Func<Element, IRestoreToElementRecord>)methodInfo.CreateDelegate(typeof(Func<Element, IRestoreToElementRecord>));
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
    public class FieldBaseElementValueRecord<TValue> : ObjectPoolItem<FieldBaseElementValueRecord<TValue>>, IRestoreToElementRecord
    {
        public static IRestoreToElementRecord Create(Element element)
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