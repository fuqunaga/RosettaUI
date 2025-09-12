using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static class PersistentData
    {
        private class Wrapper<T>
        {
            public T value;
        }

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }
        
        public static bool TryGet<T>(string key, out T value)
        {
            if (HasKey(key))
            {
                value = Get<T>(key);
                return true;
            }

            value = default;
            return false;
        }
        
        public static T Get<T>(string key)
        {
            var type = typeof(T);

            if (type == typeof(int))
                return GetInt<T>(key);

            if (type == typeof(float))
                return GetFloat<T>(key);

            if (type == typeof(bool))
                return GetBool<T>(key);
            
            if (type == typeof(string))
                return GetString<T>(key);

            return GetAny<T>(key);
        }
        
        
        public static void Set<T>(string key, T value)
        {
            var type = typeof(T);

            if (type == typeof(int))
            {
                SetInt(key, value);
            }
            else if (type == typeof(float))
            {
                SetFloat(key, value);
            }
            else if (type == typeof(bool))
            {
                SetBool(key, value);
            }
            else if (type == typeof(string))
            {
                SetString(key, value);
            }
            else
            {
                SetAny(key, value);
            }
        }

        

        private static T GetInt<T>(string key)
        {
            var intValue = PlayerPrefs.GetInt(key);
            return UnsafeUtility.As<int, T>(ref intValue);
        }

        private static void SetInt<T>(string key, T value)
        {
            var intValue = UnsafeUtility.As<T, int>(ref value);
            PlayerPrefs.SetInt(key, intValue);
        }
        
        private static T GetFloat<T>(string key)
        {
            var floatValue = PlayerPrefs.GetFloat(key);
            return UnsafeUtility.As<float, T>(ref floatValue);
        }

        private static void SetFloat<T>(string key, T value)
        {
            Assert.IsTrue(typeof(T) == typeof(float));
            var floatValue = UnsafeUtility.As<T, float>(ref value);
            PlayerPrefs.SetFloat(key, floatValue);
        }
        
        private static T GetString<T>(string key)
        {
            var stringValue = PlayerPrefs.GetString(key);
            return UnsafeUtility.As<string, T>(ref stringValue);
        }

        private static void SetString<T>(string key, T value)
        {
            Assert.IsTrue(typeof(T) == typeof(string));
            var stringValue = UnsafeUtility.As<T, string>(ref value);
            PlayerPrefs.SetString(key, stringValue);
        }
        

        private static T GetBool<T>(string key)
        {
            var intValue = PlayerPrefs.GetInt(key);
            var boolValue = intValue != 0;
            return UnsafeUtility.As<bool, T>(ref boolValue);
        }

        private static void SetBool<T>(string key, T value)
        {
            Assert.IsTrue(typeof(T) == typeof(bool));
            var boolValue = UnsafeUtility.As<T, bool>(ref value);
            var intValue = boolValue ? 1 : 0;
            PlayerPrefs.SetInt(key, intValue);
        }

        private static T GetAny<T>(string key)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return default;
            }
            
            var json = PlayerPrefs.GetString(key);
            try
            {
                var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                return wrapper.value;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse {key} as {typeof(T)}: {e}");
            }
            
            return default;
        }

        private static void SetAny<T>(string key, T value)
        {
            var wrapper = new Wrapper<T> { value = value };
            var json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(key, json);
        }
        
    }
}