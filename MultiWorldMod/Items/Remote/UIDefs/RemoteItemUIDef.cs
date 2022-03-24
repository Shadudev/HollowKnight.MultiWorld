using ItemChanger;
using ItemChanger.UIDefs;

namespace MultiWorldMod.Items.Remote.UIDefs
{
    internal class RemoteItemUIDef : MsgUIDef
    {
        public static UIDef Create(string name, int playerId)
        {
            return new RemoteItemUIDef((MsgUIDef) Finder.GetItem(name).GetResolvedUIDef(), playerId);
        }

        private MsgUIDef msgDef;
        private int playerId;

        public RemoteItemUIDef(MsgUIDef msgDef, int playerId)
        {
            this.msgDef = msgDef;
            this.playerId = playerId;

            name = this.msgDef.name;
            shopDesc = this.msgDef.shopDesc;
            sprite = this.msgDef.sprite;
            if (MultiWorldMod.RecentItemsInstalled)
                AddRecentItemsTagCallback();
        }

        private void AddRecentItemsTag(RecentItemsDisplay.ItemDisplayArgs args)
        {
            switch (MultiWorldMod.GS.RecentItemsPreferenceForRemoteItems)
            {
                case GlobalSettings.InfoPreference.OwnerOnly:
                    args.DisplayMessage = $"{MultiWorldMod.MWS.GetPlayerName(playerId)}'s\n{args.DisplayName}";
                    break;
                case GlobalSettings.InfoPreference.Both:
                    args.DisplayMessage = $"{MultiWorldMod.MWS.GetPlayerName(playerId)}'s\n{base.GetPostviewName()}\nin {args.DisplaySource}";
                    break;
            }

            RecentItemsDisplay.Events.ModifyDisplayItem -= AddRecentItemsTag;
        }

        private void AddRecentItemsTagCallback()
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
                    name = new BoxedString($"{GetPreviewName()}");
                    break;
            }
            base.SendMessage(type, callback);
            name = tmp;
        }
    }
}
