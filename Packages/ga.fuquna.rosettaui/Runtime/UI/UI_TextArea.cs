using System;
using System.Linq;
using System.Linq.Expressions;

namespace RosettaUI
{
    public partial class UI
    {
        public static Element TextArea(Expression<Func<string>> targetExpression)
            => TextArea(ExpressionUtility.CreateLabelString(targetExpression), targetExpression);
        
        public static Element TextArea(LabelElement label, Expression<Func<string>> targetExpression) 
            => _TextArea(Field(label, targetExpression));

        public static Element TextArea(Expression<Func<string>> targetExpression, Action<string> writeValue) 
            => TextArea(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue);

        public static Element TextArea(LabelElement label, Func<string> readValue, Action<string> writeValue) 
            => _TextArea(Field(label, readValue, writeValue));

        
        public static Element TextAreaReadOnly(Expression<Func<string>> targetExpression)
            => _TextArea(FieldReadOnly(targetExpression));

        public static Element TextAreaReadOnly(LabelElement label, Func<string> readValue) 
            => _TextArea(FieldReadOnly(label, readValue));


        private static Element _TextArea(Element element)
        {
            var textFieldElement = element.Query<TextFieldElement>().First();
            textFieldElement.IsMultiLine = true;
            return element;
        }
    }
}