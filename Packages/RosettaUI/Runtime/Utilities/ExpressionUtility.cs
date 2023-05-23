//#define ENABLE_IL2CPP

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static class ExpressionUtility
    {
        public static Func<T> CreateReadFunc<T>(Expression<Func<T>> expression) => expression.Compile();

        public static IBinder<T> CreateBinder<T>(Expression<Func<T>> expression)
        {
#if ENABLE_IL2CPP
            return IL2CPP.ExpressionUtility_IL2CPP.CreateBinder(expression);
#else
            return CreateBinderStandard(expression);
#endif
        }

        private static Binder<T> CreateBinderStandard<T>(Expression<Func<T>> expression)
        {
            var type = typeof(T);
            var getFunc = expression.Compile();

            Action<T> setFunc = null;
            var p = Expression.Parameter(type);

            
            // expression が実態よりアップキャストしているとき対策
            // 
            //  var array = (IList<int>)new int[];
            //  UI.List(() => array);
            // 
            // T is IList<int>
            // bodyType is System.Int32[]
            // 
            // so without casting, the setter will be null.
            var bodyType = expression.Body.Type;
            var rightHand = (type == bodyType)
                ? (Expression) p
                : Expression.Convert(p, bodyType);

            // indexer対策
            // UI.Field(() => list[0]);
            // は expression.Body は list.Item プロパティのGetMethodなので ReadOnly になってしまう
            // expression.Body がプロパティのGetMethodで、SetMethodが存在するならそちらに差し替える
            var setBody = expression.Body;
            if (setBody is MethodCallExpression mc)
            {
                if (TryGetIndexerPropertyInfoFromGetMethod(mc, out var pi) && pi.SetMethod != null)
                {
                    setBody = Expression.Property(mc.Object, pi, mc.Arguments);
                }
            }


            // check writable
            // hint code https://stackoverflow.com/questions/42773488/how-can-i-find-out-if-an-expression-is-writeable
            try
            {
                var lambdaExpr = Expression.Lambda<Action<T>>(Expression.Assign(setBody, rightHand), p);
                setFunc = lambdaExpr.Compile();
            }
            catch (Exception)
            {
                // ignored
            }


            return Binder.Create(getFunc, setFunc);
        }


        #region CreateLabelString

        const string MethodCallDummyInstanceName = "RosettaUI_MethodCallDummy...";
        const string StaticMethodCallDummyClassName = "RosettaUI_StaticMethodCallDummy";
        const string IndexerDummyInstanceName = "RosettaUI_IndexerDummyInstance";
        private static ToLabelStringVisitor _readableExpressionVisitor;


        public static string CreateLabelString<T>(Expression<Func<T>> expression)
        {
            // ReadableExpressions を使いたいが依存するのはちょっと悩ましい
            // https://github.com/AgileObjects/ReadableExpressions
            // return lambda.Body.ToReadableString();
            
            _readableExpressionVisitor ??= new();
            var changedExpr = _readableExpressionVisitor.Visit(expression.Body);
            Assert.IsNotNull(changedExpr);

            var str = changedExpr.ToString();
            
            str = str.Replace(MethodCallDummyInstanceName + ".", "");

            // Replace static method
            // StaticMethodCallDummyClassName + StaticClass = StaticMethod()  -> StaticClass.StaticMethod()
            str = Regex.Replace(str,
                @"\(" + StaticMethodCallDummyClassName + @"(\w+) = (\w+.*)\)",
                "$1.$2");

            // Replace indexer
            // IndexerDummyInstanceName = list.get_item(i) -> list[i]
            str = Regex.Replace(str, 
                @"\(" + IndexerDummyInstanceName + @" = (.*)\.get_Item\((.*)\)\)",
                "$1[$2]");
            str = Regex.Replace(str, 
                @"\(" + IndexerDummyInstanceName + @" = get_Item\((.*)\)\)",
                "[$1]");

            if (str[0] == '(' && str[^1] == ')')
            {
                str = str[1..^1];
            }

            return str;
        }


        private class ToLabelStringVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                var container = node.Expression;
                var member = node.Member;
                
                // "this.member" > "member"
                // "StaticClass.member" > "member"
                if (container is ConstantExpression 
                    || member is FieldInfo { IsStatic: true} or PropertyInfo {GetMethod: {IsStatic: true}}
                   )
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
            /// 1. Add static class name
            ///
            /// 2. Indexer
            ///   list.get_item(0) -> list[0]
            ///
            /// 3. Remove complex "this" string
            ///    Warning: Constant is not only "this"
            ///
            /// </summary>
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var method = node.Method;

                // add static class name
                // 一度Assignの形にしてあとで置換する
                // StaticMethod() -> StaticMethodCallDummyClassName+"StaticClass" = StaticMethod() 
                if ( method.IsStatic )
                {
                    var param = Expression.Parameter(node.Type, StaticMethodCallDummyClassName + method.DeclaringType?.Name);
                    return Expression.Assign(param, node);
                }
   
                // Indexer
                // 一度Assignの形にしてあとで置換する
                // list.get_item(i) -> IndexerDummyInstanceName = list.get_item(i) 
                if ( TryGetIndexerPropertyInfoFromGetMethod(node, out _))
                {
                    var param = Expression.Parameter(node.Type, IndexerDummyInstanceName);
                    return Expression.Assign(param, RemoveInstanceNameIfConstant(node));
                }
                
                // remove "this"
                return RemoveInstanceNameIfConstant(node);
            }
            
                          
            Expression RemoveInstanceNameIfConstant(MethodCallExpression expression)
            {
                var obj = expression.Object;
                    
                if (obj is ConstantExpression)
                {
                    var param = Expression.Parameter(obj.Type, MethodCallDummyInstanceName);
                    return Expression.Call(param, expression.Method, expression.Arguments);
                }
                
                return base.VisitMethodCall(expression);
            }
        }


        #endregion


        #region GetAttribute

        public static TAttribute GetAttribute<TAttribute>(LambdaExpression lambda)
            where TAttribute : Attribute
        {
            if (lambda.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.GetCustomAttribute<TAttribute>();
            }

            return null;
        }

        #endregion

        static bool TryGetIndexerPropertyInfoFromGetMethod(MethodCallExpression mc, out PropertyInfo pi)
        {
            var mi = mc.Method;
            var binding = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            pi = mi.DeclaringType?.GetProperties(binding).FirstOrDefault(info => info.GetMethod == mi);

            return pi != null;
        }
    }
}