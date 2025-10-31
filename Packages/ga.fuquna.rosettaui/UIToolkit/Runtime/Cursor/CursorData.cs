using UnityEngine;

namespace RosettaUI.UIToolkit
{
    [CreateAssetMenu(menuName = "RosettaUI/CursorData", order = 1000)]
    public class CursorData : ScriptableObject
    {
        [System.Serializable]
        public class Data
        {
            public Texture2D tex;
            public Vector2Int hotspot;
        }

        public Data resizeHorizontal;
        public Data resizeVertical;
        public Data resizeUpLeft;
        public Data resizeUpRight;
    }
}
