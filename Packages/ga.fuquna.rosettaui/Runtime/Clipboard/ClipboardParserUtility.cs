using UnityEngine;

namespace RosettaUI
{
    public static class ClipboardParserUtility
    {
        public static Vector2Int ToInt(Vector2 vector2) => new((int)vector2.x, (int)vector2.y);
        public static Vector3Int ToInt(Vector3 vector3) => new((int)vector3.x, (int)vector3.y, (int)vector3.z);
        public static Rect FromInt(RectInt rectInt) => new(rectInt.x, rectInt.y, rectInt.width, rectInt.height);
        public static RectInt ToInt(Rect rect) => new((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        public static Bounds FromInt(BoundsInt boundsInt) => new(boundsInt.center, boundsInt.size);
        public static BoundsInt ToInt(Bounds bounds) => new(ToInt(bounds.center), ToInt(bounds.size));

        // おそらく BoundsInt ↔ Bounds 間でコピペしたときUI上の数値が一致しているのが直観的で良い
        // という理由でインスペクタはこのような値変換をしている
        public static Bounds FromIntKeepValueLook(BoundsInt boundsInt) => new(boundsInt.center, boundsInt.size * 2);
        public static BoundsInt ToIntKeepValueLook(Bounds bounds) => new(ToInt(bounds.center), ToInt(bounds.size * 0.5f));
    }
}