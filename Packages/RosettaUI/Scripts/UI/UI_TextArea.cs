using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public partial class UI
    {
        public static TextFieldElement TextArea(Expression<Func<string>> targetExpression)
            => TextArea(ExpressionUtility.CreateLabelString(targetExpression), targetExpression);
        
        public static TextFieldElement TextArea(LabelElement label, Expression<Func<string>> targetExpression)
        {
            var stringField = (TextFieldElement) Field(label, targetExpression);
            stringField.MultiLine = true;
            return stringField;
        }
    }
}