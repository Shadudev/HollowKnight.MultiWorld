using ItemChanger;
using ItemChanger.Tags;
using MultiWorldLib;
using MultiWorldMod.Exceptions;
using MultiWorldMod.Items.Remote;
using MultiWorldMod.Items.Remote.Tags;
using MultiWorldMod.Items.Remote.UIDefs;
using MultiWorldMod.Randomizer;
using RandomizerCore.Extensions;
using RandomizerMod.RandomizerData;

namespace MultiWorldMod.Items
{
    class ItemManager
    {
        private static (string, string)[] s_cachedOrderedItemPlacements = null, s_newPlacements = null;
        private static AbstractPlacement s_remotePlacement = null;
        private static readonly Random random = new();
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

        internal static UIDef GetMatchingUIDef(AbstractItem item, int playerId)
        {
            if (item is ItemChanger.Items.CharmItem)
                return RemoteCharmUIDef.Create(item, playerId);
        
            return RemoteItemUIDef.Create(item, playerId);
        }

        /*
         * This function is responsible for applying the received placements.
         * It works as following:
         * 1. Iterating new placements:
         * 1.1 Get the item that is being replaced, and its original placement
         * 1.2 Get an object to represent new item (if local - try old location or remote placement, else create new instance)
         * 1.3 Copy some tags from old item to new item if necessary
         * 1.4 Add new item to placement.items
         * 1.5 If the item's original placement is different than the new placement, move old item to remote placement
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
            (string _item, string _location)[] oldPlacementsInOrder = GetShuffledItemsPlacementsInOrder();

            AbstractPlacement remotePlacement = Finder.GetLocation(RemotePlacement.REMOTE_PLACEMENT_NAME).Wrap();
            Dictionary<string, Tag[]> itemsTagsBackup = new();

            foreach (((string newMWItem, string locationName), int index) in s_newPlacements.Select((v, index) => (v, index)))
            {
                string oldItem = oldPlacementsInOrder[index]._item;

                (int playerId, string newItem) = LanguageStringManager.ExtractPlayerID(newMWItem);
                string newItemName = LanguageStringManager.GetItemName(newItem);
                AbstractItem newItemObj;

                LogHelper.LogDebug($"{oldItem} -> {MultiWorldMod.MWS.GetPlayerName(playerId)}'s " +
                    $"{newItem} @ {oldPlacementsInOrder[index]._location}");
                // Remote item

                if (playerId != -1 && (playerId != MultiWorldMod.MWS.PlayerId))
                    newItemObj = CreateRemoteItemInstance(newItemName, newItem, playerId);

                // Placement unchanged, no need to do anything
                else if (oldItem == newItem) continue;

                else
                {
                    // Item is of own player, get existing AbstractItem
                    // Try to get from original location
                    if (!TryGetItemFromPlacement(newItemName,
                        GetPlacementByLocationName(oldPlacementsInOrder.Where(p => p._item == newItem).First()._location),
                        out newItemObj, pop: ShouldRemoveItemFromOriginalLocation(newItemName, index, oldPlacementsInOrder)))
                    {
                        // Item moved to remotePlacement earlier
                        if (!TryGetItemFromPlacement(newItemName, remotePlacement, out newItemObj, pop: true))
                            throw new UnexpectedStateException("New item not in original location nor remote");
                        newItemObj.RemoveTags<ReceivedItemTag>();
                    }

                    if (!itemsTagsBackup.ContainsKey(newItem))
                    {
                        itemsTagsBackup[newItem] = newItemObj.tags.ToArray();
                        // Avoid keeping items with irrelevant costs
                        newItemObj.RemoveTags<CostTag>();
                    }
                }

                // Original item may be in original location
                AbstractPlacement oldItemPlacment = GetPlacementByLocationName(oldPlacementsInOrder[index]._location);
                (string oldItemName, int oldItemId) = LanguageStringManager.ExtractItemID(oldItem);

                if (!TryGetItemFromPlacement(oldItemName, oldItemPlacment, out AbstractItem oldItemObj, pop: true))
                {
                    // Item not found, possibly moved to remote
                    TryGetItemFromPlacement(oldItemName, remotePlacement, out oldItemObj, pop: false);
                }
                else
                {
                    // Item found in original location, move to remote placement
                    remotePlacement.Add(oldItemObj);
                    oldItemObj.GetOrAddTag<ReceivedItemTag>().Id = oldItemId;
                }

                // Item not in original nor remote placements (was placed in new location), take tags from backup
                if (itemsTagsBackup.ContainsKey(oldItem))
                    CopySpecialTags(newItemObj, itemsTagsBackup[oldItem]);
                else
                    // Found item in original placement, take tags from actual item
                    CopySpecialTags(newItemObj, oldItemObj.tags.ToArray());

                AbstractPlacement placement = GetPlacementByLocationName(locationName);
                placement.Add(newItemObj);
            }

            ItemChangerMod.AddPlacements(new AbstractPlacement[] { remotePlacement }, PlacementConflictResolution.Replace);

            ZeroCompletionWeights(remotePlacement);
        }

        // This is to avoid cases where an item is replacing a different item in the same shop
        // Next, the original item would be replaced by another.
        // If we always pop from original locations, we will end up with 0 instances of an item in such a case
        private static bool ShouldRemoveItemFromOriginalLocation(string newItem, int newLocationIndex, (string _item, string _location)[] originalPlacementsByNames)
        {
            string newItemOriginalLocation = originalPlacementsByNames[LanguageStringManager.ExtractItemID(newItem).id]._location;
            string newLocation = originalPlacementsByNames[newLocationIndex]._location;

            return newItemOriginalLocation != newLocation;
        }

        private static void ZeroCompletionWeights(AbstractPlacement placement)
        {
            placement.Items.ForEach(item => item.GetOrAddTag<CompletionWeightTag>().Weight = 0);
        }

        private static AbstractItem CreateRemoteItemInstance(string itemName, string itemId, int playerId)
        {
            AbstractItem baseItemObj = Finder.GetItem(itemName) ?? 
                new GenericAbstractItem() { name = itemName, UIDef = GenericUIDef.Create(itemName, playerId) };
            RemoteItem remoteItem = RemoteItem.Wrap(itemId, playerId, baseItemObj);

            if (baseItemObj is ItemChanger.Items.CharmItem baseCharmItem)
            {
                RemoteNotchCostTag tag = remoteItem.AddTag<RemoteNotchCostTag>();
                tag.Cost = 0;
                tag.CharmNum = baseCharmItem.charmNum;
            }

            return remoteItem;
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
            if (tags == null) return;

            bool costPreviewDisabled = false, itemPreviewDisabled = false;
            foreach (var tag in tags)
            {
                if (tag is CostTag costTag)
                {
                    CostTag newCostTag = item.GetOrAddTag<CostTag>();
                    newCostTag.Cost = costTag.Cost;
                    LogHelper.LogDebug($"Copied cost {costTag.Cost}");

                    RerollGeoCost(item, newCostTag);
                }
                else if (tag is DisableCostPreviewTag) costPreviewDisabled = true;
                else if (tag is DisableItemPreviewTag) itemPreviewDisabled = true;
                if (tag is InteropTag interopTag) item.AddTags(new InteropTag[] { interopTag });
            }

            if (costPreviewDisabled) item.GetOrAddTag<DisableCostPreviewTag>();
            else item.RemoveTags<DisableCostPreviewTag>();

            if (itemPreviewDisabled) item.GetOrAddTag<DisableItemPreviewTag>();
            else item.RemoveTags<DisableItemPreviewTag>();
        }

        private static void RerollGeoCost(AbstractItem item, CostTag newCostTag)
        {
            Cost geoCost = null;
            if (newCostTag.Cost is GeoCost)
                newCostTag.Cost = Cost.NewGeoCost(GetRandomShopCost(item));
            else if (newCostTag.Cost is MultiCost multiCost)
            {
                geoCost = multiCost.Costs.FirstOrDefault(cost => cost is GeoCost);
                if (geoCost != null)
                {
                    multiCost.Costs.Remove(geoCost);
                    multiCost.Costs.Add(Cost.NewGeoCost(GetRandomShopCost(item)));
                }
            }
        }

        // Based on RandomizerMod.RC.BuiltinRequests.ApplyShopDefs.GetShopCost
        private static int GetRandomShopCost(AbstractItem item)
        {
            double pow = 1.2; // setting?
            ItemDef itemDef = Data.GetItemDef(item.name);

            int cap = itemDef is not null ? itemDef.PriceCap : 500;
            if (cap <= 100) return cap;
            return random.PowerLaw(pow, 100, cap).ClampToMultipleOf(5);
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
                    if (item is RemoteItem)
                        remoteItemsPlacements[item] = placement;

            return remoteItemsPlacements;
        }
        internal static void UpdateOthersCharmNotchCosts(int playerId, Dictionary<int, int> costs)
        {
            ItemChangerMod.Modules.GetOrAdd<RemoteNotchCostUI>().AddPlayerNotchCosts(playerId, costs);

            foreach (AbstractItem remoteCharm in ItemChanger.Internal.Ref.Settings.GetItems().Where(item => 
                item is RemoteItem remoteItem && remoteItem.PlayerId == playerId &&
                item.HasTag<RemoteNotchCostTag>()))
            {
                RemoteNotchCostTag tag = remoteCharm.GetTag<RemoteNotchCostTag>();
                tag.Cost = costs[tag.CharmNum];
            }
        }

        internal static void AddRemoteNotchCostUI()
        {
            ItemChangerMod.Modules.Add<RemoteNotchCostUI>();
        }
    }
}
