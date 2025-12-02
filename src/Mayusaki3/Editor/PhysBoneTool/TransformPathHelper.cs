// Assets/Mayusaki3/Editor/PhysBoneTool/TransformPathHelper.cs
// -*- coding: utf-8 -*-
//
// 役割:
// - Transform からパス文字列 ("Root/Child/GrandChild") を生成するヘルパー
// - パス文字列から Transform を探索するヘルパー
//
// 注意:
// - Editor 専用ツールだが、処理自体はランタイムでも使える純粋ロジック。
// - ここでは UnityEngine.Transform 型を使用するため、using UnityEngine が必要。

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mayusaki3.PhysBoneTool
{
    public static class TransformPathHelper
    {
        /// <summary>
        /// root から target までの Transform パスを "Root/Child/GrandChild" の形式で取得する。
        /// </summary>
        /// <param name="root">ルート Transform（パスの起点）</param>
        /// <param name="target">パスを取得したい Transform</param>
        /// <returns>
        /// パス文字列。root または target が null の場合、あるいは target が root 配下に無い場合は null。
        /// </returns>
        public static string GetPath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return null;
            }

            // target から root に向かって親を辿り、名前を積む
            var names = new List<string>();
            var current = target;

            while (current != null)
            {
                names.Add(current.name);

                if (current == root)
                {
                    break;
                }

                current = current.parent;
            }

            if (current != root)
            {
                // root にたどり着かなかった → root 配下ではない
                return null;
            }

            // 現在 names は [target, parent, ..., root] の順なので反転
            names.Reverse();
            return string.Join("/", names);
        }

        /// <summary>
        /// root 配下から、指定パスに対応する Transform を探す。
        /// </summary>
        /// <param name="root">検索の起点となるルート Transform</param>
        /// <param name="path">"Root/Child/GrandChild" のようなパス文字列</param>
        /// <returns>該当する Transform。見つからない場合は null。</returns>
        public static Transform FindByPath(Transform root, string path)
        {
            if (root == null) return null;
            if (string.IsNullOrEmpty(path)) return null;

            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return null;

            Transform current = root;
            int index = 0;

            // 先頭が root.name と一致している場合はその分をスキップしてもよい
            if (parts[0] == root.name)
            {
                index = 1;
            }

            for (int i = index; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }
    }
}
