using ItemChanger;
using ItemChanger.Tags;
using MultiWorldLib;
using Newtonsoft.Json;

namespace MultiWorldMod.Items.Remote.Tags
{
    internal class RemoteItemTag : Tag
    {
        public Item item;
        public bool Given = false;
        [JsonIgnore] private AbstractItem parent;

        [JsonIgnore] private bool collectedForEjection = false;

        public override void Load(object parent)
        {
            this.parent = (AbstractItem)parent;
            this.parent.ModifyItem += ModifyItemIntoRemotePlaceholder;
            this.parent.AfterGive += SetGivenTrue;
        }

        public override void Unload(object parent)
        {
            this.parent = (AbstractItem)parent;
            this.parent.ModifyItem -= ModifyItemIntoRemotePlaceholder;
            this.parent.AfterGive -= SetGivenTrue;
        }

        private void ModifyItemIntoRemotePlaceholder(GiveEventArgs args)
        {
            args.Item = new RemoteItem()
            {
                name = parent.name,
                tags = parent.tags,
                Item = item,
                UIDef = ItemManager.GetMatchingUIDef(parent, item.OwnerID),
            };
        }

        internal void CollectForEjection(AbstractPlacement placement, List<Item> itemsToSend)
        {
            itemsToSend.Add(item);
            collectedForEjection = true;
            parent.Give(placement, GetEjectGiveInfo());
            collectedForEjection = false;
        }

        internal bool IsCollectedForEjection() => collectedForEjection;

        private GiveInfo GetEjectGiveInfo()
        {
            return new GiveInfo()
            {
                Container = "MultiWorld",
                FlingType = FlingType.DirectDeposit,
                MessageType = MessageType.Corner,
                Transform = null,
                Callback = null
            };
        }

        internal bool CanBeGiven()
        {
            return !Given || IsItemSomewhatPersistent();
        }

        private void SetGivenTrue(ReadOnlyGiveEventArgs args)
        {
            Given = true;
            // This is partially broken due to persistent items
            RandomizerMod.RandomizerMod.RS.TrackerData.OnPlacementCleared(args.Placement.Name);
        }

        private bool IsItemSomewhatPersistent()
        {
            return parent.GetTag(out IPersistenceTag tag) && tag.Persistence != Persistence.Single;
        }
    }
}
