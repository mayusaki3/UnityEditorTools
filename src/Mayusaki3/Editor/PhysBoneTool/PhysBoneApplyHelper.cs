using UnityEngine;
namespace Mayusaki3.PhysBoneTool
{
    public static class PhysBoneApplyHelper
    {
        /// <summary>
        /// DTO から PhysBone / Collider コンポーネントへ設定を反映する
        /// </summary>
        public static void Apply(GameObject avatarRoot, PhysBoneSettingsData data)
        {
            // - TransformPathHelper で Transform を見つける
            // - 既存の VRCPhysBone / Collider を探す or 新規追加する
            // - DTO のパラメータを上書き
            // - Undo.RecordObject を適宜挟む
        }
    }
}
