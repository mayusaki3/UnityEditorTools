using System.IO;
using System.Text;
using UnityEngine;

namespace Mayusaki3.PhysBoneTool
{
    /// <summary>
    /// Assets 内に JSON で保存する実装例
    /// </summary>
    public class JsonPhysBoneSettingsStorage : IPhysBoneSettingsStorage
    {
        private readonly string _rootDir;

        public JsonPhysBoneSettingsStorage(string rootDir)
        {
            _rootDir = rootDir;
        }

        private string GetFilePath(string key)
        {
            // key からファイル名を決定（安全な文字列にする処理は要検討）
            var fileName = $"{key}.physbone.json";
            return Path.Combine(_rootDir, fileName);
        }

        public void Save(string key, PhysBoneSettingsData data)
        {
            Directory.CreateDirectory(_rootDir);
            var path = GetFilePath(key);
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json, Encoding.UTF8);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public bool TryLoad(string key, out PhysBoneSettingsData data)
        {
            var path = GetFilePath(key);
            if (!File.Exists(path))
            {
                data = null;
                return false;
            }

            var json = File.ReadAllText(path, Encoding.UTF8);
            data = JsonUtility.FromJson<PhysBoneSettingsData>(json);
            return data != null;
        }

        public IReadOnlyList<string> ListKeys()
        {
            if (!Directory.Exists(_rootDir)) return Array.Empty<string>();
            var files = Directory.GetFiles(_rootDir, "*.physbone.json");
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i])
                    .Replace(".physbone", "");
            }
            return files;
        }

        public bool Delete(string key)
        {
            var path = GetFilePath(key);
            if (!File.Exists(path)) return false;
            File.Delete(path);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            return true;
        }
    }
}
