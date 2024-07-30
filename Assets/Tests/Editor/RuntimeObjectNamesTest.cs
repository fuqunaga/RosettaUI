using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace RosettaUI.Test
{
    public static class RuntimeObjectNamesTest
    {
        // refs: https://gist.github.com/ErnSur/842d72606159fdd865979c4d9db21a18
        [TestCaseSource(nameof(GetTestCases))]
        [TestCaseSource(nameof(GetTestCases2))]
        public static void Should_OutputMatchUnityEditorMethod_When_NicifyClassName(string input)
        {
            var expected = UnityEditor.ObjectNames.NicifyVariableName(input);
            var actual = RuntimeObjectNames.NicifyVariableName(input);
            Assert.AreEqual(expected, actual);
        }

        private static IEnumerable<string> GetTestCases()
        {
            yield return "className2Number";
            yield return "className2number";
            yield return "_className2number_";
            yield return "m_className2number_";
            yield return "_m_className2number_";
            yield return " _className2number_ ";
            yield return "_ className2number _";
            yield return "className22Number";
            yield return "className222Number";
            yield return "className22number";
            yield return "className222number";
            yield return "className23";
            yield return "clasSName23";
            yield return "clasSDKName23";
            yield return "Class_Name_23";
            yield return "SDKName";
            yield return "SDK_Name";
            yield return "sDK_Name";

            yield return "G2A";
            yield return "SDK2";
            yield return "SDK22";
            yield return "SDK2a";
            yield return "SDK22a";

            yield return "SDk2";
            yield return "SDk22";
            yield return "SDk2a";
            yield return "SDk22a";

            yield return "SDK22a2a";
            yield return "SDK2a2a";
            yield return "SDk2";
            yield return "SDk22";
            yield return "SDk22a";
            yield return "SD K2";
            yield return "SD k2";
            yield return "SDK2a";
            yield return "SDk2A";
            yield return "SD2A";
            yield return "2 A2 a";
        }

        private static IEnumerable<string> GetTestCases2()
        {
            var str = "m_AAA02_";
            var results =
                from e in Enumerable.Range(0, 1 << str.Length)
                let p = from b in Enumerable.Range(0, str.Length)
                    select (e & (1 << b)) == 0 ? (char?)null : str[b]
                select string.Join(string.Empty, p);
            return results;
        }
    }
}