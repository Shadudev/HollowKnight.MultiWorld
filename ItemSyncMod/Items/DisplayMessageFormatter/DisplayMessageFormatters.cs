namespace ItemSyncMod.Items.DisplayMessageFormatter
{
    public interface IDisplayMessageFormatter
    {
        public string GetDisplayMessage(string displayName, string from, string displaySource, GlobalSettings.InfoPreference preference);
        public string GetCornerMessage(string postviewName, string from);
    }

    public class DefaultRemoteFormatter : IDisplayMessageFormatter
    {
        public string GetCornerMessage(string postviewName, string from)
        {
            return $"{postviewName}\nFrom {from}";
        }

        public string GetDisplayMessage(string displayName, string from, string displaySource, GlobalSettings.InfoPreference preference)
        {
            return ItemSyncMod.GS.RecentItemsPreference switch
            {
                GlobalSettings.InfoPreference.SenderOnly => $"{displayName}\nfrom {from}",
                GlobalSettings.InfoPreference.Both => $"{displayName}\nfrom {from}\nin {displaySource}",
                _ => displayName,
            };
        }
    }

    public class DoorUnlockedFormatter : IDisplayMessageFormatter
    {
        public string GetCornerMessage(string postviewName, string from)
        {
            return $"{postviewName}\nBy {from}";
        }

        public string GetDisplayMessage(string displayName, string from, string displaySource, GlobalSettings.InfoPreference preference)
        {
            return $"{displayName}\nBy {from}";
        }
    }
}
