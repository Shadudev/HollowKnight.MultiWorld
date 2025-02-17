﻿namespace ItemSyncMod
{
    public class GlobalSettings
    {
        public string URL { get; set; } = MultiWorldLib.Consts.PUBLIC_SERVER_URL;

        public string UserName { get; set; } = "WhoAmI";
        public bool SyncVanillaItems { get; set; } = true;
        public bool SyncSimpleKeysUsages { get; internal set; } = false;
                
        public enum InfoPreference
        {
            Both = 0,
            SenderOnly,
            AreaNameOnly,
            ItemOnly
        }
        public InfoPreference RecentItemsPreference { get; set; } = InfoPreference.Both;
        public InfoPreference CornerMessagePreference { get; internal set; }
    }
}
