using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public partial class UI
    {
        public static TextFieldElement TextArea(Expression<Func<string>> targetExpression, Action<string> onValueChanged = null)
            => TextArea(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, onValueChanged);
        
        public static TextFieldElement TextArea(LabelElement label, Expression<Func<string>> targetExpression,
            Action<string> onValueChanged = null)
        {
            var stringField = (TextFieldElement) Field(label, targetExpression, onValueChanged);
            stringField.MultiLine = true;
            return stringField;
        }
    }
}