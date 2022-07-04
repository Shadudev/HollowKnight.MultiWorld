using ItemChanger;
using MultiWorldLib;
using MultiWorldMod.Items.Remote.Tags;
using MultiWorldMod.Items.Remote.UIDefs;

namespace MultiWorldMod.Items.Remote
{
    internal class RemoteItem : AbstractItem
    {
        public Item Item;

        public override void GiveImmediate(GiveInfo info)
        {
            if (!GetTag<RemoteItemTag>().IsCollectedForEjection())
                MultiWorldMod.Connection.SendItem(Item);

            if (MultiWorldMod.RecentItemsInstalled)
                ((RemoteItemUIDef)GetResolvedUIDef()).AddRecentItemsCallback();
        }

        public override bool Redundant()
        {
            return false;
        }
    }
}
