using System;
using System.Linq.Expressions;
using NUnit.Framework;
using RosettaUI.Builder;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace RosettaUI.Test
{
    public class ExpressionUtilityTest
    {
        public class Class
        {
            public int field = 0;
            public int Property { get; set; } = 0;
            public int Method() => 0;
            public static int StaticMethod() => 0;
        }
        
        public static class StaticClass
        {
            public static int field = 0;
            public static int Property { get; set; } = 0;
            public static int Method() => 0;
            public static int StaticMethod() => 0;
        }
        
        private int _field;
        private int Property { get; set; } = 0;
        int Method() => 0;

        static int StaticMethod() => 0;
        

        
        Class _instance;
        
        [Test]
        public void Test_CreateLabelString()
        {
            Assert.AreEqual("_field", ExpressionUtility.CreateLabelString(() => _field));
            Assert.AreEqual("Property", ExpressionUtility.CreateLabelString(() => Property));
            Assert.AreEqual("Method()", ExpressionUtility.CreateLabelString(() => Method()));
            Assert.AreEqual("ExpressionUtilityTest.StaticMethod()", ExpressionUtility.CreateLabelString(() => StaticMethod()));

            Assert.AreEqual("_instance.field", ExpressionUtility.CreateLabelString(() => _instance.field));
            Assert.AreEqual("_instance.Property", ExpressionUtility.CreateLabelString(() => _instance.Property));
            Assert.AreEqual("_instance.Method()", ExpressionUtility.CreateLabelString(() => _instance.Method()));
            Assert.AreEqual("Class.StaticMethod()", ExpressionUtility.CreateLabelString(() => Class.StaticMethod()));

            Assert.AreEqual("StaticClass.field", ExpressionUtility.CreateLabelString(() => StaticClass.field));
            Assert.AreEqual("StaticClass.Property", ExpressionUtility.CreateLabelString(() => StaticClass.Property));
            Assert.AreEqual("StaticClass.Method()", ExpressionUtility.CreateLabelString(() => StaticClass.Method()));
            Assert.AreEqual("StaticClass.StaticMethod()", ExpressionUtility.CreateLabelString(() => StaticClass.StaticMethod()));
        }
    }
}