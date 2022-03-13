using ItemChanger;
using ItemChanger.UIDefs;

namespace ItemSyncMod.Items
{
    public class RemoteUIDef : MsgUIDef
    {
        public static UIDef Convert(UIDef orig, string from)
        {
            if (orig is MsgUIDef msgDef)
            {
                RemoteUIDef uidef = new(msgDef, from);
                
                return uidef;
            }
            return orig;
        }

        public RemoteUIDef(MsgUIDef msgUIDef, string from)
        {
            this.msgUIDef = msgUIDef;
            From = from;

            name = this.msgUIDef?.name?.Clone();
            shopDesc = this.msgUIDef?.shopDesc?.Clone();
            sprite = this.msgUIDef?.sprite?.Clone();
            if (ItemSyncMod.RecentItemsInstalled)
                AddRecentItemsTagCallback();
        }

        private MsgUIDef msgUIDef;
        private string From;

        private void AddRecentItemsTag(RecentItemsDisplay.ItemDisplayArgs args)
        {
            switch (ItemSyncMod.GS.RecentItemsPreference)
            {
                case GlobalSettings.InfoPreference.SenderOnly:
                    args.DisplayMessage = $"{args.DisplayName}\nfrom {From}";
                    break;
                case GlobalSettings.InfoPreference.Both:
                    args.DisplayMessage = $"{args.DisplayName}\nfrom {From}\nin {args.DisplaySource}";
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
                    name = new BoxedString($"{GetPostviewName()}\nfrom {From}");
                    break;
            }
            base.SendMessage(type, callback);
            name = tmp;
        }
    }
}
