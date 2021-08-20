using System;
using System.Linq.Expressions;
using UnityEngine;

namespace Comugi.IL2CPP
{
    public static class ExpressionUtility_IL2CPP
    {
        public static BinderBase<T> CreateBinder<T>(Expression<Func<T>> lambda)
        {
            var binder = _CreateBinder<T>(lambda);
            if (binder == null)
            {
                Debug.LogError($"Unsupported lambda at IL2CPP.\n{lambda}");
            }

            return binder;
        }


        static BinderBase<T> _CreateBinder<T>(Expression<Func<T>> lambda)
        {
            return CreateBinder(lambda.Body) as BinderBase<T>;
        }

        internal static IBinder CreateBinder(Expression expr)
        {
            switch (expr)
            {
                case MemberExpression mexpr:
                    {
                        var parentExpr = mexpr.Expression;
                        var parentBinder = CreateBinder(parentExpr);
                        
                        return MemberInfoToBinder.CreateBinder(parentBinder, mexpr.Member, mexpr.Type);
                    }

                case ConstantExpression cexpr:
                    return ConstBinder.Create(cexpr.Value, cexpr.Type);

                case MethodCallExpression mexpr:
                    return MethodCallToBinder.CreateBinder(mexpr);

                case BinaryExpression bexpr:
                    return BinaryExpressionToBinder.CreateBinder(bexpr);

                case UnaryExpression uexpr:
                    return UnaryExpressionToBinder.CreateBinder(uexpr);

                default:
                    break;
            }

            return null;
        }
    }
}