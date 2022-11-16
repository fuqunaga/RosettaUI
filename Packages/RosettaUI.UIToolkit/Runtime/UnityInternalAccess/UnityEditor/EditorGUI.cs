using System.Globalization;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public sealed class EditorGUI
    {
        internal static string kFloatFieldFormatString = "g7";
        // internal static readonly string s_AllowedCharactersForFloat = "inftynaeINFTYNAE0123456789.,-*/+%^()cosqrludxvRL=pP#";
        internal static readonly string s_AllowedCharactersForFloat = "0123456789.-+";
        
        
        // ref: https://github.com/Unity-Technologies/UnityCsReference/blob/79d86f859b8454fcacc96962eb6e455cf6cd4f45/Runtime/Export/ExpressionEvaluator.cs#L508
        public static bool StringToDouble(string str, out double num)
        {
            return double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out num);
        }
        
        internal static string kIntFieldFormatString = "#######0";
        internal static readonly string s_AllowedCharactersForInt = "0123456789-+";
        
        // ref: https://github.com/Unity-Technologies/UnityCsReference/blob/79d86f859b8454fcacc96962eb6e455cf6cd4f45/Runtime/Export/ExpressionEvaluator.cs#L514
        internal static bool StringToLong(string str, out long value)
        {
            return long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out value);
        } 
    }
}