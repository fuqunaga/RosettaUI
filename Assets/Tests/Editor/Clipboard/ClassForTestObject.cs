using System;
using System.Collections.Generic;
using UnityEngine;

namespace RosettaUI.Test
{
    [Serializable]
    public class ClassForTest
    {
        public bool boolValue;
        public byte byteValue;
        public sbyte sbyteValue;
        public int intValue;
        public uint uintValue;
        public long longValue;
        public ulong ulongValue;
        public short shortValue;
        public ushort ushortValue;
        public float floatValue;
        public double doubleValue;
        public string stringValue;
        public Color colorValue;
        public LayerMask layerMask;
        public EnumForTest enumValue;
        public Vector2 vector2Value;
        public Vector3 vector3Value;
        public Vector4 vector4Value;
        public Rect rectValue;
        public char charValue;
        public Bounds boundsValue;
        public Gradient gradient = new();
        public Quaternion quaternion;
        public Vector2Int vector2IntValue;
        public Vector3Int vector3IntValue;
        public RectInt rectIntValue;
        public BoundsInt boundsIntValue;
        // public Hash128 hash128Value;
#if UNITY_6000_OR_NEWER
        public RenderingLayerMask renderingLayerMask;
#endif

        public RectOffset rectOffset = new();
        
        public int[] intArray = { 1, 2, 3 };
        public List<float> floatList = new(){ 1, 2, 3 };
        public RectOffset[] classArray = { new() };
        public List<RectOffset> classList = new(){ new RectOffset() };
    }
    
    
    /// <summary>
    /// EditorのClipboardは任意クラスのパースでSerializedPropertyのみ対応している
    /// SerializedPropertyを用意するためにScriptableObjectを使用している
    /// </summary>
    [CreateAssetMenu(fileName = "ClassForTest", menuName = "Scriptable Objects/ClassForTest")]
    public class ClassForTestObject : ScriptableObject
    {
        public ClassForTest classValue;
    }
}