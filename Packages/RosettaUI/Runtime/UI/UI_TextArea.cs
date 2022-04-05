using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public partial class UI
    {
        public static TextFieldElement TextArea(Expression<Func<string>> targetExpression)
            => TextArea(ExpressionUtility.CreateLabelString(targetExpression), targetExpression);
        
        public static TextFieldElement TextArea(LabelElement label, Expression<Func<string>> targetExpression) 
            => _TextArea(Field(label, targetExpression));

        public static TextFieldElement TextArea(Expression<Func<string>> targetExpression, Action<string> writeValue) 
            => TextArea(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue);

        public static TextFieldElement TextArea(LabelElement label, Func<string> readValue, Action<string> writeValue) 
            => _TextArea(Field(label, readValue, writeValue));

        
        public static TextFieldElement TextAreaReadOnly(Expression<Func<string>> targetExpression)
            => _TextArea(FieldReadOnly(targetExpression));

        public static TextFieldElement TextAreaReadOnly(LabelElement label, Func<string> readValue) 
            => _TextArea(FieldReadOnly(label, readValue));

        

        static TextFieldElement _TextArea(Element element)
        {
            var textFieldElement = (TextFieldElement) element;
            textFieldElement.MultiLine = true;
            return textFieldElement;
        }
    }
}