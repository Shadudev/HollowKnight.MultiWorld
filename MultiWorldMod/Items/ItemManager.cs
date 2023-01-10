using ItemChanger;
using ItemChanger.Tags;
using MultiWorldLib;
using MultiWorldMod.Items.Remote;
using MultiWorldMod.Items.Remote.Tags;
using MultiWorldMod.Items.Remote.UIDefs;
using MultiWorldMod.Randomizer;
using MultiWorldMod.Randomizer.SpecialClasses;
using RandomizerCore;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace MultiWorldMod.Items
{
    class ItemManager
    {
        private static Dictionary<string, (string, string)[]> s_cachedOrderedItemPlacements = null, s_newPlacements = null;
        private static Dictionary<IRandoItem, int> s_receivedItemIDs = new();
        private static Dictionary<string, string> s_remoteItems = new(), s_ownedItemsPlacements = null;        
        // Maps between a literal remote location and the location owner's (played) ID
        private static Dictionary<string, int> s_remoteLocations = new();
        private static Random random;

        internal static void LoadShuffledItemsPlacementsInOrder(RandoController rc)
        {
            var orderedItemPlacements = OrderedItemPlacements.Get(rc);
            s_cachedOrderedItemPlacements = new();

            int itemIncrementalId = 0;
            foreach (string group in orderedItemPlacements.Keys)
            {
                s_cachedOrderedItemPlacements[group] = orderedItemPlacements[group].Select(itemPlacement =>
                    (LanguageStringManager.AddItemId(itemPlacement.Item.Name, itemIncrementalId++), itemPlacement.Location.Name)).ToArray();
            }
        }

        internal static Dictionary<string, (string, string)[]> GetShuffledItemsPlacementsInOrder()
        {
            return s_cachedOrderedItemPlacements;
        }

        internal static void StorePlacements(Dictionary<string, (string, string)[]> placements)
        {
            s_newPlacements = placements;
        }

        internal static void StoreOwnedItemsRemotePlacements(Dictionary<string, string> ownedItemsPlacements)
        {
            s_ownedItemsPlacements = ownedItemsPlacements;
        }

        internal static void UnloadCache()
        {
            s_cachedOrderedItemPlacements = null;
            s_newPlacements = null;
            s_ownedItemsPlacements = null;
            s_remoteItems = new();
            s_remoteLocations = new();
            s_receivedItemIDs = new();
        }
        internal static void SubscribeEvents()
        {
            MultiWorldMod.Connection.OnDataReceived += TryGiveItem;
        }

        internal static void UnsubscribeEvents()
        {
            MultiWorldMod.Connection.OnDataReceived -= TryGiveItem;
        }

        internal static UIDef GetMatchingUIDef(AbstractItem item, int playerId)
        {
            if (item is ItemChanger.Items.CharmItem || item is ItemChanger.Items.WhiteFragmentItem)
                return RemoteCharmUIDef.Create(item, playerId);
        
            return RemoteItemUIDef.Create(item, playerId);
        }

        internal static void GetRemoteItem(GetItemEventArgs getItemEventArgs)
        {
            if (s_remoteItems.ContainsKey(getItemEventArgs.ItemName))
            {
                string mwItem = s_remoteItems[getItemEventArgs.ItemName];
                (int playerId, string itemId) = LanguageStringManager.ExtractPlayerID(mwItem);
                string itemName = LanguageStringManager.GetItemName(itemId);

                getItemEventArgs.Current = CreateRemoteAbstractItem(itemName, itemId, playerId);
            }
        }

        internal static void GetRemoteLocation(GetLocationEventArgs getLocationEventArgs)
        {
            string locationName = getLocationEventArgs.LocationName;
            if (s_remoteLocations.ContainsKey(locationName))
                getLocationEventArgs.Current = RemoteLocation.Create(locationName, s_remoteLocations[locationName]);
        }

        /*
         * This function is responsible for adding the correct metadata to ctx.itemPlacements
         * The general principles for the algorithm are:
         * keep itemPlacements locations in order
         * reuse items from itemPlacements
         * add placeholders for remote items and remote location
         * 
         * ReceivedItemTags are added later (refer to AddReceivedItemTags)
         */
        internal static void SetupPlacements(RandoController rc)
        {
            List<ItemPlacement> newItemsPlacements = new();
            List<ItemPlacement> remotelyPlacedItems = new();
            
            Dictionary<string, (string _item, string _location)[]> originalPlacementsInOrder = GetShuffledItemsPlacementsInOrder();
            foreach (string group in s_newPlacements.Keys)
            {
                foreach (((string newMWItem, string locationName), int index) in s_newPlacements[group].Select((v, index) => (v, index)))
                {
                    (int playerId, string newItem) = LanguageStringManager.ExtractPlayerID(newMWItem);
                    string newItemName = LanguageStringManager.GetItemName(newItem);
                    RandoModItem newRandoModItem;
                    RandoModLocation randoModLocation;

                    string originalItem = originalPlacementsInOrder[group][index]._item;
                    LogHelper.LogDebug($"{originalItem} -> {MultiWorldMod.MWS.GetPlayerName(playerId)}'s " +
                        $"{newItem} @ {originalPlacementsInOrder[group][index]._location}");

                    // Remote item
                    if (playerId != -1 && playerId != MultiWorldMod.MWS.PlayerId)
                        newRandoModItem = CreateRemoteRandoItem(MultiWorldMod.MWS.GetPlayerName(playerId), newItem, newMWItem);

                    // Item is of own player, get its RandoModItem
                    else
                    {
                        string originalLocationName = originalPlacementsInOrder[group].First(p => p._item == newItem)._location;
                        ItemPlacement newItemPlacement = GetItemPlacement(rc.ctx.itemPlacements,
                            newItemName, originalLocationName);
                        newRandoModItem = newItemPlacement.Item;
                    }

                    // Get RandoModLocation and the RandoModItem that's being replaced
                    (string origItemName, int origItemId) = LanguageStringManager.ExtractItemID(originalItem);
                    ItemPlacement originalItemPlacement = GetItemPlacement(rc.ctx.itemPlacements, 
                                                                           origItemName, locationName);
                    randoModLocation = originalItemPlacement.Location;

                    newItemsPlacements.Add(new ItemPlacement { Item = newRandoModItem, Location = randoModLocation });
                    // A replaced item will never be locally placed later (breaks main principle for multiworld)
                    rc.ctx.itemPlacements.Remove(originalItemPlacement);

                    // Check if the original item was placed elsewhere in the local world
                    string mwLocation = s_ownedItemsPlacements[originalItem];
                    (int locationOwnerId, string remoteLocationName) = LanguageStringManager.ExtractPlayerID(mwLocation);
                    // If it wasn't, put it in a remote location
                    if (locationOwnerId != MultiWorldMod.MWS.PlayerId && locationOwnerId != -1)
                    {
                        string fullLocationName = $"{MultiWorldMod.MWS.GetPlayerName(locationOwnerId)}'s " +
                                $"{remoteLocationName}";

                        RandoModLocation remoteRandoLocation = CreateRemoteRandoLocation(rc.randomizer.lm, fullLocationName, locationOwnerId);

                        remotelyPlacedItems.Add(new ItemPlacement
                        {
                            Item = originalItemPlacement.Item,
                            Location = remoteRandoLocation
                        });
                        s_receivedItemIDs[originalItemPlacement.Item] = origItemId;

                    }
                }
            }

            // Merge with item groups that were not included in the multiworld
            rc.ctx.itemPlacements.AddRange(newItemsPlacements);
            rc.ctx.itemPlacements.AddRange(remotelyPlacedItems);
            rc.args.ctx.itemPlacements = rc.ctx.itemPlacements.ToList();
        }

        internal static void RerollShopCosts()
        {
            random = new Random(MultiWorldMod.Controller.randoController.gs.Seed + 152);
            string[] shops = new[]
            {
                LocationNames.Sly, LocationNames.Sly_Key, LocationNames.Iselda, LocationNames.Salubra, LocationNames.Leg_Eater,
            };

            foreach (string shop in shops)
            {
                AbstractPlacement placement = GetPlacementByLocationName(shop);
                foreach (AbstractItem item in placement.Items)
                    RerollGeoCost(item);
            }
        }

        // Gets a RandoModItem by item and location names from itemPlacements
        private static ItemPlacement GetItemPlacement(List<ItemPlacement> itemPlacements, string itemName, string locationName)
        {
            return itemPlacements.First(itemPlacement => itemPlacement.Item.Name == itemName && 
                                                        itemPlacement.Location.Name == locationName);
        }

        private static RandoModItem CreateRemoteRandoItem(string playerName, string itemName, string mwItem)
        {
            string fullItemName = $"{playerName}'s {itemName}";
            s_remoteItems[fullItemName] = mwItem;
            return new RandoModItem() { item = new RandomizerCore.LogicItems.EmptyItem(fullItemName) };
        }

        private static RandoModLocation CreateRemoteRandoLocation(LogicManager lm, string locationName, int locationOwnerId)
        {
            var location = RemoteRandoLocation.Create(lm, locationName);
            s_remoteLocations[locationName] = locationOwnerId;
            location.info.customAddToPlacement = SetupRemotelyPlacedItem;
            return location;
        }

        private static void SetupRemotelyPlacedItem(ICFactory icfactory, RandoPlacement randoPlacement, 
            AbstractPlacement placement, AbstractItem item)
        {
            item.AddTag<ReceivedItemTag>().Id = s_receivedItemIDs[randoPlacement.Item];
            item.GetOrAddTag<CompletionWeightTag>().Weight = 0;
            placement.Add(item);
        }

        private static AbstractItem CreateRemoteAbstractItem(string itemName, string itemId, int playerId)
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
                .First(_p => _p.Name == originalLocation);
        }

        private static void RerollGeoCost(AbstractItem item)
        {
            CostTag costTag = item.GetTag<CostTag>();
            if (costTag.Cost is GeoCost)
                costTag.Cost = Cost.NewGeoCost(GetRandomShopCost(item));
            else if (costTag.Cost is MultiCost multiCost)
            {
                GeoCost geoCost = (GeoCost)multiCost.FirstOrDefault(cost => cost is GeoCost);
                int newGeoCost = GetRandomShopCost(item);
                Cost otherCosts = geoCost == null ? multiCost : multiCost.Where(
                    cost => cost is not GeoCost).Aggregate((cost1, cost2) => cost1 + cost2);

                costTag.Cost = otherCosts + Cost.NewGeoCost(newGeoCost);
            }
        }

        // Based on RandomizerMod.RC.BuiltinRequests.ApplyShopDefs.GetShopCost
        private static int GetRandomShopCost(AbstractItem item)
        {
            double pow = 1.2; // setting?
            // This is due to a RemoteItem patch
            string itemTrueName = item is RemoteItem remoteItem ? remoteItem.TrueName : item.name;
            ItemDef itemDef = Data.GetItemDef(itemTrueName);

            int cap = itemDef is not null ? itemDef.PriceCap : 500;
            if (cap <= 100) return cap;
            return random.PowerLaw(pow, 100, cap).ClampToMultipleOf(5);
        }

        internal static void TryGiveItem(DataReceivedEvent dataReceivedEvent)
        {
            if (dataReceivedEvent.Label != Consts.MULTIWORLD_ITEM_MESSAGE_LABEL) return;

            (string itemName, int itemId) = LanguageStringManager.ExtractItemID(dataReceivedEvent.Content);
            foreach (RemotePlacement placement in ItemChanger.Internal.Ref.Settings.GetPlacements().Where(
                placement => placement is RemotePlacement))
            {
                foreach (AbstractItem item in placement.Items)
                {
                    if (item.name == itemName && item.GetTag(out ReceivedItemTag tag) && tag.IdEquals(itemId) && tag.CanBeGiven())
                    {
                        tag.GiveThisItem(placement, dataReceivedEvent.From);
                        dataReceivedEvent.Handled = true;
                        return;
                    }
                }
            }
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
