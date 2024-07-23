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
                nameof(Int32) or nameof(UInt32) => SerializedPropertyTypeRuntime.Integer,
                nameof(Boolean) => SerializedPropertyTypeRuntime.Boolean,
                nameof(Single) => SerializedPropertyTypeRuntime.Float,
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
                nameof(RenderingLayerMask) => SerializedPropertyTypeRuntime.RenderingLayerMask,
                
                _ when type.IsEnum => SerializedPropertyTypeRuntime.Enum,
                _ when type.IsSubclassOf(typeof(Object)) =>  SerializedPropertyTypeRuntime.ObjectReference,
                _ => SerializedPropertyTypeRuntime.Generic
            };
        }
    }
}