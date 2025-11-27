using System;
using UnityEngine;
using UnityEditor;

namespace Mayusaki3
{
    /// <summary>
    /// UIヘルパークラス: enum属性拡張
    /// </summary>
    /// <remarks>
    /// - enum に StringValueAttribute を付与し、表示名をカスタマイズするためのヘルパー
    /// - Editor 拡張からポップアップ選択を簡易に実装する用途で使用する
    /// </remarks>
    public static partial class UIHelper
    {
        #region インナークラス

        #region 表示用の文字列を持つ属性(StringValueAttribute)

        /// <summary>
        /// 表示用の文字列を持つ属性(StringValueAttribute)
        /// </summary>
        /// <remarks>
        /// 次のように enum 値に表示文字列を設定します。
        ///
        /// 例:
        /// private enum MyEnum
        /// {
        ///     [StringValue("表示文字列１")]
        ///     EnumValue1,
        /// }
        /// </remarks>
        [AttributeUsage(AttributeTargets.Field)]
        public class StringValueAttribute : Attribute
        {
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="displayString">表示文字列</param>
            public StringValueAttribute(string displayString)
            {
                DisplayString = displayString;
            }

            /// <summary>
            /// 表示文字列
            /// </summary>
            public string DisplayString { get; private set; }
        }

        #endregion

        #endregion

        #region メソッド

        #region enum値のStringValue属性から表示用文字列を取得 (GetEnumDisplayText<T>)

        /// <summary>
        /// enum値の StringValue 属性から表示用文字列を取得します。
        /// </summary>
        /// <typeparam name="T">StringValue 属性を設定した enum 型</typeparam>
        /// <param name="value">enum値</param>
        /// <returns>enum値の表示用文字列（属性が無い場合は enum の ToString()）</returns>
        public static string GetEnumDisplayText<T>(T value) where T : Enum
        {
            var type = typeof(T);
            var name = value.ToString();
            var fieldInfo = type.GetField(name);
            if (fieldInfo == null)
            {
                // 想定外だが、安全のため ToString() でフォールバック
                return name;
            }

            var stringValueAttributes =
                (StringValueAttribute[])fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false);

            if (stringValueAttributes != null && stringValueAttributes.Length > 0)
            {
                return stringValueAttributes[0].DisplayString;
            }

            return name;
        }

        #endregion

        #region enum値のStringValue属性から表示用文字列リストを取得 (GetEnumDisplayTexts<T>) [private]

        /// <summary>
        /// enum値の StringValue 属性から表示用文字列リストを取得します。
        /// </summary>
        /// <typeparam name="T">StringValue 属性を設定した enum 型</typeparam>
        /// <returns>enum値の表示用文字列リスト</returns>
        private static string[] GetEnumDisplayTexts<T>() where T : Enum
        {
            var values = (T[])Enum.GetValues(typeof(T));
            var displayTexts = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                displayTexts[i] = GetEnumDisplayText(values[i]);
            }

            return displayTexts;
        }

        #endregion

        #region インデックス値に対応するenum値を取得 (GetEnumValueAtIndex<T>) [private]

        /// <summary>
        /// インデックス値に対応する enum 値を取得します。
        /// </summary>
        /// <typeparam name="T">StringValue 属性を設定した enum 型</typeparam>
        /// <param name="index">インデックス値</param>
        /// <returns>対応する enum 値</returns>
        private static T GetEnumValueAtIndex<T>(int index) where T : Enum
        {
            return (T)Enum.GetValues(typeof(T)).GetValue(index);
        }

        #endregion

        #region enum値をポップアップして選択 (EnumPopup<T>)

        /// <summary>
        /// enum値をポップアップで選択します。
        /// </summary>
        /// <typeparam name="T">StringValue 属性を設定した enum 型</typeparam>
        /// <param name="label">フィールドのラベル</param>
        /// <param name="selected">現在選択されている enum 値</param>
        /// <returns>選択された enum 値</returns>
        public static T EnumPopup<T>(string label, T selected) where T : Enum
        {
            var displayTexts = GetEnumDisplayTexts<T>();
            int selectedIndex = GetSelectedIndex(selected, displayTexts);
            int newIndex = EditorGUILayout.Popup(label, selectedIndex, displayTexts);
            return GetEnumValueAtIndex<T>(newIndex);
        }

        #endregion

        #region 選択してenum値のインデックス値を取得 (GetSelectedIndex<T>) [private]

        /// <summary>
        /// 選択された enum 値に対応するインデックス値を取得します。
        /// </summary>
        /// <typeparam name="T">StringValue 属性を設定した enum 型</typeparam>
        /// <param name="selected">選択された enum 値</param>
        /// <param name="displayTexts">表示用文字列リスト</param>
        /// <returns>インデックス値</returns>
        private static int GetSelectedIndex<T>(T selected, string[] displayTexts) where T : Enum
        {
            var selectedText = GetEnumDisplayText(selected);

            for (int i = 0; i < displayTexts.Length; i++)
            {
                if (displayTexts[i] == selectedText)
                {
                    return i;
                }
            }

            return 0;
        }

        #endregion

        #endregion
    }
}
