using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        #region string interface
        
        public static DropdownElement Dropdown(Expression<Func<string>> targetExpression, IEnumerable<string> options) 
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, options);

        public static DropdownElement Dropdown(LabelElement label, Expression<Func<string>> targetExpression, IEnumerable<string> options) 
            => Dropdown(label, UIInternalUtility.CreateBinder(targetExpression), options);
        
        
        public static DropdownElement Dropdown(Expression<Func<string>> targetExpression, Action<string> writeValue, IEnumerable<string> options) 
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression) , targetExpression.Compile(), writeValue, options);

        public static DropdownElement Dropdown(LabelElement label, Func<string> readValue, Action<string> writeValue, IEnumerable<string> options) 
            => Dropdown(label, Binder.Create(readValue, writeValue), options);

        
        public static DropdownElement DropdownReadOnly(Expression<Func<string>> targetExpression, IEnumerable<string> options)
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression), UIInternalUtility.CreateReadOnlyBinder(targetExpression),
                options);

        public static DropdownElement DropdownReadOnly(LabelElement label, Func<string> readValue, IEnumerable<string> options) 
            => Dropdown(label, Binder.Create(readValue, null), options);
        
        public static DropdownElement Dropdown(LabelElement label, IBinder<string> binder, IEnumerable<string> options)
        {
            var optionList = options.ToList();

            var intBinder = Binder.Create(
                () => optionList.IndexOf(binder.Get()),
                binder.IsReadOnly 
                    ? null
                    : i => binder.Set(optionList[i])
            );

            return Dropdown(label, intBinder, optionList);
        }
        
        #endregion
        
        
        #region int interface
        
        public static DropdownElement Dropdown(Expression<Func<int>> targetExpression, IEnumerable<string> options) 
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, options);

        public static DropdownElement Dropdown(LabelElement label, Expression<Func<int>> targetExpression, IEnumerable<string> options) 
            => Dropdown(label, UIInternalUtility.CreateBinder(targetExpression), options);
        
        
        public static DropdownElement Dropdown(Expression<Func<int>> targetExpression, Action<int> writeValue, IEnumerable<string> options) 
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression) , targetExpression.Compile(), writeValue, options);

        public static DropdownElement Dropdown(LabelElement label, Func<int> readValue, Action<int> writeValue, IEnumerable<string> options) 
            => Dropdown(label, Binder.Create(readValue, writeValue), options);

        
        public static DropdownElement DropdownReadOnly(Expression<Func<int>> targetExpression, IEnumerable<string> options)
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression), UIInternalUtility.CreateReadOnlyBinder(targetExpression),
                options);

        public static DropdownElement DropdownReadOnly(LabelElement label, Func<int> readValue, IEnumerable<string> options) 
            => Dropdown(label, Binder.Create(readValue, null), options);
        
        public static DropdownElement Dropdown(LabelElement label, IBinder<int> binder, IEnumerable<string> options)
        {
            var element = new DropdownElement(label, binder, options);
            UIInternalUtility.SetInteractableWithBinder(element, binder);
            return element;
        }
        
        #endregion
    }
}