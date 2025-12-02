// Assets/Mayusaki3/Editor/PhysBoneTool/JsonPhysBoneSettingsStorage.cs
// -*- coding: utf-8 -*-
//
// 役割:
// - PhysBoneSettingsData を JSON ファイルとして保存・読み込みする Editor 用ストレージ実装。
// - 保存先はプロジェクト内の任意ディレクトリ（例: Assets/Mayusaki3/PhysBoneTool/Presets）

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mayusaki3.PhysBoneTool
{
    /// <summary>
    /// Assets 内のフォルダに JSON で保存する実装例。
    /// </summary>
    public class JsonPhysBoneSettingsStorage : IPhysBoneSettingsStorage
    {
        /// <summary>
        /// 保存先のルートディレクトリ（プロジェクト内パス）。
        /// 例: "Assets/Mayusaki3/PhysBoneTool/Presets"
        /// </summary>
        private readonly string _rootDir;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="rootDir">保存先ルートディレクトリ（プロジェクト内パス）</param>
        public JsonPhysBoneSettingsStorage(string rootDir)
        {
            _rootDir = rootDir;
        }

        /// <summary>
        /// key から実ファイルパスを生成する。
        /// </summary>
        private string GetFilePath(string key)
        {
            // TODO: key に使えない文字の置き換えなどは必要に応じて実装
            var fileName = $"{key}.physbone.json";
            return Path.Combine(_rootDir, fileName);
        }

        public void Save(string key, PhysBoneSettingsData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key is null or empty", nameof(key));

            var dir = _rootDir;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var path = GetFilePath(key);
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json, Encoding.UTF8);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public bool TryLoad(string key, out PhysBoneSettingsData data)
        {
            if (string.IsNullOrEmpty(key))
            {
                data = null;
                return false;
            }

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
            if (!Directory.Exists(_rootDir))
            {
                return Array.Empty<string>();
            }

            var files = Directory.GetFiles(_rootDir, "*.physbone.json");
            if (files.Length == 0) return Array.Empty<string>();

            var keys = new List<string>(files.Length);
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (name.EndsWith(".physbone", StringComparison.Ordinal))
                {
                    name = name.Substring(0, name.Length - ".physbone".Length);
                }
                keys.Add(name);
            }

            return keys;
        }

        public bool Delete(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            var path = GetFilePath(key);
            if (!File.Exists(path)) return false;

            File.Delete(path);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            return true;
        }
    }
}
