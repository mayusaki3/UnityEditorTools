using UnityEngine;
namespace Mayusaki3.PhysBoneTool
{
    public static class PhysBoneCleanupHelper
    {
        /// <summary>
        /// Missing Root を持つ PhysBone を削除
        /// </summary>
        public static int RemoveMissingRoots(GameObject avatarRoot)
        {
            // - VRCPhysBone を走査し、root や bone の参照が Missing なものを削除
            // - 削除数を返す
            return 0;
        }

        /// <summary>
        /// 重複している Collider を削除
        /// </summary>
        public static int RemoveDuplicateColliders(GameObject avatarRoot, float epsilon = 1e-4f)
        {
            // - 同一 Transform 上の VRCPhysBoneCollider を比較
            // - radius / center / shape などがほぼ同じものを「重複」とみなし削除
            // - 削除数を返す
            return 0;
        }
    }
}
