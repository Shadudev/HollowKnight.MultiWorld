using ItemChanger;
using ItemChanger.Tags;
using MultiWorldMod.Items.Remote.UIDefs;
using Newtonsoft.Json;

namespace MultiWorldMod.Items.Remote
{
    internal class RemoteItem : AbstractItem
    {
        public string Item, TrueName;
        public int PlayerId;
        public string PreferredContainer;
        public bool Given = false;

        [JsonIgnore] private bool collectedForEjection = false;

        protected override void OnLoad()
        {
            AfterGive += SetGivenTrue;
            base.OnLoad();
        }

        protected override void OnUnload()
        {
            AfterGive -= SetGivenTrue;
            base.OnUnload();
        }

        public override string GetPreferredContainer() => PreferredContainer;

        public override bool GiveEarly(string containerType)
        {
            return containerType switch
            {
                Container.GrubJar => true,
                Container.GeoRock => true,
                Container.Totem => true,
                _ => false,
            };
        }

        public override void GiveImmediate(GiveInfo info)
        {
            if (!IsCollectedForEjection())
                MultiWorldMod.Connection.SendItem(Item, PlayerId);

            if (MultiWorldMod.RecentItemsInstalled)
                ((RemoteItemUIDef)GetResolvedUIDef()).AddRecentItemsCallback();
        }

        public override bool Redundant()
        {
            return false;
        }

        internal void CollectForForfeiting(AbstractPlacement placement, List<(string, int)> itemsToSend)
        {
            itemsToSend.Add((Item, PlayerId));
            collectedForEjection = true;
            Give(placement, GetEjectGiveInfo());
            collectedForEjection = false;
        }

        private static GiveInfo GetEjectGiveInfo()
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
            // RandomizerMod.RandomizerMod.RS.TrackerData.OnPlacementCleared(args.Placement.Name);
        }

        private bool IsItemSomewhatPersistent()
        {
            return GetTag(out IPersistenceTag tag) && tag.Persistence != Persistence.Single;
        }

        internal bool IsCollectedForEjection() => collectedForEjection;

        public static RemoteItem Wrap(string itemId, int playerId, AbstractItem item)
        {
            return new RemoteItem()
            {
                TrueName = item.name,
                name = $"{MultiWorldMod.MWS.GetPlayerName(playerId)}'s_{item.name}",
                UIDef = ItemManager.GetMatchingUIDef(item, playerId),
                Item = itemId,
                PlayerId = playerId,
                PreferredContainer = item.GetPreferredContainer()
            };
        }
    }
}
