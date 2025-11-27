namespace Mayusaki3.PhysBoneTool
{
    /// <summary>
    /// 1アバター分の PhysBone 設定全体
    /// </summary>
    [Serializable]
    public class PhysBoneSettingsData
    {
        public string avatarName;          // or GUID / PrefabPath など
        public string avatarGuid;          // 将来VRChatのIDや独自IDで差し替えてもよい
        public AvatarPlatform platform;    // PC / Mobile

        public List<PhysBoneEntry> physBones = new();
        public List<PhysBoneColliderEntry> colliders = new();
    }

    [Serializable]
    public class PhysBoneEntry
    {
        public string transformPath;   // Root からの Transform パス
        public string componentId;     // 同一Transformに複数ある場合の識別子（indexなど）

        // ここに Pull, Spring, Radius 等のパラメータを列挙
        public float pull;
        public float spring;
        // ...
    }

    [Serializable]
    public class PhysBoneColliderEntry
    {
        public string transformPath;   // Collider が載っている Transform パス
        public string componentId;     // 同一 Transform 上の collider index 等

        // ここに Radius, Height, Center, Shape 等のパラメータを列挙
        public float radius;
        // ...
    }
}
