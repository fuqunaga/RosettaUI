using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

namespace RosettaUI
{
    public static partial class UI
    {
        public static DropdownElement Dropdown(Expression<Func<int>> targetExpression, IEnumerable<string> options) 
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, options);

        public static DropdownElement Dropdown(LabelElement label, Expression<Func<int>> targetExpression, IEnumerable<string> options) 
            => Dropdown(label, CreateBinder(targetExpression), options);
        
        
        public static DropdownElement Dropdown(Expression<Func<int>> targetExpression, Action<int> writeValue, IEnumerable<string> options) 
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression) , targetExpression.Compile(), writeValue, options);

        public static DropdownElement Dropdown(LabelElement label, Func<int> readValue, Action<int> writeValue, IEnumerable<string> options) 
            => Dropdown(label, Binder.Create(readValue, writeValue), options);

        
        public static DropdownElement DropdownReadOnly(Expression<Func<int>> targetExpression, IEnumerable<string> options)
            => Dropdown(ExpressionUtility.CreateLabelString(targetExpression), CreateReadOnlyBinder(targetExpression),
                options);

        public static DropdownElement DropdownReadOnly(LabelElement label, Func<int> readValue, IEnumerable<string> options) 
            => Dropdown(label, Binder.Create(readValue, null), options);

        
        public static DropdownElement Dropdown(LabelElement label, IBinder<int> binder, IEnumerable<string> options)
        {
            var element = new DropdownElement(label, binder, options);
            SetInteractableWithBinder(element, binder);
            return element;
        }
   }
}