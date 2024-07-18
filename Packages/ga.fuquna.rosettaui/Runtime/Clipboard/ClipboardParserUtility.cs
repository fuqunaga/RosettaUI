using UnityEngine;

namespace RosettaUI
{
    public static class ClipboardParserUtility
    {
        public static Rect RectIntToRect(RectInt rectInt) => new(rectInt.x, rectInt.y, rectInt.width, rectInt.height);
        public static RectInt RectToRectInt(Rect rect) => new((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        public static Bounds BoundsIntToBounds(BoundsInt boundsInt) => new(boundsInt.center, boundsInt.size * 2);
        public static BoundsInt BoundsToBoundsInt(Bounds bounds) => new(Vector3Int.FloorToInt(bounds.center), Vector3Int.FloorToInt(bounds.size * 0.5f));

    }
}