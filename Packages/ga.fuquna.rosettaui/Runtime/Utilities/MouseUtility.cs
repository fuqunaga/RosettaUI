using UnityEngine;

namespace RosettaUI
{
    public static class MouseUtility
    {
        // 呼び出し元がGameViewとEditorWindowか判別しそれぞれのUIに渡す座標系でマウスの位置を取得
        public static Vector2 GetPositionUICoordinate()
        {
            return Event.current?.mousePosition
                   ?? new Vector2(
                       Input.mousePosition.x,
                       Screen.height - Input.mousePosition.y
                   );
        }
    }
}