// Assets/Mayusaki3/PhysBoneTool/Editor/PhysBoneToolWindow.cs
// -*- coding: utf-8 -*-
//
// VRChatアバター向け PhysBone 編集ツール用 EditorWindow
//
// 役割:
// - シーンまたはPrefab内のアバターを指定する
// - 対象プラットフォーム（PC / Mobile）を選択する
// - 「PhysBone / Collider スキャン」「設定の Save/Load」「Missing Root / 重複Collider クリーンアップ」の入口になる
//
// 注意点:
// - Editor 拡張用のため、必ず "Editor" フォルダ配下に配置してください
// - VRC SDK への依存はこの段階では最小限にとどめ、UI とヘルパー呼び出しの制御に専念します

using System;
using System.IO;
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
    /// PhysBone 編集ツール用 EditorWindow クラス
    /// </summary>
    /// <remarks>
    /// 使い方:
    /// - Unity メニューから「Tools/Mayusaki3 Tools/PhysBone Tool」を選択して起動
    /// - 「対象アバター」にシーン上の Root GameObject（アバターのルート）を指定
    /// - 「対象プラットフォーム」を選択
    /// - 「PhysBone / Collider をスキャン」「設定を保存/読み込み」「クリーンアップ」ボタンから処理を実行
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

        /// <summary>
        /// 直近の状態メッセージ
        /// </summary>
        [SerializeField]
        private string _statusMessage = "アバター未指定です。";

        /// <summary>
        /// 直近のスキャンまたは読み込みで得られた PhysBone 設定
        /// </summary>
        [SerializeField]
        private PhysBoneSettingsData _currentSettings;

        /// <summary>
        /// 直近に Save/Load に使用した設定ファイルパス
        /// </summary>
        [SerializeField]
        private string _lastSettingsPath;

        /// <summary>
        /// PhysBone / Collider スキャン処理ヘルパー
        /// </summary>
        private IPhysBoneScanHelper _scanHelper;

        /// <summary>
        /// 設定の保存・読み込み処理ヘルパー
        /// </summary>
        private IPhysBoneSettingsStorage _settingsStorage;

        /// <summary>
        /// Missing Root / 重複Collider クリーンアップ処理ヘルパー
        /// </summary>
        private IPhysBoneCleanupHelper _cleanupHelper;

        #endregion

        #region プロパティ

        /// <summary>
        /// 有効なアバターが指定されているかどうか
        /// </summary>
        private bool HasValidAvatar => _avatarRoot != null;

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
            // 最小サイズと初期サイズを設定
            window.minSize = new Vector2(420f, 480f);
            if (window.position.width < 420f || window.position.height < 480f)
            {
                var pos = window.position;
                pos.width = Mathf.Max(pos.width, 420f);
                pos.height = Mathf.Max(pos.height, 480f);
                window.position = pos;
            }
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

            // 最小ウィンドウサイズを設定
            minSize = new Vector2(420f, 480f);

            // シーン上の選択を初期値として利用（オプション）
            if (_autoUseSelection && _avatarRoot == null && Selection.activeGameObject != null)
            {
                _avatarRoot = Selection.activeGameObject;
            }

            // ヘルパーを生成
            _scanHelper      = new PhysBoneScanHelper();
            _settingsStorage = new JsonPhysBoneSettingsStorage();
            _cleanupHelper   = new PhysBoneCleanupHelper();

            if (string.IsNullOrEmpty(_statusMessage))
            {
                _statusMessage = HasValidAvatar
                    ? $"アバター: {_avatarRoot.name} / プラットフォーム: {_platform}"
                    : "アバター未指定です。";
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
            EditorGUILayout.LabelField("VRChat PhysBone 編集ツール", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "VRM から VRChat アバターに変換した後の PhysBone 設定を編集／整理するためのツールです。\n" +
                "このウィンドウから、アバター指定・プラットフォーム選択・スキャン／Save／Load／クリーンアップ処理を実行します。",
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
                        SetStatus($"アバターを選択中のオブジェクトに更新しました: {_avatarRoot.name}");
                    }

                    EditorGUILayout.HelpBox(
                        $"現在の選択: {Selection.activeGameObject.name}",
                        MessageType.None);
                }

                // 手動指定用 ObjectField
                var newAvatarRoot = (GameObject)EditorGUILayout.ObjectField(
                    new GUIContent("対象アバター", "PhysBone 設定を編集したいアバターのルート GameObject を指定します。"),
                    _avatarRoot,
                    typeof(GameObject),
                    true);

                if (newAvatarRoot != _avatarRoot)
                {
                    _avatarRoot = newAvatarRoot;
                    if (_avatarRoot != null)
                    {
                        SetStatus($"アバターを変更しました: {_avatarRoot.name}");
                    }
                    else
                    {
                        SetStatus("アバター未指定です。");
                    }
                }

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
                var newPlatform = Mayusaki3.UIHelper.EnumPopup("対象プラットフォーム", _platform);
                if (newPlatform.Equals(_platform) == false)
                {
                    _platform = newPlatform;
                    SetStatus($"プラットフォームを変更しました: {_platform}");
                }

                EditorGUILayout.HelpBox(
                    "PC と Mobile で別々のアバター（Prefab）を用意している場合、\n" +
                    "ここでプラットフォーム別の設定を切り替える想定です。",
                    MessageType.None);
            }
        }

        /// <summary>
        /// アクションボタンセクションを描画
        /// </summary>
        private void DrawActionButtonsSection()
        {
            EditorGUILayout.LabelField("3. 操作", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope())
            {
                using (new EditorGUI.DisabledScope(!HasValidAvatar))
                {
                    // 3-1. スキャン
                    if (GUILayout.Button("PhysBone / Collider をスキャン", GUILayout.Height(24)))
                    {
                        DoScan();
                    }

                    EditorGUILayout.Space(2);

                    // 3-2. 設定を保存（スキャン済み or 読み込み済みで有効）
                    using (new EditorGUI.DisabledScope(_currentSettings == null))
                    {
                        if (GUILayout.Button("設定を保存", GUILayout.Height(22)))
                        {
                            DoSave();
                        }
                    }

                    // 3-3. 設定を読み込み
                    if (GUILayout.Button("設定を読み込み", GUILayout.Height(22)))
                    {
                        DoLoad();
                    }

                    EditorGUILayout.Space(4);

                    // 3-4. Missing Root / 重複Collider クリーンアップ
                    if (GUILayout.Button("Missing Root / 重複Colliderをクリーンアップ", GUILayout.Height(24)))
                    {
                        DoCleanup();
                    }
                }

                if (!HasValidAvatar)
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
                    _statusMessage ?? string.Empty);
            }
        }

        #endregion

        #region 操作処理（Scan / Save / Load / Cleanup）

        /// <summary>
        /// PhysBone / Collider のスキャン処理を実行する
        /// </summary>
        private void DoScan()
        {
            if (!HasValidAvatar)
            {
                SetStatus("対象アバターが指定されていません。");
                return;
            }

            try
            {
                _currentSettings = _scanHelper.Scan(_avatarRoot, _platform);

                if (_currentSettings == null)
                {
                    SetStatus("スキャン結果が空です。PhysBone が存在しない可能性があります。");
                }
                else
                {
                    SetStatus("PhysBone / Collider のスキャンが完了しました。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhysBoneTool] Scan failed: {ex}");
                SetStatus("スキャンに失敗しました。詳細は Console を確認してください。");
            }
        }

        /// <summary>
        /// スキャン結果または読み込み済みの設定を JSON ファイルに保存する
        /// </summary>
        private void DoSave()
        {
            if (_currentSettings == null)
            {
                SetStatus("保存する設定がありません。先にスキャンまたは設定ファイルの読み込みを行ってください。");
                return;
            }

            string defaultName = _avatarRoot != null
                ? $"{_avatarRoot.name}_PhysBoneSettings.json"
                : "PhysBoneSettings.json";

            string initialDir = "Assets";
            if (!string.IsNullOrEmpty(_lastSettingsPath))
            {
                // SaveFilePanelInProject の initialPath は "Assets/..." ベースなので、
                // 直近のパスがプロジェクト内であればそれを利用
                if (_lastSettingsPath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
                {
                    initialDir = Path.GetDirectoryName(_lastSettingsPath)?.Replace("\\", "/") ?? "Assets";
                    defaultName = Path.GetFileName(_lastSettingsPath);
                }
            }

            string assetPath = EditorUtility.SaveFilePanelInProject(
                "PhysBone 設定の保存先",
                defaultName,
                "json",
                "PhysBone 設定の保存先を選択してください。",
                initialDir);

            if (string.IsNullOrEmpty(assetPath))
            {
                // ユーザーがキャンセルした場合
                return;
            }

            try
            {
                _settingsStorage.Save(assetPath, _currentSettings);
                _lastSettingsPath = assetPath;
                AssetDatabase.Refresh();
                SetStatus($"設定を保存しました: {assetPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhysBoneTool] Save failed: {ex}");
                SetStatus("設定の保存に失敗しました。詳細は Console を確認してください。");
            }
        }

        /// <summary>
        /// JSON の設定ファイルを読み込む
        /// </summary>
        private void DoLoad()
        {
            string initialDir = Application.dataPath;
            if (!string.IsNullOrEmpty(_lastSettingsPath))
            {
                // 直近のパスがプロジェクト内の "Assets/..." であればフルパスに変換
                if (_lastSettingsPath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
                {
                    string full = Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? "", _lastSettingsPath);
                    initialDir = Path.GetDirectoryName(full) ?? initialDir;
                }
                else if (Path.IsPathRooted(_lastSettingsPath))
                {
                    initialDir = Path.GetDirectoryName(_lastSettingsPath) ?? initialDir;
                }
            }

            string path = EditorUtility.OpenFilePanel(
                "PhysBone 設定ファイルを選択",
                initialDir,
                "json");

            if (string.IsNullOrEmpty(path))
            {
                // ユーザーがキャンセルした場合
                return;
            }

            try
            {
                var loaded = _settingsStorage.Load(path);
                if (loaded == null)
                {
                    SetStatus("設定ファイルの読み込みに失敗しました。形式が不正の可能性があります。");
                    return;
                }

                _currentSettings = loaded;
                _lastSettingsPath = path;
                SetStatus($"設定を読み込みました: {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhysBoneTool] Load failed: {ex}");
                SetStatus("設定ファイルの読み込みに失敗しました。詳細は Console を確認してください。");
            }
        }

        /// <summary>
        /// Missing Root / 重複Collider のクリーンアップ処理を実行する
        /// </summary>
        private void DoCleanup()
        {
            if (!HasValidAvatar)
            {
                SetStatus("対象アバターが指定されていません。");
                return;
            }

            try
            {
                // 現状はアバターとプラットフォームのみを渡す想定。
                // 必要に応じて IPhysBoneCleanupHelper 側のシグネチャに合わせて引数を調整してください。
                _cleanupHelper.Cleanup(_avatarRoot, _platform, _currentSettings);
                SetStatus("Missing Root / 重複Collider のクリーンアップが完了しました。");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PhysBoneTool] Cleanup failed: {ex}");
                SetStatus("クリーンアップに失敗しました。詳細は Console を確認してください。");
            }
        }

        #endregion

        #region ユーティリティ

        /// <summary>
        /// ステータスメッセージを更新し、ウィンドウを再描画する
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        private void SetStatus(string message)
        {
            _statusMessage = message ?? string.Empty;
            Repaint();
        }

        #endregion
    }
}
