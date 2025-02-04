using UnityEngine;

namespace RosettaUI
{
    public static class InputUtility
    {
        // 呼び出し元がGameViewとEditorWindowか判別しそれぞれのUIに渡す座標系でマウスの位置を取得
        public static Vector2 GetMousePositionUICoordinate()
        {
            return Event.current?.mousePosition
                   ?? new Vector2(
                       Input.mousePosition.x,
                       Screen.height - Input.mousePosition.y
                   );
        }
        
        public static bool GetKey(KeyCode key)
        {
#if UNITY_INPUT_SYSTEM_ENABLED
        return Keyboard.current != null && Keyboard.current[(Key)key].isPressed;
#else
            return Input.GetKey(key);
#endif
        }

        public static bool GetKeyDown(KeyCode key)
        {
#if UNITY_INPUT_SYSTEM_ENABLED
        return Keyboard.current != null && Keyboard.current[(Key)key].wasPressedThisFrame;
#else
            return Input.GetKeyDown(key);
#endif
        }

        public static bool GetKeyUp(KeyCode key)
        {
#if UNITY_INPUT_SYSTEM_ENABLED
        return Keyboard.current != null && Keyboard.current[(Key)key].wasReleasedThisFrame;
#else
            return Input.GetKeyUp(key);
#endif
        }
    }
}