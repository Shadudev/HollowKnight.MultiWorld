namespace MultiWorldMod
{
    public class GlobalSettings
	{
        public string URL { get; set; } = MultiWorldLib.Consts.PUBLIC_SERVER_URL;

        public int ReadyID { get; set; }

        public string UserName { get; set; } = "WhoAmI";

        public enum InfoPreference
        {
            Both = 0,
            OwnerOnly,
            AreaNameOnly,
            ItemOnly
        }
        public InfoPreference RecentItemsPreferenceForRemoteItems { get; set; } = InfoPreference.Both;
        public InfoPreference CornerMessagePreference { get; internal set; }
        public bool RecentItemsPreferenceShowSender { get; internal set; } = true;
    }
}
