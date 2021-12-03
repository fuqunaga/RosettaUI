//#define ENABLE_IL2CPP

using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;


namespace RosettaUI
{
    public static class ExpressionUtility
    {
        public static Func<T> CreateReadFunc<T>(Expression<Func<T>> expression) => expression.Compile();
        
        public static IBinder<T> CreateBinder<T>(Expression<Func<T>> expression)
        {
#if ENABLE_IL2CPP
            return IL2CPP.ExpressionUtility_IL2CPP.CreateBinder(lambda);
#else
            return _CreateBinder(expression);
#endif
        }

        static Binder<T> _CreateBinder<T>(Expression<Func<T>> expression)
        {
            var type = typeof(T);
            var getFunc = expression.Compile();

            Action<T> setFunc = null;
            var p = Expression.Parameter(type);
            
            // for UI.List()
            // 
            //   var array = new int[];
            //   UI.List(() => array);
            
            // T is IList<int>
            // bodyType is System.Int32[]
            // 
            // so without casting, the setter will be null.
            var bodyType = expression.Body.Type;
            var rightHand = (type == bodyType)
                ? (Expression)p
                : Expression.Convert(p, bodyType);
            
            
            // check writable
            // hint code https://stackoverflow.com/questions/42773488/how-can-i-find-out-if-an-expression-is-writeable
            try
            {
                var lambdaExpr = Expression.Lambda<Action<T>>(Expression.Assign(expression.Body, rightHand), p);
                setFunc = lambdaExpr.Compile();
            }
            catch (Exception)
            {
                // ignored
            }


            return Binder.Create(getFunc, setFunc);
        }


        #region CreateLabelString

        const string MethodCallDummyInstanceName = "MethodCallDummy...";
        static readonly SimplifyVisitor ReadableExpressionVisitor = new SimplifyVisitor();

        public static string CreateLabelString<T>(Expression<Func<T>> expression)
        {
#if false
            // ReadableExpressions を使いたいが依存するのはちょっと悩ましい
            // https://github.com/AgileObjects/ReadableExpressions
            return lambda.Body.ToReadableString();
#else

            //return lambda.Body.ToString();
            var changedExpr = ReadableExpressionVisitor.Visit(expression.Body);
            return changedExpr?.ToString().Replace(MethodCallDummyInstanceName + ".", "");
#endif
        }


        private class SimplifyVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                var container = node.Expression;
                var member = node.Member;

                if (container is ConstantExpression)
                {
                    Type type;
                    switch (member)
                    {
                        case FieldInfo fi:
                            type = fi.FieldType;
                            break;
                        case PropertyInfo pi:
                            type = pi.PropertyType;
                            break;

                        default:
                            return node;
                    }

                    var param = Expression.Parameter(type, member.Name);
                    return param;
                }

                return base.VisitMember(node);
            }


            /// <summary>
            /// Remove complex "this" string
            /// 
            /// Warning: Constant is not only "this"
            /// This will make that 1.ToString() > ToString()
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var obj = node.Object;

                if (obj is ConstantExpression)
                {
                    var param = Expression.Parameter(obj.Type, MethodCallDummyInstanceName);
                    return Expression.Call(param, node.Method, node.Arguments);
                }

                return base.VisitMethodCall(node);
            }
        }

        #endregion


        #region GetAttribute

        public static TAttribute GetAttribute<T, TAttribute>(Expression<Func<T>> lambda)
            where TAttribute : Attribute
        {
            if (lambda.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.GetCustomAttribute<TAttribute>();
            }

            return null;
        }

        #endregion
    }
}