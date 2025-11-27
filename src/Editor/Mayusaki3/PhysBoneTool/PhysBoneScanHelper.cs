namespace Mayusaki3.PhysBoneTool
{
    public static class PhysBoneScanHelper
    {
        /// <summary>
        /// アバターのルートから PhysBone 設定を走査し、DTO に詰める
        /// </summary>
        public static PhysBoneSettingsData Scan(GameObject avatarRoot, AvatarPlatform platform)
        {
            // - VRCPhysBone / VRCPhysBoneCollider を GetComponentsInChildren で取得
            // - TransformPathHelper を使って transformPath を設定
            // - 各パラメータを PhysBoneEntry / PhysBoneColliderEntry にコピー
            // をここで行う想定
            return new PhysBoneSettingsData();
        }
    }
}
