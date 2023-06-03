using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Mayusaki3
{
    /// <Summary>
    /// UIヘルパークラス: enum属性拡張
    /// </Summary>
    public static partial class UIHelper
    {
        #region インナークラス

        #region 表示用の文字列を持つ属性(StringValueAttribute)

        /// <Summary>
        /// 表示用の文字列を持つ属性(StringValueAttribute)
        /// </Summary>
        /// <remarks>
        /// 次のようにenum値に表示文字列を設定します。
        /// 例. private enum MyEnum
        ///     {
        ///         [StringValue("表示文字列１")]
        ///         enumvalue1
        ///     }
        /// </remarks>
        [AttributeUsage(AttributeTargets.Field)]
        public class StringValueAttribute : Attribute
        {
            #region コンストラクタ

            /// <Summary>
            /// コンストラクタ
            /// </Summary>
            /// <param name="displayString">表示文字列</param>
            public StringValueAttribute(string displayString)
            {
                DisplayString = displayString;
            }

            #endregion

            #region プロパティ

            #region 表示文字列 ([R] DisplayString)

            public string DisplayString { get; private set; }

            #endregion
    
            #endregion
        }

        #endregion

        #endregion

        #region メソッド

        #region enum値のStringValue属性から表示用文字列を取得 (GetEnumDisplayText<T>)

        /// <Summary>
        /// enum値のStringValue属性から表示用文字列を取得します。
        /// </Summary>
        /// <typeparam name="T">StringValue属性を設定したenum型</typeparam>
        /// <param name="value">enum値</param>
        /// <returns>enum値の表示用文字列</returns>
        public static string GetEnumDisplayText<T>(T value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var stringValueAttribute = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            if (stringValueAttribute != null && stringValueAttribute.Length > 0)
            {
                return stringValueAttribute[0].DisplayString;
            }
            return value.ToString();
        }

        #endregion

        #region enum値のStringValue属性から表示用文字列リストを取得 (GetEnumDisplayTexts<T>) [private]

        /// <Summary>
        /// enum値のStringValue属性から表示用文字列リストを取得します。
        /// </Summary>
        /// <typeparam name="T">StringValue属性を設定したenum型</typeparam>
        /// <returns>enum値の表示用文字列リスト</returns>
        private static string[] GetEnumDisplayTexts<T>()
        {
            T[] values = (T[])System.Enum.GetValues(typeof(T));
            string[] displayTexts = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                displayTexts[i] = GetEnumDisplayText(values[i]);
            }
            return displayTexts;
        }

        #endregion

        #region インデックス値に対応するenum値を取得 (GetEnumValueAtIndex<T>) [private]

        /// <Summary>
        /// enum値のStringValue属性から表示用文字列リストを取得します。
        /// </Summary>
        /// <typeparam name="T">StringValue属性を設定したenum型</typeparam>
        /// <param name="index">インデックス値</param>
        /// <returns>対応するenum値</returns>
        private static T GetEnumValueAtIndex<T>(int index)
        {
            return (T)System.Enum.GetValues(typeof(T)).GetValue(index);
        }

        #endregion

        #region enum値をポップアップして選択 (EnumPopup<T>)

        /// <Summary>
        /// enum値をポップアップして選択します。
        /// </Summary>
        /// <typeparam name="T">StringValue属性を設定したenum型</typeparam>
        /// <param name="label">フィールドのラベル</param>
        /// <param name="selected">現在選択されているenum値</param>
        /// <returns>選択したenum値</returns>
        public static T EnumPopup<T>(string label, T selected)
        {
            string[] displayTexts = GetEnumDisplayTexts<T>();
            int selectedIndex = EditorGUILayout.Popup(label, GetSelectedIndex(selected, displayTexts), displayTexts);
            return GetEnumValueAtIndex<T>(selectedIndex);
        }

        #endregion

        #region 選択してenum値のインデックス値を取得 (GetSelectedIndex<T>) [private]

        /// <Summary>
        /// 選択してenum値のインデックス値を取得します。
        /// </Summary>
        /// <typeparam name="T">StringValue属性を設定したenum型</typeparam>
        /// <param name="selected">選択したenum値</param>
        /// <param name="displayTexts">表示用文字列リスト</param>
        /// <returns>インデックス値</returns>
        private static int GetSelectedIndex<T>(T selected, string[] displayTexts)
        {
            for (int i = 0; i < displayTexts.Length; i++)
            {
                if (GetEnumDisplayText(GetEnumValueAtIndex<T>(i)).Equals(GetEnumDisplayText(selected)))
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
