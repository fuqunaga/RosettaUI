using System;
using System.ComponentModel;
using System.Linq.Expressions;
using UnityEngine.Assertions;

namespace Comugi.IL2CPP
{
    static class UnaryExpressionToBinder
    {
        public static IBinder CreateBinder(UnaryExpression expr)
        {
            var converterType = typeof(UnaryExpressionToBinder<>).MakeGenericType(expr.Type);
            var converter = Activator.CreateInstance(converterType, expr) as IBinderCreatable;

            return converter.CreateBinder();
        }
    }

    class UnaryExpressionToBinder<T> : IBinderCreatable
    {
        readonly IBinder operandBinder;
        readonly Func<T> getFunc;


        public UnaryExpressionToBinder(UnaryExpression expr)
        {
            operandBinder = ExpressionUtility_IL2CPP.CreateBinder(expr.Operand);

            if (expr.Method == null)
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Convert:
                        {
                            var converter = TypeDescriptor.GetConverter(typeof(T));
                            if (converter.CanConvertFrom(expr.Operand.Type))
                            {
                                getFunc = () => (T)converter.ConvertFrom(GetOperand());
                            }
                            else if (typeof(T).IsEnum)
                            {
                                getFunc = () => (T)GetOperand();
                            }
                            break;
                        }
                    
                }

                Assert.IsNotNull(getFunc);
            }
            else
            {
                getFunc = () => (T)expr.Method.Invoke(null, new[] { GetOperand() });
            }
        }

        object GetOperand() => operandBinder.GetObject();
        
        public IBinder CreateBinder() => Binder.Create(getFunc, null);
    }
}