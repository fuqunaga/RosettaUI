using UnityEngine;

namespace RosettaUI
{
    public static class Clipboard
    {
        public static bool TryGet<T>(out T value)
        {
            var text = GUIUtility.systemCopyBuffer;
            
            bool success;
            (success, value) = ClipboardParser.Deserialize<T>(text);

            return success;
        }

        public static void Set<T>(T value)
        {
            GUIUtility.systemCopyBuffer = ClipboardParser.Serialize(value);
        }
    }
}