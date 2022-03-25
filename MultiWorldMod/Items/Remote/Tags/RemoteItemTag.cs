using ItemChanger;
using ItemChanger.Tags;
using MultiWorldMod.Items.Remote.UIDefs;
using Newtonsoft.Json;

namespace MultiWorldMod.Items.Remote.Tags
{
    internal class RemoteItemTag : Tag
    {
        public string ItemId;
        public int PlayerId;
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
                Item = ItemId,
                PlayerId = PlayerId,
                UIDef = ItemManager.GetMatchingUIDef(parent, args, PlayerId),
            };
            
        }

        internal void CollectForEjection(AbstractPlacement placement, List<(int, string)> itemsToSend)
        {
            itemsToSend.Add((PlayerId, ItemId));
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
        }

        private bool IsItemSomewhatPersistent()
        {
            return parent.GetTag(out IPersistenceTag tag) && tag.Persistence != Persistence.Single;
        }
    }
}
