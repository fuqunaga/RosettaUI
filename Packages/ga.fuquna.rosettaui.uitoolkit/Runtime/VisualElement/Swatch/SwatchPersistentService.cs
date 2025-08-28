using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// Swatchのセーブロード
    /// </summary>
    public class SwatchPersistentService<TValue>
    {
        [Serializable]
        public struct NameAndValue
        {
            public string name;
            public TValue value;
        }


        public event Action onSaveSwatches;
        
        private readonly string _keyPrefix;
        
        
        public bool HasSwatches => PersistentData.HasKey(KeySwatches);
        
        protected string KeySwatches => $"{_keyPrefix}-Swatches";
        
        public SwatchPersistentService(string keyPrefix) => _keyPrefix = keyPrefix;

        
        public void SaveSwatches(IEnumerable<SwatchBase<TValue>> swatches)
        {
            SaveSwatches(swatches.Select(swatch => new NameAndValue()
            {
                name = swatch.Label,
                value = swatch.Value
            }));
        }

        public void SaveSwatches(IEnumerable<NameAndValue> nameAndValueEnumerable)
        {
            using var _ = ListPool<NameAndValue>.Get(out var nameAndValues);
            nameAndValues.AddRange(nameAndValueEnumerable);
            
            PersistentData.Set(KeySwatches, nameAndValues);
            
            onSaveSwatches?.Invoke();
        }
        
        public IEnumerable<NameAndValue> LoadSwatches()
        {
            return PersistentData.Get<List<NameAndValue>>(KeySwatches);
        }
        
        /// <summary>
        /// 汎用サービス
        /// </summary>
        public T Get<T>(string key)
        {
            return PersistentData.Get<T>($"{_keyPrefix}-{key}");
        }
        
        public void Set<T>(string key, T value)
        {
            PersistentData.Set($"{_keyPrefix}-{key}", value);
        }
    }
}