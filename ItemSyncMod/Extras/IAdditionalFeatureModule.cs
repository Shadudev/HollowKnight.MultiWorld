namespace ItemSyncMod.Extras
{
    public interface IAdditionalFeatureModule
    {
        public List<(string, string)> GetPreloadNames();
        public void SavePreloads(Dictionary<string, Dictionary<string, UnityEngine.GameObject>> preloads);

        public void Hook();
        public void Unhook();
    }
}
