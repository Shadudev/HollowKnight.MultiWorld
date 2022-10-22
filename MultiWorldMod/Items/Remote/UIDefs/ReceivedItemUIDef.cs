using ItemChanger;
using ItemChanger.UIDefs;

namespace MultiWorldMod.Items.Remote.UIDefs
{
    internal class ReceivedItemUIDef : MsgUIDef
    {
        public static UIDef Convert(UIDef orig, string from)
        {
            if (orig is MsgUIDef msgDef)
            {
                ReceivedItemUIDef uidef = new(msgDef, from);

                return uidef;
            }
            return orig;
        }

        public string from { get; set; }

        public ReceivedItemUIDef(MsgUIDef msgDef, string from)
        {
            this.from = from;

            name = msgDef?.name?.Clone();
            shopDesc = msgDef?.shopDesc?.Clone();
            sprite = msgDef?.sprite?.Clone();
            if (MultiWorldMod.RecentItemsInstalled)
                AddRecentItemsTagCallback();
        }

        private void AddRecentItemsTag(RecentItemsDisplay.ItemDisplayArgs args)
        {
            // Avoid races with picked & received items
            if (args.GiveEventArgs.Item.UIDef != this) return;

            if (MultiWorldMod.GS.RecentItemsPreferenceShowSender)
                args.DisplayMessage = $"{args.DisplayName}\nfrom {from}";

            RecentItemsDisplay.Events.ModifyDisplayItem -= AddRecentItemsTag;
        }

        private void AddRecentItemsTagCallback()
        {
            RecentItemsDisplay.Events.ModifyDisplayItem += AddRecentItemsTag;
        }

        public override void SendMessage(MessageType type, Action callback)
        {
            var tmp = name;
            if (MultiWorldMod.GS.RecentItemsPreferenceShowSender)
                name = new BoxedString($"{GetPostviewName()}\nfrom {from}");

            base.SendMessage(type, callback);
            name = tmp;
        }
    }
}
