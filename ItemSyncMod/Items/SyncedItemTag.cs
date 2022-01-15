using ItemChanger;
using Newtonsoft.Json;

namespace ItemSyncMod.Items
{
    internal class SyncedItemTag : Tag
    {
        public string ItemID;
        public bool Given = false;
        [JsonIgnore] private AbstractItem parent;

        public override void Load(object parent)
        {
            this.parent = (AbstractItem)parent;
            this.parent.AfterGive += AfterGiveItem;
        }

        public override void Unload(object parent)
        {
            this.parent = (AbstractItem)parent;
            this.parent.AfterGive -= AfterGiveItem;
        }

        public void AfterGiveItem(ReadOnlyGiveEventArgs args)
        {
            if (!ItemManager.ShouldItemBeIgnored(ItemID) && (!Given || IsItemSomewhatPersistent()))
            {
                Given = true;
                ItemSyncMod.Connection.SendItemToAll(ItemID);
            }
        }

        public void GiveThisItem()
        {
            Given = true;
            if (!parent.IsObtained())
                parent.Give(ItemManager.GetItemPlacement(ItemID), ItemManager.GetItemSyncStandardGiveInfo());
        }

        private bool IsItemSomewhatPersistent()
        {
            return parent.GetTag(out ItemChanger.Tags.IPersistenceTag tag) && tag.Persistence != Persistence.Single;
        }
    }
}
