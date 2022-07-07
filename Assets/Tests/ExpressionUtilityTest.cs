using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace RosettaUI.Test
{
    public class ExpressionUtilityTest
    {
        // ReSharper disable once ClassNeverInstantiated.Local
        private class Class
        {
            // ReSharper disable once ConvertToConstant.Local
            public readonly int field = 0;
            public int Property { get; set; } = 0;
            // ReSharper disable once MemberCanBeMadeStatic.Local
            public int Method() => 0;

            public static int classStaticField;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public static int ClassStaticProperty { get; set; }
            public static int ClassStaticMethod() => 0;
        }
        
        public static class StaticClass
        {
            // ReSharper disable once ConvertToConstant.Global
            public static readonly int StaticClassField = 0;
            public static int StaticClassProperty { get; set; } = 0;
            public static int Method() => 0;
        }
        
        private int _field;
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private int Property { get; set; }
        // ReSharper disable once MemberCanBeMadeStatic.Local
        int Method() => 0;

        private static int _staticField;
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private static int StaticProperty { get; set; }
        private static int StaticMethod() => 0;
        
        
        Class _instance;
        
        [Test]
        public void CreateLabelString()
        {
            // できれば StaticClass.StaticClassField => StaticClass.StaticClassField としたいが、
            // クラス内呼び出しの省略形と区別ができず
            // _staticField => ExpressionUtilityTest._staticField
            // となってしまうので、
            // StaticClass.StaticClassField => StaticClassField
            // を許容する
            Assert.AreEqual("_field", ExpressionUtility.CreateLabelString(() => _field));
            Assert.AreEqual("Property", ExpressionUtility.CreateLabelString(() => Property));
            Assert.AreEqual("Method()", ExpressionUtility.CreateLabelString(() => Method()));
            Assert.AreEqual("_staticField", ExpressionUtility.CreateLabelString(() => _staticField));
            Assert.AreEqual("StaticProperty", ExpressionUtility.CreateLabelString(() => StaticProperty));
            Assert.AreEqual("ExpressionUtilityTest.StaticMethod()", ExpressionUtility.CreateLabelString(() => StaticMethod()));

            Assert.AreEqual("_instance.field", ExpressionUtility.CreateLabelString(() => _instance.field));
            Assert.AreEqual("_instance.Property", ExpressionUtility.CreateLabelString(() => _instance.Property));
            Assert.AreEqual("_instance.Method()", ExpressionUtility.CreateLabelString(() => _instance.Method()));
            Assert.AreEqual("classStaticField", ExpressionUtility.CreateLabelString(() => Class.classStaticField));
            Assert.AreEqual("ClassStaticProperty", ExpressionUtility.CreateLabelString(() => Class.ClassStaticProperty));
            Assert.AreEqual("Class.ClassStaticMethod()", ExpressionUtility.CreateLabelString(() => Class.ClassStaticMethod()));

            Assert.AreEqual("StaticClassField", ExpressionUtility.CreateLabelString(() => StaticClass.StaticClassField));
            Assert.AreEqual("StaticClassProperty", ExpressionUtility.CreateLabelString(() => StaticClass.StaticClassProperty));
            Assert.AreEqual("StaticClass.Method()", ExpressionUtility.CreateLabelString(() => StaticClass.Method()));
        }
    }
}