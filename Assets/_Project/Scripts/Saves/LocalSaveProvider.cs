using System.Threading.Tasks;
using UnityEngine;

namespace IdleBiz.Saves
{
    public sealed class LocalSaveProvider : ISaveProvider
    {
        private readonly string _key;
        public LocalSaveProvider(string key) => _key = key;

        public Task<SaveData> LoadAsync()
        {
            if (!PlayerPrefs.HasKey(_key)) return Task.FromResult<SaveData>(null);
            var json = PlayerPrefs.GetString(_key);
            var data = JsonUtility.FromJson<SaveData>(json);
            return Task.FromResult(data);
        }

        public Task SaveAsync(SaveData data)
        {
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(_key, json);
            PlayerPrefs.Save();
            return Task.CompletedTask;
        }
    }
}

