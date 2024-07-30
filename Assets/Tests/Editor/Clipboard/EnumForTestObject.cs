using UnityEngine;

// ReSharper disable InconsistentNaming
namespace RosettaUI.Test
{
    public enum EnumForTest
    {
        one,
        _two,
        three_,
        fourthItem,
        FifthItem,
        Sixth_Item,
        SEVEN,
    }

    /// <summary>
    /// EditorのClipboardParserはEnumのパースでSerializedPropertyのみ対応している
    /// SerializedPropertyを用意するためにScriptableObjectを使用している
    /// </summary>
    [CreateAssetMenu(fileName = "EnumForTest", menuName = "Scriptable Objects/EnumForTest")]
    public class EnumForTestObject : ScriptableObject
    {
        public EnumForTest enumValue;
    }
}