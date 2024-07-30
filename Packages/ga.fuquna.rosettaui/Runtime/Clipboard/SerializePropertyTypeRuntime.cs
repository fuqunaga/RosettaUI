namespace RosettaUI
{
    /// <summary>
    ///   <para>Represents the type of a SerializedProperty.</para>
    /// </summary>
    public enum SerializedPropertyTypeRuntime
    {
        /// <summary>
        ///   <para>Represents an array, list, struct or class.</para>
        /// </summary>
        Generic = -1, // 0xFFFFFFFF

        /// <summary>
        ///   <para>Represents an integer property, for example int, byte, short, uint and long. </para>
        /// </summary>
        Integer = 0,

        /// <summary>
        ///   <para>Represents a boolean property.</para>
        /// </summary>
        Boolean = 1,

        /// <summary>
        ///   <para>Represents a single or double precision floating point property.</para>
        /// </summary>
        Float = 2,

        /// <summary>
        ///   <para>Represents a string property.</para>
        /// </summary>
        String = 3,

        /// <summary>
        ///   <para>Represents a color property.</para>
        /// </summary>
        Color = 4,

        /// <summary>
        ///   <para>Provides a reference to an object that derives from UnityEngine.Object.</para>
        /// </summary>
        ObjectReference = 5,

        /// <summary>
        ///   <para>Represents a LayerMask property.</para>
        /// </summary>
        LayerMask = 6,

        /// <summary>
        ///   <para>Represents an enumeration property.</para>
        /// </summary>
        Enum = 7,

        /// <summary>
        ///   <para>Represents a 2D vector property.</para>
        /// </summary>
        Vector2 = 8,

        /// <summary>
        ///   <para>Represents a 3D vector property.</para>
        /// </summary>
        Vector3 = 9,

        /// <summary>
        ///   <para>Represents a 4D vector property.</para>
        /// </summary>
        Vector4 = 10, // 0x0000000A

        /// <summary>
        ///   <para>Represents a rectangle property.</para>
        /// </summary>
        Rect = 11, // 0x0000000B

        /// <summary>
        ///   <para>Represents an array size property.</para>
        /// </summary>
        ArraySize = 12, // 0x0000000C

        /// <summary>
        ///   <para>Represents a character property.</para>
        /// </summary>
        Character = 13, // 0x0000000D

        /// <summary>
        ///   <para>Represents an AnimationCurve property.</para>
        /// </summary>
        AnimationCurve = 14, // 0x0000000E

        /// <summary>
        ///   <para>Represents a bounds property.</para>
        /// </summary>
        Bounds = 15, // 0x0000000F

        /// <summary>
        ///   <para>Represents a gradient property.</para>
        /// </summary>
        Gradient = 16, // 0x00000010

        /// <summary>
        ///   <para>Represents a quaternion property.</para>
        /// </summary>
        Quaternion = 17, // 0x00000011

        /// <summary>
        ///   <para>Provides a reference to another Object in the Scene.</para>
        /// </summary>
        ExposedReference = 18, // 0x00000012

        /// <summary>
        ///   <para>Represents a fixed buffer size property.</para>
        /// </summary>
        FixedBufferSize = 19, // 0x00000013

        /// <summary>
        ///   <para>Represents a 2D integer vector property.</para>
        /// </summary>
        Vector2Int = 20, // 0x00000014

        /// <summary>
        ///   <para>Represents a 3D integer vector property.</para>
        /// </summary>
        Vector3Int = 21, // 0x00000015

        /// <summary>
        ///   <para>Represents a rectangle with Integer values property.</para>
        /// </summary>
        RectInt = 22, // 0x00000016

        /// <summary>
        ///   <para>Represents a bounds with Integer values property.</para>
        /// </summary>
        BoundsInt = 23, // 0x00000017

        /// <summary>
        ///   <para>Represents a property that references an object that does not derive from UnityEngine.Object.</para>
        /// </summary>
        ManagedReference = 24, // 0x00000018

        /// <summary>
        ///   <para>Represents a Hash128 property.</para>
        /// </summary>
        Hash128 = 25, // 0x00000019

        /// <summary>
        ///   <para>Represents a RenderingLayerMask property.</para>
        /// </summary>
        RenderingLayerMask = 26, // 0x0000001A
    }
}