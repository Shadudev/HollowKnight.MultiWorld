using ItemChanger;
using MultiWorldMod.Items.Remote.Tags;

namespace MultiWorldMod.Items.Remote
{
    internal class RemoteItem : AbstractItem
    {
        public string Item;
        public int PlayerId;

        public override void GiveImmediate(GiveInfo info)
        {
            if (!GetTag<RemoteItemTag>().IsCollectedForEjection())
                MultiWorldMod.Connection.SendItem(Item, PlayerId);
        }
    }
}
