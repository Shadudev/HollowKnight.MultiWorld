using ItemChanger;
using ItemChanger.UIDefs;
using ItemSyncMod.Items.DisplayMessageFormatter;

namespace ItemSyncMod.Items
{
    public class RemoteUIDef : MsgUIDef
    {
        public static UIDef Convert(UIDef orig, string from, IDisplayMessageFormatter formatter)
        {
            if (orig is MsgUIDef msgDef)
            {
                RemoteUIDef uidef = new(msgDef, from, formatter);
                
                return uidef;
            }
            return orig;
        }

        public RemoteUIDef(MsgUIDef msgUIDef, string from, IDisplayMessageFormatter formatter)
        {
            this.msgUIDef = msgUIDef;
            From = from;
            Formatter = formatter;

            name = this.msgUIDef?.name?.Clone();
            shopDesc = this.msgUIDef?.shopDesc?.Clone();
            sprite = this.msgUIDef?.sprite?.Clone();
            if (ItemSyncMod.RecentItemsInstalled)
                AddRecentItemsTagCallback();
        }

        private MsgUIDef msgUIDef;
        private string From;
        private IDisplayMessageFormatter Formatter;

        private void AddRecentItemsTag(RecentItemsDisplay.ItemDisplayArgs args)
        {
            switch (ItemSyncMod.GS.RecentItemsPreference)
            {
                case GlobalSettings.InfoPreference.SenderOnly:
                case GlobalSettings.InfoPreference.Both:
                    args.DisplayMessage = Formatter.GetDisplayMessage(args.DisplayName, 
                        From, args.DisplaySource, ItemSyncMod.GS.RecentItemsPreference);
                    break;
            }

            RecentItemsDisplay.Events.ModifyDisplayItem -= AddRecentItemsTag;
        }

        private void AddRecentItemsTagCallback()
        {
            RecentItemsDisplay.Events.ModifyDisplayItem += AddRecentItemsTag;
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
}
