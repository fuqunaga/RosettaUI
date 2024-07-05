using UnityEngine;

namespace RosettaUI
{
    public static class PersistantData
    {
        private class Wrapper<T>
        {
            public T value;
        }
        
        public static bool TryGet<T>(string key, out T value)
        {
            if (PlayerPrefs.HasKey(key))
            {
                var json = PlayerPrefs.GetString(key);
                var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                value = wrapper.value;
                return true;
            }

            value = default;
            return false;
        }
        
        public static void Set<T>(string key, T value)
        {
            var wrapper = new Wrapper<T> { value = value };
            var json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(key, json);
        }
        
    }
}