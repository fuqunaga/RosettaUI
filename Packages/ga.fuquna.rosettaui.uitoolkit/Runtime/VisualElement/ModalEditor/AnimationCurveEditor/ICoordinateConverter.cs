using UnityEngine;

namespace RosettaUI.UIToolkit.AnimationCurveEditor
{
    public interface ICoordinateConverter
    {
        Vector2 GetScreenUvFromScreenPos(Vector2 screenPos);
        Vector2 GetScreenPosFromScreenUv(Vector2 screenUv);
        Vector2 GetCurvePosFromScreenUv(Vector2 screenUv);
        Vector2 GetScreenUvFromCurvePos(Vector2 curvePos);
        float GetCurveTangentFromScreenTangent(float tangent);
        float GetScreenTangentFromCurveTangent(float tangent);
        Vector2 GetScreenPosFromUIWorldPos(Vector2 uiWorldPos);
        Vector2 GetUIWorldPosFromScreenPos(Vector2 screenPos);
    }

    public static class CoordinateConverterExtension
    {
        public static Vector2 GetCurvePosFromScreenPos(this ICoordinateConverter converter, Vector2 screenPos)
        {
            return converter.GetCurvePosFromScreenUv(converter.GetScreenUvFromScreenPos(screenPos));
        }
        
        public static Vector2 GetScreenPosFromCurvePos(this ICoordinateConverter converter, Vector2 curvePos)
        {
            return converter.GetScreenPosFromScreenUv(converter.GetScreenUvFromCurvePos(curvePos));
        }
        
        public static Vector2 GetCurvePosFromUIWorldPos(this ICoordinateConverter converter, Vector2 uiWorldPos)
        {
            return converter.GetCurvePosFromScreenPos(converter.GetScreenPosFromUIWorldPos(uiWorldPos));
        }
        
        public static Vector2 GetUIWorldPosFromCurvePos(this ICoordinateConverter converter, Vector2 curvePos)
        {
            return converter.GetUIWorldPosFromScreenPos(converter.GetScreenPosFromCurvePos(curvePos));
        }
    }
}