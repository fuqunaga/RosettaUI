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
    }
}