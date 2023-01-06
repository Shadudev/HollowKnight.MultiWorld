using ItemChanger;
using ItemChanger.UIDefs;

namespace MultiWorldMod.Items.Remote.UIDefs
{
    internal class ReceivedItemUIDef : MsgUIDef
    {
        public static UIDef Convert(UIDef orig, string from)
        {
            if (orig is MsgUIDef msgDef && orig is not ReceivedItemUIDef)
                return new ReceivedItemUIDef(msgDef, from);
            
            return orig;
        }

        public string From { get; set; }

        public ReceivedItemUIDef(MsgUIDef msgDef, string from)
        {
            From = from;

            name = msgDef?.name?.Clone();
            shopDesc = msgDef?.shopDesc?.Clone();
            sprite = msgDef?.sprite?.Clone();
            if (MultiWorldMod.RecentItemsInstalled)
                AddRecentItemsTagCallback();
        }

        private void AddRecentItemsTagCallback()
        {
            RecentItemsDisplay.Events.ModifyDisplayItem += this.AddRecentItemsTag;
        }

        public override void SendMessage(MessageType type, Action callback)
        {
            var tmp = name;
            if (MultiWorldMod.GS.RecentItemsPreferenceShowSender)
                name = new BoxedString($"{GetPostviewName()}\nfrom {From}");

            base.SendMessage(type, callback);
            name = tmp;
        }
    }

    internal static class ReceivedItemUIDefExtensions
    {
        internal static void AddRecentItemsTag(this ReceivedItemUIDef self, RecentItemsDisplay.ItemDisplayArgs args)
        {
            // Avoid races with picked & received items
            if (args.GiveEventArgs.Item.UIDef != self) return;

            if (MultiWorldMod.GS.RecentItemsPreferenceShowSender)
                args.DisplayMessage = $"{args.DisplayName}\nfrom {self.From}";

            RecentItemsDisplay.Events.ModifyDisplayItem -= self.AddRecentItemsTag;
        }
    }
}
