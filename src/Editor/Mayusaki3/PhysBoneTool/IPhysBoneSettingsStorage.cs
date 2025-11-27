namespace Mayusaki3.PhysBoneTool
{
    /// <summary>
    /// PhysBone 設定の永続化インターフェース
    /// </summary>
    public interface IPhysBoneSettingsStorage
    {
        void Save(string key, PhysBoneSettingsData data);
        bool TryLoad(string key, out PhysBoneSettingsData data);
        IReadOnlyList<string> ListKeys();
        bool Delete(string key);
    }
}
