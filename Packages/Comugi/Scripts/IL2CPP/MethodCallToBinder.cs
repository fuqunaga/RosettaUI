using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Comugi.IL2CPP
{
    static class MethodCallToBinder
    {
        public static IBinder CreateBinder(MethodCallExpression mexpr)
        {
            var converterType = typeof(MethodCallToBinder<>).MakeGenericType(mexpr.Type);
            var converter = Activator.CreateInstance(converterType, mexpr) as IBinderCreatable;

            return converter.CreateBinder();
        }
    }

    class MethodCallToBinder<T> : IBinderCreatable
    {
        readonly MethodInfo exprMinfo;
        readonly IBinder objBinder;
        readonly List<IBinder> argBinders;

        public MethodCallToBinder(MethodCallExpression mexpr)
        {
            //minfo = ReGetMethodInfo(mexpr.Method, mexpr.Object.Type);
            exprMinfo = mexpr.Method;
            objBinder = ExpressionUtility_IL2CPP.CreateBinder(mexpr.Object);
            argBinders = mexpr.Arguments?.Select(ExpressionUtility_IL2CPP.CreateBinder).ToList();
        }

        // Genericかつ親クラスのMethodInfoをコールするとIL2CPP環境でフリーズする
        // 回避策として実行時に実態のTypeからMethodInfoを持ってくる
        // 
        // 例
        // public abstract class A<T>
        // {
        //     public abstract T GetInt();
        // }
        //
        // public class B<T> : A<T>
        // {
        //     public T t;
        //     public override T GetInt() => t;
        // }
        //
        // void Start()
        // {
        //     var b = new B<int>() { t = 1 };
        //     var a = b as A<int>;
        //     var miB = typeof(B<int>).GetMethod("GetInt");
        //     var miA = typeof(A<int>).GetMethod("GetInt");
        //     Debug.Log("B: " + b.GetInt());
        //     Debug.Log("A: " + a.GetInt());
        //     Debug.Log("MiB: " + miB.Invoke(b, null));
        //     Debug.Log("MiA: " + miA.Invoke(b, null)); // freeze
        // }

        static Dictionary<(Type, MethodInfo), MethodInfo> methodInfoTable = new Dictionary<(Type, MethodInfo), MethodInfo>();
        static MethodInfo GetMethodInfo(object obj, MethodInfo baseMi)
        {
            var objType = obj.GetType();
            if (!methodInfoTable.TryGetValue((objType, baseMi), out var mi))
            {
                var bindings = baseMi.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
                bindings |= baseMi.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
                mi =  objType.GetMethod(baseMi.Name, bindings);
            }

            return mi;
        }




        T Get()
        {
            var obj = objBinder.GetObject();
            var mi = GetMethodInfo(obj, exprMinfo);
            return (T)mi.Invoke(obj, argBinders?.Select(b => b.GetObject()).ToArray());
        }

        public IBinder CreateBinder() => Binder.Create(Get, null);
    }
}