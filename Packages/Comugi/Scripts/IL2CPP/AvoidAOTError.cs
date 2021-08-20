using UnityEngine;

namespace Comugi.IL2CPP
{
    /// <summary>
    /// AOTのエラーを回避するために使用するコードを書いておく
    /// 呼び出す必要はない
    /// </summary>
    public static class AvoidAOTError
    {
        static void ComplieHint()
        {
            DefineTypes<bool>();
            DefineTypes<int>();
            DefineTypes<uint>();
            DefineTypes<long>();
            DefineTypes<ulong>();
            DefineTypes<float>();
            DefineTypes<double>();
            DefineTypes<Vector2>();
            DefineTypes<Vector3>();
            DefineTypes<Vector4>();
            DefineTypes<Vector2Int>();
            DefineTypes<Vector3Int>();
            DefineTypes<Color>();
            DefineTypes<Rect>();
            DefineTypes<RectInt>();
            DefineTypes<Bounds>();
            DefineTypes<BoundsInt>();
        }


        public static void DefineTypes<T>()
        {
            new MemberInfoToBinder<T>(null, null);
            new MethodCallToBinder<T>(null);
            new BinaryExpressionToBinder<T>(null);
            new UnaryExpressionToBinder<T>(null);

            Binder.Create<T>(null, null);
            new EnumToIdxBinder<T>(null);

            DefineTypes_PropertyOrFieldBinder<T>();
        }


        //　組み合わせ爆発するのでなんとかしたい
        static void DefineTypes_PropertyOrFieldBinder<T>()
        {
            new PropertyOrFieldBinder<T, bool>(null, null);
            new PropertyOrFieldBinder<T, int>(null, null);
            new PropertyOrFieldBinder<T, uint>(null, null);
            new PropertyOrFieldBinder<T, long>(null, null);
            new PropertyOrFieldBinder<T, ulong>(null, null);
            new PropertyOrFieldBinder<T, float>(null, null);
            new PropertyOrFieldBinder<T, double>(null, null);
            new PropertyOrFieldBinder<T, Vector2>(null, null);
            new PropertyOrFieldBinder<T, Vector3>(null, null);
            new PropertyOrFieldBinder<T, Vector4>(null, null);
            new PropertyOrFieldBinder<T, Vector2Int>(null, null);
            new PropertyOrFieldBinder<T, Vector3Int>(null, null);
            new PropertyOrFieldBinder<T, Color>(null, null);
            new PropertyOrFieldBinder<T, Rect>(null, null);
            new PropertyOrFieldBinder<T, RectInt>(null, null);
            new PropertyOrFieldBinder<T, Bounds>(null, null);
            new PropertyOrFieldBinder<T, BoundsInt>(null, null);

            new PropertyOrFieldMinMaxGetter<T, bool>(null, null);
            new PropertyOrFieldMinMaxGetter<T, int>(null, null);
            new PropertyOrFieldMinMaxGetter<T, uint>(null, null);
            new PropertyOrFieldMinMaxGetter<T, long>(null, null);
            new PropertyOrFieldMinMaxGetter<T, ulong>(null, null);
            new PropertyOrFieldMinMaxGetter<T, float>(null, null);
            new PropertyOrFieldMinMaxGetter<T, double>(null, null);
            new PropertyOrFieldMinMaxGetter<T, Vector2>(null, null);
            new PropertyOrFieldMinMaxGetter<T, Vector3>(null, null);
            new PropertyOrFieldMinMaxGetter<T, Vector4>(null, null);
            new PropertyOrFieldMinMaxGetter<T, Vector2Int>(null, null);
            new PropertyOrFieldMinMaxGetter<T, Vector3Int>(null, null);
            new PropertyOrFieldMinMaxGetter<T, Color>(null, null);
            new PropertyOrFieldMinMaxGetter<T, Rect>(null, null);
            new PropertyOrFieldMinMaxGetter<T, RectInt>(null, null);
            new PropertyOrFieldMinMaxGetter<T, Bounds>(null, null);
            new PropertyOrFieldMinMaxGetter<T, BoundsInt>(null, null);
        }
    }
}