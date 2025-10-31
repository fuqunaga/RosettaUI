using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RosettaUI
{
    public static class SerializedPropertyTypeRuntimeUtility
    {
        public static SerializedPropertyTypeRuntime TypeToSerializedPropertyType(Type type)
        {
            return type.Name switch
            {
                nameof(Int16) or nameof(UInt16) or
                    nameof(Int32) or nameof(UInt32) or
                    nameof(Byte) or nameof(SByte) or
                    nameof(Int64) or nameof(UInt64) => SerializedPropertyTypeRuntime.Integer,
                nameof(Boolean) => SerializedPropertyTypeRuntime.Boolean,
                nameof(Single) or nameof(Double) => SerializedPropertyTypeRuntime.Float,
                nameof(String) => SerializedPropertyTypeRuntime.String,
                nameof(Color) => SerializedPropertyTypeRuntime.Color,
                // ObjectReferenceは下部で判定
                nameof(LayerMask) => SerializedPropertyTypeRuntime.LayerMask,
                // Enumは下部で判定
                nameof(Vector2) => SerializedPropertyTypeRuntime.Vector2,
                nameof(Vector3) => SerializedPropertyTypeRuntime.Vector3,
                nameof(Vector4) => SerializedPropertyTypeRuntime.Vector4,
                nameof(Rect) => SerializedPropertyTypeRuntime.Rect,
                //nameof(ArraySize) => SerializedPropertyTypeRuntime.ArraySize,
                nameof(Char) => SerializedPropertyTypeRuntime.Character,
                nameof(AnimationCurve) => SerializedPropertyTypeRuntime.AnimationCurve,
                nameof(Bounds) => SerializedPropertyTypeRuntime.Bounds,
                nameof(Gradient) => SerializedPropertyTypeRuntime.Gradient,
                nameof(Quaternion) => SerializedPropertyTypeRuntime.Quaternion,
                // nameof(ExposedReference) => SerializedPropertyTypeRuntime.ExposedReference,
                // nameof(FixedBufferSize) => SerializedPropertyTypeRuntime.FixedBufferSize,
                nameof(Vector2Int) => SerializedPropertyTypeRuntime.Vector2Int,
                nameof(Vector3Int) => SerializedPropertyTypeRuntime.Vector3Int,
                nameof(RectInt) => SerializedPropertyTypeRuntime.RectInt,
                nameof(BoundsInt) => SerializedPropertyTypeRuntime.BoundsInt,
                // nameof(ManagedReference) => SerializedPropertyTypeRuntime.ManagedReference,
                nameof(Hash128) => SerializedPropertyTypeRuntime.Hash128,
#if UNITY_6000_0_OR_NEWER
                nameof(RenderingLayerMask) => SerializedPropertyTypeRuntime.RenderingLayerMask,
#endif
                
                _ when type.IsEnum => SerializedPropertyTypeRuntime.Enum,
                _ when type.IsSubclassOf(typeof(Object)) =>  SerializedPropertyTypeRuntime.ObjectReference,
                _ => SerializedPropertyTypeRuntime.Generic
            };
        }
    }
}