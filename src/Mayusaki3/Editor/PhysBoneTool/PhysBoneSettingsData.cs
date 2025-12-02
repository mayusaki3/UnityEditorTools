// Assets/Mayusaki3/Editor/PhysBoneTool/PhysBoneSettingsData.cs
// -*- coding: utf-8 -*-
//
// 役割:
// - PhysBoneTool で保存・読み込みする設定データ（DTO）定義。
// - JsonPhysBoneSettingsStorage などの永続化クラスと、Scan/Apply helper の間でやり取りする。
// 注意:
// - Editor 専用ツールのため、Editor アセンブリ（Editor フォルダ配下）に配置する。

using System;                    // [Serializable] 用
using System.Collections.Generic; // List<T> 用

namespace Mayusaki3.PhysBoneTool
{
    /// <summary>
    /// 1アバター分の PhysBone 設定全体を表すデータ。
    /// </summary>
    [Serializable]
    public class PhysBoneSettingsData
    {
        /// <summary>
        /// アバター名（ヒューマン向け識別用）
        /// </summary>
        public string avatarName;

        /// <summary>
        /// アバターを一意に識別するID（PrefabパスやGUIDなど、今後の拡張用）
        /// </summary>
        public string avatarGuid;

        /// <summary>
        /// 対象プラットフォーム（PC / Mobile など）
        /// </summary>
        public AvatarPlatform platform;

        /// <summary>
        /// PhysBone 設定の一覧
        /// </summary>
        public List<PhysBoneEntry> physBones = new List<PhysBoneEntry>();

        /// <summary>
        /// PhysBoneCollider 設定の一覧
        /// </summary>
        public List<PhysBoneColliderEntry> colliders = new List<PhysBoneColliderEntry>();
    }

    /// <summary>
    /// 1つの VRCPhysBone コンポーネントに対応する設定データ。
    /// </summary>
    [Serializable]
    public class PhysBoneEntry
    {
        /// <summary>
        /// アバターRootからこのコンポーネントが載っているTransformまでのパス
        /// 例: "Armature/Hips/Spine/Chest/Head"
        /// </summary>
        public string transformPath;

        /// <summary>
        /// 同一 Transform 上に複数の VRCPhysBone がある場合の識別子（インデックスなど）
        /// </summary>
        public int componentIndex;

        // ここに必要なパラメータを追加（今は代表例だけ記載）
        public float pull;
        public float spring;
        public float stiffness;
        public float gravity;
        public float radius;
    }

    /// <summary>
    /// 1つの VRCPhysBoneCollider コンポーネントに対応する設定データ。
    /// </summary>
    [Serializable]
    public class PhysBoneColliderEntry
    {
        /// <summary>
        /// アバターRootからこのコンポーネントが載っているTransformまでのパス
        /// </summary>
        public string transformPath;

        /// <summary>
        /// 同一 Transform 上に複数の Collider がある場合の識別子（インデックスなど）
        /// </summary>
        public int componentIndex;

        // 必要なパラメータをここに追加（代表例）
        public float radius;
        public float height;

        // Center や Direction など、必要に応じて構造体 or 個別値として追加
        // public Vector3 center;
        // public int direction;
    }
}
