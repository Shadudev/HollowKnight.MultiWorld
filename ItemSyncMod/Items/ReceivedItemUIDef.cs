using ItemChanger;
using ItemChanger.UIDefs;
using ItemSyncMod.Items.DisplayMessageFormatter;
using Newtonsoft.Json;

namespace ItemSyncMod.Items
{
    public class ReceivedItemUIDef : MsgUIDef
    {
        public static UIDef Convert(UIDef orig, string from, IDisplayMessageFormatter formatter)
        {
            if (orig is MsgUIDef msgDef)
                return new ReceivedItemUIDef(msgDef, from, formatter);

            return orig;
        }

        public ReceivedItemUIDef(MsgUIDef msgUIDef, string from, IDisplayMessageFormatter formatter)
        {
            From = from;
            Formatter = formatter;

            name = msgUIDef?.name?.Clone();
            shopDesc = msgUIDef?.shopDesc?.Clone();
            sprite = msgUIDef?.sprite?.Clone();

            if (ItemSyncMod.RecentItemsInstalled) AddRecentItemsTagCallback();
        }

        [JsonConstructor]
        internal ReceivedItemUIDef()
        {
            if (ItemSyncMod.RecentItemsInstalled) AddRecentItemsTagCallback();
        }

        [JsonProperty] internal string From;
        [JsonProperty] internal IDisplayMessageFormatter Formatter;

        private void AddRecentItemsTagCallback()
        {
            RecentItemsDisplay.Events.ModifyDisplayItem += this.AddRecentItemsTag;
        }

        public override void SendMessage(MessageType type, Action callback)
        {
            var tmp = name;
            switch (ItemSyncMod.GS.CornerMessagePreference)
            {
                case GlobalSettings.InfoPreference.Both:
                    name = new BoxedString(Formatter.GetCornerMessage(GetPostviewName(), From));
                    break;
            }
            base.SendMessage(type, callback);
            name = tmp;
        }
    }

    internal static class ReceivedItemUIDefExtensions
    {
        internal static void AddRecentItemsTag(this ReceivedItemUIDef self, RecentItemsDisplay.ItemDisplayArgs args)
        {
            try
            {
                switch (ItemSyncMod.GS.RecentItemsPreference)
                {
                    case GlobalSettings.InfoPreference.SenderOnly:
                    case GlobalSettings.InfoPreference.Both:
                        args.DisplayMessage = self.Formatter.GetDisplayMessage(args.DisplayName,
                            self.From, args.DisplaySource, ItemSyncMod.GS.RecentItemsPreference);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError($"Exception during formatter.GetDisplayMessage of item {self.name}," +
                    $" displayed as {args.DisplayName} from {self.From}, source {args.DisplaySource}\n{ex}");
            }

            RecentItemsDisplay.Events.ModifyDisplayItem -= self.AddRecentItemsTag;
        }
    }
}
