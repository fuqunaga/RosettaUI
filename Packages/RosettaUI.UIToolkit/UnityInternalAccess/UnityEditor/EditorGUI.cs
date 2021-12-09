namespace RosettaUI.UIToolkit.PackageInternal
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
    }
}