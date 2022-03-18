namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public sealed class EditorGUI
    {
        internal static string kFloatFieldFormatString = "g7";
        // internal static readonly string s_AllowedCharactersForFloat = "inftynaeINFTYNAE0123456789.,-*/+%^()cosqrludxvRL=pP#";
        internal static readonly string s_AllowedCharactersForFloat = "0123456789.-+";
        
        public static void StringToDouble(string str, out double num)
        {
            double.TryParse(str, out num);
        }
        
        internal static string kIntFieldFormatString = "#######0";
        internal static readonly string s_AllowedCharactersForInt = "0123456789-+";
        
        internal static bool StringToLong(string str, out long value) => long.TryParse(str, out value);
    }
}