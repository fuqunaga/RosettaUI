using System.Collections.Generic;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace RosettaUI.Test
{
    public static class MiniJsonTest
    {
        private const string json = "{ \"array\": [1.44,2,3], " +
                                    "\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
                                    "\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
                                    "\"unicode\": \"\\u3041 Men\u00fa sesi\u00f3n\", " +
                                    "\"int\": 65536, " +
                                    "\"float\": 3.1415926, " +
                                    "\"bool\": true, " +
                                    "\"null\": null }";
        
        [Test]
        public static void Serialize_MatchEditorJson()
        {
            var dic = EditorJson.Deserialize(json) as Dictionary<string, object>;
            
            var expected = EditorJson.Serialize(dic);
            var actual = Json.Serialize(dic);
            
            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public static void Deserialize_MatchEditorJson()
        {

            
            var expected = EditorJson.Deserialize(json) as Dictionary<string, object>;
            var actual = Json.Deserialize(json) as Dictionary<string, object>;
            
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}