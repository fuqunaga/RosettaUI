using System;
using System.Reflection;
using UnityEngine.Assertions;

namespace RosettaUI.IL2CPP
{
    internal interface IBinderCreatable
    {
        IBinder CreateBinder();
    }

    internal static class MemberInfoToBinder
    {

        public static IBinder CreateBinder(IBinder parentBinder, MemberInfo mi, Type valueType)
        {
            var converterType = typeof(MemberInfoToBinder<>).MakeGenericType(valueType);
            var converter = Activator.CreateInstance(converterType, parentBinder, mi) as IBinderCreatable;
            return converter.CreateBinder();
        }
    }

    internal class MemberInfoToBinder<T> : IBinderCreatable
    {
        readonly IBinder parentBinder;

        readonly FieldInfo fi;
        readonly PropertyInfo pi;

        public Type Type => fi?.FieldType ?? pi.PropertyType;
        public T GetValue() => (T)(fi?.GetValue(GetParent()) ?? pi.GetValue(GetParent()));
        public void SetValue(T v)
        {
            var parent = GetParent();

            if (fi != null)
                fi.SetValue(parent, v);
            else
                pi.SetValue(parent, v);

            SetParent(parent);
        }

        object GetParent() => parentBinder?.GetObject();
        void SetParent(object obj) => parentBinder?.SetObject(obj);

        public MemberInfoToBinder(IBinder parent, MemberInfo mi)
        {
            this.parentBinder = parent;

            fi = mi as FieldInfo;
            pi = mi as PropertyInfo;

            Assert.IsTrue(fi != null || pi != null);
            if (parentBinder == null)
            {
                Assert.IsTrue(fi == null || fi.IsStatic);
                Assert.IsTrue(pi == null || pi.GetGetMethod().IsStatic);
            }
        }

        public IBinder CreateBinder()
        {
            return Binder.Create(GetValue, SetValue);
        }
    }
}