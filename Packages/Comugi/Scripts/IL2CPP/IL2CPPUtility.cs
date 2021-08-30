using System;
using System.Linq;
using System.Reflection;


namespace RosettaUI.IL2CPP
{
    public static class IL2CPPUtility
    {
        public static MethodInfo ReGetMethodInfo(MethodInfo minfo, Type objType)
        {
            return objType.GetMethod(minfo.Name, minfo.GetParameters().Select(p => p.ParameterType).ToArray());
        }
    }
}