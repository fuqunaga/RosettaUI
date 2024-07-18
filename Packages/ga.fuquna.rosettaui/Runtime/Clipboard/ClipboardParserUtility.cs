using UnityEngine;

namespace RosettaUI
{
    public static class ClipboardParserUtility
    {
        public static Vector2Int Vector2ToVector2Int(Vector2 vector2) => new((int)vector2.x, (int)vector2.y);
        public static Vector3Int Vector3ToVector3Int(Vector3 vector3) => new((int)vector3.x, (int)vector3.y, (int)vector3.z);
        public static Rect RectIntToRect(RectInt rectInt) => new(rectInt.x, rectInt.y, rectInt.width, rectInt.height);
        public static RectInt RectToRectInt(Rect rect) => new((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        public static Bounds BoundsIntToBounds(BoundsInt boundsInt) => new(boundsInt.center, boundsInt.size * 2);
        public static BoundsInt BoundsToBoundsInt(Bounds bounds) => new(Vector3Int.FloorToInt(bounds.center), Vector3Int.FloorToInt(bounds.size * 0.5f));

    }
}