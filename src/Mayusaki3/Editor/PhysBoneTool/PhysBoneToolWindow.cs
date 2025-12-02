// Assets/Mayusaki3/PhysBoneTool/Editor/PhysBoneToolWindow.cs
// -*- coding: utf-8 -*-
//
// VRChatアバター向け PhysBone 編集ツール用 EditorWindow（骨組み）
//
// 役割:
// - シーンまたはPrefab内のアバターを指定する
// - 対象プラットフォーム（PC / Mobile）を選択する
// - 今後実装する「PhysBone 一覧取得」「設定の Save/Load」「クリーンアップ処理」の入口になる
//
// 注意点:
// - Editor 拡張用のため、必ず "Editor" フォルダ配下に配置してください
// - VRC SDK への依存はこの段階では持たせていません（PhysBone 検出などは今後実装）
// - ここでは UI と状態管理の骨組みのみを提供しています

using System;
using UnityEngine;
using UnityEditor;

namespace Mayusaki3.PhysBoneTool
{
    #region アバタープラットフォーム種別 (AvatarPlatform)

    /// <summary>
    /// 対象となるアバタープラットフォーム種別
    /// </summary>
    /// <remarks>
    /// - PC: デスクトップ向けアバター
    /// - Mobile: Android / Quest / iOS などのモバイル向けアバター
    /// </remarks>
    public enum AvatarPlatform
    {
        // 表示用ラベルは StringValueAttribute (UIHelper) を利用
        [Mayusaki3.UIHelper.StringValueAttribute("PC（デスクトップ）")]
        PC,

        [Mayusaki3.UIHelper.StringValueAttribute("Mobile（Android / Quest / iOS）")]
        Mobile,
    }

    #endregion

    /// <summary>
    /// PhysBone 編集ツール用 EditorWindow の骨組みクラス
    /// </summary>
    /// <remarks>
    /// 使い方:
    /// - Unity メニューから「Tools/Mayusaki3/PhysBone Tool」を選択して起動
    /// - 「対象アバター」にシーン上の Root GameObject（アバターのルート）を指定
    /// - 「対象プラットフォーム」を選択
    /// - 今後実装する「スキャン」「Save/Load」「クリーンアップ」ボタンから処理を実行
    /// </remarks>
    public class PhysBoneToolWindow : EditorWindow
    {
        #region フィールド

        /// <summary>
        /// 編集対象となるアバターの Root GameObject
        /// </summary>
        /// <remarks>
        /// - 通常は VRC Avatar Descriptor や Animator が付与されている最上位オブジェクトを想定
        /// - シーン上のオブジェクトまたは Prefab Instance を指定可能
        /// </remarks>
        [SerializeField]
        private GameObject _avatarRoot;

        /// <summary>
        /// 対象アバターのプラットフォーム
        /// </summary>
        [SerializeField]
        private AvatarPlatform _platform = AvatarPlatform.PC;

        /// <summary>
        /// シーン上の現在の選択から自動でアバターを設定するかどうか
        /// </summary>
        [SerializeField]
        private bool _autoUseSelection = true;

        #endregion

        #region メニュー / ウィンドウ生成

        /// <summary>
        /// メニューからウィンドウを開く
        /// </summary>
        [MenuItem("Tools/Mayusaki3 Tools/PhysBone Tool")]
        private static void OpenWindow()
        {
            // 既存ウィンドウを取得または新規作成
            var window = GetWindow<PhysBoneToolWindow>();
            window.titleContent = new GUIContent("PhysBone Tool");
            window.Show();
        }

        /// <summary>
        /// 有効化時に呼び出される処理
        /// </summary>
        private void OnEnable()
        {
            // タイトルを設定
            if (titleContent == null || string.IsNullOrEmpty(titleContent.text))
            {
                titleContent = new GUIContent("PhysBone Tool");
            }

            // シーン上の選択を初期値として利用（オプション）
            if (_autoUseSelection && _avatarRoot == null && Selection.activeGameObject != null)
            {
                _avatarRoot = Selection.activeGameObject;
            }
        }

        #endregion

        #region OnGUI（メイン描画処理）

        /// <summary>
        /// EditorWindow のメイン描画処理
        /// </summary>
        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(4);

            DrawAvatarSelectionSection();
            EditorGUILayout.Space(6);

            DrawPlatformSection();
            EditorGUILayout.Space(10);

            DrawActionButtonsSection();
            EditorGUILayout.Space(6);

            DrawFooter();
        }

        #endregion

        #region セクション描画

        /// <summary>
        /// ヘッダー表示（ツール名と簡単な説明）
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.LabelField("VRChat PhysBone 編集ツール（骨組み）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "VRM から VRChat アバターに変換した後の PhysBone 設定を編集／整理するためのツールです。\n" +
                "この段階では、アバター指定とプラットフォーム選択のみ実装しています。\n" +
                "今後「スキャン」「Save/Load」「クリーンアップ」処理を追加する想定です。",
                MessageType.Info);
        }

        /// <summary>
        /// アバター選択セクションを描画
        /// </summary>
        private void DrawAvatarSelectionSection()
        {
            EditorGUILayout.LabelField("1. 対象アバターの指定", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                _autoUseSelection = EditorGUILayout.Toggle(
                    new GUIContent("選択を自動で使用", "SceneView / Hierarchy で選択した GameObject を自動でアバター候補に使用します。"),
                    _autoUseSelection);

                // 現在の選択を利用
                if (_autoUseSelection && Selection.activeGameObject != null)
                {
                    if (_avatarRoot != Selection.activeGameObject)
                    {
                        _avatarRoot = Selection.activeGameObject;
                    }

                    EditorGUILayout.HelpBox(
                        $"現在の選択: {Selection.activeGameObject.name}",
                        MessageType.None);
                }

                // 手動指定用 ObjectField
                _avatarRoot = (GameObject)EditorGUILayout.ObjectField(
                    new GUIContent("対象アバター", "PhysBone 設定を編集したいアバターのルート GameObject を指定します。"),
                    _avatarRoot,
                    typeof(GameObject),
                    true);

                if (_avatarRoot == null)
                {
                    EditorGUILayout.HelpBox(
                        "対象アバターが未指定です。シーン上のアバターを選択するか、ここにドラッグ＆ドロップしてください。",
                        MessageType.Warning);
                }
            }
        }

        /// <summary>
        /// プラットフォーム選択セクションを描画
        /// </summary>
        private void DrawPlatformSection()
        {
            EditorGUILayout.LabelField("2. 対象プラットフォーム", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                // StringValueAttribute を利用したポップアップ表示
                _platform = Mayusaki3.UIHelper.EnumPopup("対象プラットフォーム", _platform);

                EditorGUILayout.HelpBox(
                    "PC と Mobile で別々のアバター（Prefab）を用意している場合、\n" +
                    "将来的にはここでプラットフォーム別の設定を切り替える想定です。",
                    MessageType.None);
            }
        }

        /// <summary>
        /// アクションボタンセクションを描画（現時点ではダミー処理）
        /// </summary>
        private void DrawActionButtonsSection()
        {
            EditorGUILayout.LabelField("3. 操作（現時点ではダミー）", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                using (new EditorGUI.DisabledScope(_avatarRoot == null))
                {
                    if (GUILayout.Button("PhysBone / Collider をスキャン（未実装）", GUILayout.Height(24)))
                    {
                        // 今後、PhysBone / Collider 一覧取得処理をここに実装
                        Debug.Log("[PhysBoneTool] スキャン処理は未実装です。今後追加予定です。");
                    }

                    EditorGUILayout.Space(2);

                    if (GUILayout.Button("設定を保存（未実装）", GUILayout.Height(22)))
                    {
                        // 今後、設定 Save 処理をここに実装
                        Debug.Log("[PhysBoneTool] Save 処理は未実装です。今後追加予定です。");
                    }

                    if (GUILayout.Button("設定を読み込み（未実装）", GUILayout.Height(22)))
                    {
                        // 今後、設定 Load 処理をここに実装
                        Debug.Log("[PhysBoneTool] Load 処理は未実装です。今後追加予定です。");
                    }

                    EditorGUILayout.Space(4);

                    if (GUILayout.Button("Missing Root / 重複Colliderをクリーンアップ（未実装）", GUILayout.Height(24)))
                    {
                        // 今後、クリーンアップ処理をここに実装
                        Debug.Log("[PhysBoneTool] クリーンアップ処理は未実装です。今後追加予定です。");
                    }
                }

                if (_avatarRoot == null)
                {
                    EditorGUILayout.HelpBox(
                        "アクションを実行するには、対象アバターを指定してください。",
                        MessageType.Warning);
                }
            }
        }

        /// <summary>
        /// フッター情報を描画
        /// </summary>
        private void DrawFooter()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(
                    "状態",
                    _avatarRoot != null
                        ? $"アバター: {_avatarRoot.name} / プラットフォーム: {_platform}"
                        : "アバター未指定");
            }
        }

        #endregion
    }
}
