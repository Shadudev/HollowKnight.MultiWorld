using ItemChanger;
using ItemChanger.UIDefs;

namespace MultiWorldMod.Items.Remote.UIDefs
{
    internal class RemoteItemUIDef : MsgUIDef
    {
        public static UIDef Create(AbstractItem item, int playerId)
        {
            return new RemoteItemUIDef((MsgUIDef)item.UIDef, playerId);
        }

        public int playerId { get; set; }

        public RemoteItemUIDef(MsgUIDef msgDef, int playerId)
        {
            this.playerId = playerId;

            if (msgDef != null && msgDef is SplitUIDef splitUIDef)
                name = splitUIDef.preview.Clone();
            else
                name = msgDef?.name?.Clone();
            shopDesc = msgDef?.shopDesc?.Clone();
            sprite = msgDef?.sprite?.Clone();
        }

        private void AddRecentItemsTag(RecentItemsDisplay.ItemDisplayArgs args)
        {
            // Avoid races with picked & received items
            if (args.GiveEventArgs.Item.UIDef != this) return;

            RecentItemsDisplay.Events.ModifyDisplayItem -= AddRecentItemsTag;

            switch (MultiWorldMod.GS.RecentItemsPreferenceForRemoteItems)
            {
                case GlobalSettings.InfoPreference.OwnerOnly:
                    args.DisplayMessage = $"{MultiWorldMod.MWS.GetPlayerName(playerId)}'s\n{args.DisplayName}";
                    break;
                case GlobalSettings.InfoPreference.Both:
                    args.DisplayMessage = $"{MultiWorldMod.MWS.GetPlayerName(playerId)}'s\n{args.DisplayName}\nin {args.DisplaySource}";
                    break;
            }
        }

        internal void AddRecentItemsCallback()
        {
            RecentItemsDisplay.Events.ModifyDisplayItem += AddRecentItemsTag;
        }

        public override string GetPreviewName()
        {
            return $"{MultiWorldMod.MWS.GetPlayerName(playerId)}'s {base.GetPreviewName()}";
        }

        public override void SendMessage(MessageType type, Action callback)
        {
            var tmp = name;
            switch (MultiWorldMod.GS.CornerMessagePreference)
            {
                case GlobalSettings.InfoPreference.Both:
                    name = new BoxedString(GetPreviewName());
                    break;
            }
            base.SendMessage(type, callback);
            name = tmp;
        }
    }
}
