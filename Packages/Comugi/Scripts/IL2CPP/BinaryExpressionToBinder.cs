using System;
using System.Linq.Expressions;
using UnityEngine.Assertions;

namespace Comugi.IL2CPP
{
    static class BinaryExpressionToBinder
    {
        public static IBinder CreateBinder(BinaryExpression bexpr)
        {
            var converterType = typeof(BinaryExpressionToBinder<>).MakeGenericType(bexpr.Type);
            var converter = Activator.CreateInstance(converterType, bexpr) as IBinderCreatable;

            return converter.CreateBinder();
        }
    }

    class BinaryExpressionToBinder<T> : IBinderCreatable
    {
        readonly IBinder leftBinder;
        readonly IBinder rightBinder;
        readonly Func<T> getFunc;


        public BinaryExpressionToBinder(BinaryExpression bexpr)
        {
            leftBinder = ExpressionUtility_IL2CPP.CreateBinder(bexpr.Left);
            rightBinder = ExpressionUtility_IL2CPP.CreateBinder(bexpr.Right);


            if (bexpr.Method == null)
            {
                switch (bexpr.NodeType)
                {
                    case ExpressionType.Add: getFunc = CreateFunc_Add(); break;
                    case ExpressionType.Subtract: getFunc = CreateFunc_Subtract(); break;
                    case ExpressionType.Multiply: getFunc = CreateFunc_Multiply(); break;
                    case ExpressionType.Divide: getFunc = CreateFunc_Divide(); break;
                }

                Assert.IsNotNull(getFunc);
            }
            else
            {
                getFunc = () => (T)bexpr.Method.Invoke(null, new[] { GetLeft(), GetRight() });
            }
        }

        object GetLeft() => leftBinder.GetObject();
        object GetRight() => rightBinder.GetObject();

        public IBinder CreateBinder() => Binder.Create(getFunc, null);


        Func<T> CreateFunc_Add()
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32: return () => (T)(object)(Convert.ToInt32(GetLeft()) + Convert.ToInt32(GetRight()));
                case TypeCode.UInt32: return () => (T)(object)(Convert.ToUInt32(GetLeft()) + Convert.ToUInt32(GetRight()));
                case TypeCode.Int64: return () => (T)(object)(Convert.ToInt64(GetLeft()) + Convert.ToInt64(GetRight()));
                case TypeCode.UInt64: return () => (T)(object)(Convert.ToUInt64(GetLeft()) + Convert.ToUInt64(GetRight()));
                case TypeCode.Single: return () => (T)(object)(Convert.ToSingle(GetLeft()) + Convert.ToSingle(GetRight()));
                case TypeCode.Double: return () => (T)(object)(Convert.ToDouble(GetLeft()) + Convert.ToDouble(GetRight()));

                case TypeCode.String: return () => (T)(object)(Convert.ToString(GetLeft()) + Convert.ToString(GetRight()));
            }

            return null;
        }

        Func<T> CreateFunc_Subtract()
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32: return () => (T)(object)(Convert.ToInt32(GetLeft()) - Convert.ToInt32(GetRight()));
                case TypeCode.UInt32: return () => (T)(object)(Convert.ToUInt32(GetLeft()) - Convert.ToUInt32(GetRight()));
                case TypeCode.Int64: return () => (T)(object)(Convert.ToInt64(GetLeft()) - Convert.ToInt64(GetRight()));
                case TypeCode.UInt64: return () => (T)(object)(Convert.ToUInt64(GetLeft()) - Convert.ToUInt64(GetRight()));
                case TypeCode.Single: return () => (T)(object)(Convert.ToSingle(GetLeft()) - Convert.ToSingle(GetRight()));
                case TypeCode.Double: return () => (T)(object)(Convert.ToDouble(GetLeft()) - Convert.ToDouble(GetRight()));
            }

            return null;
        }

        Func<T> CreateFunc_Multiply()
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32: return () => (T)(object)(Convert.ToInt32(GetLeft()) * Convert.ToInt32(GetRight()));
                case TypeCode.UInt32: return () => (T)(object)(Convert.ToUInt32(GetLeft()) * Convert.ToUInt32(GetRight()));
                case TypeCode.Int64: return () => (T)(object)(Convert.ToInt64(GetLeft()) * Convert.ToInt64(GetRight()));
                case TypeCode.UInt64: return () => (T)(object)(Convert.ToUInt64(GetLeft()) * Convert.ToUInt64(GetRight()));
                case TypeCode.Single: return () => (T)(object)(Convert.ToSingle(GetLeft()) * Convert.ToSingle(GetRight()));
                case TypeCode.Double: return () => (T)(object)(Convert.ToDouble(GetLeft()) * Convert.ToDouble(GetRight()));
            }

            return null;
        }

        Func<T> CreateFunc_Divide()
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Int32: return () => (T)(object)(Convert.ToInt32(GetLeft()) / Convert.ToInt32(GetRight()));
                case TypeCode.UInt32: return () => (T)(object)(Convert.ToUInt32(GetLeft()) / Convert.ToUInt32(GetRight()));
                case TypeCode.Int64: return () => (T)(object)(Convert.ToInt64(GetLeft()) / Convert.ToInt64(GetRight()));
                case TypeCode.UInt64: return () => (T)(object)(Convert.ToUInt64(GetLeft()) / Convert.ToUInt64(GetRight()));
                case TypeCode.Single: return () => (T)(object)(Convert.ToSingle(GetLeft()) / Convert.ToSingle(GetRight()));
                case TypeCode.Double: return () => (T)(object)(Convert.ToDouble(GetLeft()) / Convert.ToDouble(GetRight()));
            }

            return null;
        }
    }
}