using ItemChanger;
using ItemChanger.UIDefs;

namespace MultiWorldMod.Items.Remote.UIDefs
{
    internal class RemoteItemUIDef : MsgUIDef
    {
        private static readonly List<RemoteItemUIDef> remoteItemUIDefs = new();

        public static UIDef Create(AbstractItem item, int playerId)
        {
            if (item.UIDef is MsgUIDef msgUIDef)
                return new RemoteItemUIDef(msgUIDef, playerId);

            LogHelper.LogError($"RemoteItemUIDef.Create(item.UIDef.GetType().FullName = {item.UIDef.GetType().FullName}), is not MsgUIDef. item.name = {item.name}");
            return item.UIDef;
        }

        public int PlayerId { get; set; }

        public RemoteItemUIDef(MsgUIDef msgDef, int playerId)
        {
            PlayerId = playerId;

            if (msgDef != null && msgDef is SplitUIDef splitUIDef)
                name = splitUIDef.preview.Clone();
            else
                name = msgDef?.name?.Clone();
            shopDesc = msgDef?.shopDesc?.Clone();
            sprite = msgDef?.sprite?.Clone();
        }

        private static void AddRecentItemsTag(RecentItemsDisplay.ItemDisplayArgs args)
        {
            for (int i = 0; i < remoteItemUIDefs.Count; i++)
            {
                RemoteItemUIDef uidef = remoteItemUIDefs[i];
                // Avoid races with picked & received items
                if (args.GiveEventArgs.Item.UIDef != uidef) continue;

                switch (MultiWorldMod.GS.RecentItemsPreferenceForRemoteItems)
                {
                    case GlobalSettings.InfoPreference.OwnerOnly:
                        args.DisplayMessage = $"{MultiWorldMod.MWS.GetPlayerName(uidef.PlayerId)}'s\n" + 
                            args.DisplayName;
                        break;
                    case GlobalSettings.InfoPreference.Both:
                        args.DisplayMessage = $"{MultiWorldMod.MWS.GetPlayerName(uidef.PlayerId)}'s\n" +
                            args.DisplayName +
                            $"\nin {args.DisplaySource}";
                        break;
                }

                remoteItemUIDefs.RemoveAt(i);
            }
        }

        internal static void RegisterRecentItemsCallback()
        {
            remoteItemUIDefs.Clear();
            RecentItemsDisplay.Events.ModifyDisplayItem += AddRecentItemsTag;
        }

        internal static void UnregisterRecentItemsCallback()
        {
            remoteItemUIDefs.Clear();
            RecentItemsDisplay.Events.ModifyDisplayItem -= AddRecentItemsTag;
        }

        internal void AddRecentItemsCallback()
        {
            remoteItemUIDefs.Add(this);
        }

        public override string GetPreviewName()
        {
            return $"{MultiWorldMod.MWS.GetPlayerName(PlayerId)}'s {base.GetPreviewName()}";
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
