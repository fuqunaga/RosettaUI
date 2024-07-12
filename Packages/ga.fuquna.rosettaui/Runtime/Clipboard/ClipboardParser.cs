using System;
using UnityEngine;

/// <summary>
/// Unity inspector compatible parser
///
/// ref: https://github.com/Unity-Technologies/UnityCsReference/blob/6000.0/Editor/Mono/Clipboard/ClipboardParser.cs
/// </summary>
public static class ClipboardParser
{
    public static bool DeserializeBool(string text, out bool value)
    {
        value = false;
        return !string.IsNullOrEmpty(text) && bool.TryParse(text, out value);
    }
    
    public static string SerializeBool(bool value) => value.ToString();
    

    public const string PrefixGradient = nameof(UnityEditor) + ".GradientWrapperJSON:";
    
    //　Gradient
    // インスペクタはCustomPrefix付きでEditorJsonUtilityでシリアライズしている
    // https://github.com/Unity-Technologies/UnityCsReference/blob/5406f17521a16bb37880960352990229987aa676/Editor/Mono/Clipboard/ClipboardParser.cs#L353
    //
    // さらにGradientWrapperでかぶせている
    public static bool DeserializeGradient(string text, out Gradient gradient)
    {
        gradient = new Gradient();
        if (string.IsNullOrEmpty(text)) return false;
        
        
        if (!text.StartsWith(PrefixGradient, StringComparison.OrdinalIgnoreCase)) return false;
        
        try
        {
            var wrapper = new GradientWrapper();
            JsonUtility.FromJsonOverwrite(text.Remove(0, PrefixGradient.Length), wrapper);
            gradient = wrapper.gradient;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
        
        return true;
    }
    
    public static string SerializeGradient(Gradient gradient)
    {
        var wrapper = new GradientWrapper { gradient = gradient };
        return $"{PrefixGradient}{JsonUtility.ToJson(wrapper)}";
    } 
    
    [Serializable]
    private class GradientWrapper
    {
        public Gradient gradient;
    }
}
