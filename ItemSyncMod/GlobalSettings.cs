namespace ItemSyncMod
{
    public class GlobalSettings
    {
        public string URL { get; set; } = "18.189.16.129";

#if (DEBUG)
        internal readonly int DefaultPort = 38283;
#else
        internal readonly int DefaultPort = 38282;
#endif

        public int ReadyID { get; set; }

        public string UserName { get; set; } = "WhoAmI";
        public bool SyncVanillaItems { get; set; } = true;
        public bool AdditionalFeaturesEnabled { get; set; } = false;

        public bool ReducePreload { get; set; } = false;
    }
}
