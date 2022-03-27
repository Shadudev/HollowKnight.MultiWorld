using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using MultiWorldLib;
using MultiWorldMod.Exceptions;
using MultiWorldMod.Items.Remote;
using MultiWorldMod.Items.Remote.Tags;
using MultiWorldMod.Items.Remote.UIDefs;
using MultiWorldMod.Randomizer;

namespace MultiWorldMod.Items
{
    class ItemManager
    {
        private static (string, string)[] s_cachedOrderedItemPlacements = null, s_newPlacements = null;
        private static AbstractPlacement s_remotePlacement = null;

        internal static void LoadShuffledItemsPlacementsInOrder(RandomizerMod.RC.RandoController rc)
        {
            s_cachedOrderedItemPlacements = OrderedItemPlacements.Get(rc);
        }

        internal static (string, string)[] GetShuffledItemsPlacementsInOrder()
        {
            return s_cachedOrderedItemPlacements;
        }

        internal static void StorePlacements((string item, string location)[] placements)
        {
            s_newPlacements = placements;
        }

        internal static void UnloadCache()
        {
            s_cachedOrderedItemPlacements = s_newPlacements = null;
            s_remotePlacement = null;
        }

        internal static UIDef GetMatchingUIDef(AbstractItem item, GiveEventArgs args, int playerId)
        {
            if (item is GenericAbstractItem)
                return GenericUIDef.Create(args.Item.name, playerId);
            else if (item.HasTag<RemoteNotchCostTag>())
                return RemoteCharmUIDef.Create(args.Item.name, playerId);
            else
                return RemoteItemUIDef.Create(args.Item.name, playerId);
        }

        /*
         * This function is responsible for applying the received placements.
         * It works as following:
         * 1. Iterating new placements:
         * 1.1 Get the item that is being replaced
         * 1.2 Get an object to represent new item (if local - try old location or remote placement, else create new instance)
         * 1.3 Copy some tags from old item to new item if necessary
         * 1.4 Remove old item and add new item to placement.items
         * 1.5 Place old item in remote placement
         * 2. Add a tag for all items in remote placement, since we expect to receive them later. Work by item ID
         * 3. Add remote placement to placements (other modified placements are pre-existing ones)
         * 
         * For tags (1.3), an item is in 1 of 3 states:
         * 1. Item is in original placement, tags not modified.
         * 2. Item was moved to remote placement (new item set in original placement), tags already copied.
         * 3. Item was moved to a new placement (locally placed), tags modified.
         * In 3, tags have to be backed up for potential later use
         * 
         * 1. If item was found in original placement as the new item, its tags have to be backed up.
         * 2. If item was found in remote placement as the new item, its tags have been used
         * 3. If item not found, take tags from backup (item was in state 1 before hand)
         */
        internal static void SetupPlacements()
        {
            (string _item, string _location)[] oldPlacementsByNames = GetShuffledItemsPlacementsInOrder();
            foreach ((var il, int index) in oldPlacementsByNames.Select((il, i) => (il,i)))
                LogHelper.LogDebug($"{index} {il._item}@{il._location}");

            AbstractPlacement remotePlacement = Finder.GetLocation(RemotePlacement.REMOTE_PLACEMENT_NAME).Wrap();
            Dictionary<string, Tag[]> itemsTagsBackup = new();

            LogHelper.LogDebug($"PlayerID: {MultiWorldMod.MWS.PlayerId}");
            foreach (((string newMWItem, string locationName), int index) in s_newPlacements.Select((v, index) => (v, index)))
            {
                string oldItem = oldPlacementsByNames[index]._item;
                LogHelper.LogDebug($"{newMWItem} ({oldItem}) -> {locationName}");

                (int playerId, string newItem) = LanguageStringManager.ExtractPlayerID(newMWItem);
                string newItemName = LanguageStringManager.GetItemName(newItem);
                AbstractItem newItemObj;

                // Remote item
                if (playerId != -1 && playerId != MultiWorldMod.MWS.PlayerId)
                {
                    newItemObj = CreateRemoteItemInstance(newItemName, newItem, playerId);
                }
                // Item is of own player, get existing AbstractItem
                else if (oldItem != newItem)
                {
                    LogHelper.LogDebug($"{newItem}@original?");
                    // Try to get from original location
                    if (!TryGetItemFromPlacement(newItemName, 
                        GetPlacementByLocationName(oldPlacementsByNames.Where(p => p._item == newItem).First()._location),
                        out newItemObj, pop: true))
                    {
                        LogHelper.LogDebug($"{newItem}@remote?");
                        // Item moved to remotePlacement earlier
                        if (!TryGetItemFromPlacement(newItemName, remotePlacement, out newItemObj, pop: true))
                            throw new UnexpectedStateException("New item not in original location nor remote");
                        newItemObj.RemoveTags<ReceivedItemTag>();

                        // Backup tags before new ones are copied
                    }

                    if (!itemsTagsBackup.ContainsKey(newItem))
                        itemsTagsBackup[newItem] = newItemObj.tags.ToArray();
                }

                // Placement unchanged, no need to do anything
                else continue;

                // Original item may be in original location
                AbstractPlacement oldItemPlacment = GetPlacementByLocationName(oldPlacementsByNames[index]._location);
                (string oldItemName, int oldItemId) = LanguageStringManager.ExtractItemID(oldItem);

                if (!TryGetItemFromPlacement(oldItemName, oldItemPlacment, out AbstractItem oldItemObj, pop: true))
                {
                    // Item not found, possibly moved to remote
                    TryGetItemFromPlacement(oldItemName, remotePlacement, out oldItemObj, pop: false);
                }
                else
                {
                    // Item found in original location, place in remote placement and backup tags
                    remotePlacement.Add(oldItemObj);
                    oldItemObj.AddTag<ReceivedItemTag>().Id = oldItemId;
                }

                if (oldItemObj == null)     // Item not in original nor remote placements, take tags from backup
                    CopySpecialTags(newItemObj, itemsTagsBackup[oldItem]);
                else                        // Found item, take tags from actual item
                    CopySpecialTags(newItemObj, oldItemObj.tags.ToArray());

                AbstractPlacement placement = GetPlacementByLocationName(locationName);
                placement.Add(newItemObj);
            }

            ItemChangerMod.AddPlacements(new AbstractPlacement[] { remotePlacement }, PlacementConflictResolution.Replace);
        }

        private static AbstractItem CreateRemoteItemInstance(string itemName, string itemId, int playerId)
        {
            AbstractItem remoteItemObj = Finder.GetItem(itemName) ?? 
                new GenericAbstractItem() { name = itemName, UIDef = GenericUIDef.Create(itemName, playerId) };

            RemoteItemTag tag = remoteItemObj.AddTag<RemoteItemTag>();
            tag.PlayerId = playerId;
            tag.ItemId = itemId;

            if (remoteItemObj is ItemChanger.Items.CharmItem)
                remoteItemObj.AddTag<RemoteNotchCostTag>().Cost = 0;

            return remoteItemObj;
        }

        private static AbstractPlacement GetPlacementByLocationName(string originalLocation)
        {
            return ItemChanger.Internal.Ref.Settings.GetPlacements()
                .Where(_p => _p.Name == originalLocation)?.First();
        }

        private static bool TryGetItemFromPlacement(string itemName, AbstractPlacement placement, out AbstractItem item, bool pop = false)
        {
            var itemQuery = placement.Items.Where(_i => _i.name == itemName);
            if (!itemQuery.Any())
            {
                item = null;
                return false;
            }

            item = itemQuery.First();
            if (pop)
                placement.Items.Remove(item);
            return true;
        }

        private static void CopySpecialTags(AbstractItem item, Tag[] tags)
        {
            bool costPreviewDisabled = false, itemPreviewDisabled = false;
            foreach (var tag in tags)
            {
                if (tag is CostTag costTag) item.GetOrAddTag<CostTag>().Cost = costTag.Cost;
                else if (tag is DisableCostPreviewTag) costPreviewDisabled = true;
                else if (tag is DisableItemPreviewTag) itemPreviewDisabled = true;
                if (tag is InteropTag interopTag) item.AddTags(new InteropTag[] { interopTag });
            }

            if (costPreviewDisabled) item.GetOrAddTag<DisableCostPreviewTag>();
            else item.RemoveTags<DisableCostPreviewTag>();

            if (itemPreviewDisabled) item.GetOrAddTag<DisableItemPreviewTag>();
            else item.RemoveTags<DisableItemPreviewTag>();
        }

        internal static bool TryGiveItem(string item, string from)
        {
            AbstractPlacement remotePlacement = GetRemotePlacement();

            (string itemName, int itemId) = LanguageStringManager.ExtractItemID(item);
            foreach (AbstractItem _item in remotePlacement.Items)
            {
                if (_item.name == itemName && _item.GetTag(out ReceivedItemTag tag) && tag.IdEquals(itemId) && tag.CanBeGiven())
                {
                    tag.GiveThisItem(remotePlacement, from);
                    return true;
                }
            }

            return false;
        }

        private static AbstractPlacement GetRemotePlacement()
        {
            if (s_remotePlacement == null)
            {
                s_remotePlacement = ItemChanger.Internal.Ref.Settings.GetPlacements()
                    .Where(p => p.Name == RemotePlacement.REMOTE_PLACEMENT_NAME).First();
            }
            return s_remotePlacement;
        }

        internal static Dictionary<AbstractItem, AbstractPlacement> GetRemoteItemsPlacements()
        {
            Dictionary<AbstractItem, AbstractPlacement> remoteItemsPlacements = new();
            foreach (AbstractPlacement placement in ItemChanger.Internal.Ref.Settings.GetPlacements())
                foreach (AbstractItem item in placement.Items)
                    if (item.HasTag<RemoteItemTag>())
                        remoteItemsPlacements[item] = placement;

            return remoteItemsPlacements;
        }
        internal static void UpdateOthersCharmNotchCosts(int playerId, int[] costs)
        {
            ItemChangerMod.Modules.Get<RemoteNotchCostUI>().AddPlayerNotchCosts(playerId, costs);

            foreach (AbstractItem remoteCharm in ItemChanger.Internal.Ref.Settings.GetItems().Where(item => 
                item.GetTag(out RemoteItemTag tag) && tag.PlayerId == playerId &&
                item.HasTag<RemoteNotchCostTag>()))
            {

                remoteCharm.GetTag<RemoteNotchCostTag>().Cost = costs[((ItemChanger.Items.CharmItem)remoteCharm).charmNum - 1];
            }
        }

        internal static void AddRemoteNotchCostUI()
        {
            ItemChangerMod.Modules.Add<RemoteNotchCostUI>();
        }
    }
}
