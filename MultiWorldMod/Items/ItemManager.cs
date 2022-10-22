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
        private static AbstractPlacement s_remotePlacement = null;
        private static Dictionary<IRandoItem, int> s_receivedItemIDs = new();
        private static readonly Random random = new();
        private static readonly string REMOTE_RANDOITEM_PREFIX = "Remote-";

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

        internal static void UnloadCache()
        {
            s_cachedOrderedItemPlacements = null;
            s_newPlacements = null;
            s_remotePlacement = null;
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

        internal static void RegisterRemoteLocation()
        {
            Finder.DefineCustomLocation(RemoteLocation.CreateDefault());
        }

        internal static void GetRemoteItem(GetItemEventArgs getItemEventArgs)
        {
            if (!getItemEventArgs.ItemName.StartsWith(REMOTE_RANDOITEM_PREFIX)) return;

            string mwItem = getItemEventArgs.ItemName.Substring(REMOTE_RANDOITEM_PREFIX.Length);
            (int playerId, string itemId) = LanguageStringManager.ExtractPlayerID(mwItem);
            string itemName = LanguageStringManager.GetItemName(itemId);

            getItemEventArgs.Current = CreateRemoteAbstractItem(itemName, itemId, playerId);
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
            RandoModLocation remoteLocation = CreateRemoteRandoLocation(rc.randomizer.lm);
            List<ItemPlacement> newItemsPlacements = new();
            List<RandoModItem> remotelyPlacedItems = new();
            
            Dictionary<string, (string _item, string _location)[]> originalPlacementsInOrder = GetShuffledItemsPlacementsInOrder();
            int remoteItems = 0;
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
                    {
                        newRandoModItem = CreateRemoteRandoItem(newMWItem);
                        remoteItems++;
                    }

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

                    // Check if the original item was placed elsewhere (in newItemPlacmeents)
                    if (!newItemsPlacements.Any(itemPlacement => itemPlacement.Item == originalItemPlacement.Item))
                    {
                        // If it wasn't, put it in a remote location - otherwise it was put in its updated location
                        remotelyPlacedItems.Add(originalItemPlacement.Item);
                        s_receivedItemIDs[originalItemPlacement.Item] = origItemId;
                    }
                }
            }

            foreach (RandoModItem randoModItem in remotelyPlacedItems)
                newItemsPlacements.Add(new ItemPlacement { Item = randoModItem, Location = remoteLocation });

            // Merge with item groups that were not included in the multiworld
            rc.ctx.itemPlacements.AddRange(newItemsPlacements);
        }

        internal static void RerollShopCosts()
        {
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

        private static RandoModItem CreateRemoteRandoItem(string remoteItem)
        {
            return new RandoModItem() { item = new RandomizerCore.LogicItems.EmptyItem(REMOTE_RANDOITEM_PREFIX + remoteItem) };
        }

        private static RandoModLocation CreateRemoteRandoLocation(LogicManager lm)
        {
            var location = new RemoteRandoLocation(lm);
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
                GeoCost geoCost = (GeoCost)multiCost.Costs.FirstOrDefault(cost => cost is GeoCost);
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

            AbstractPlacement remotePlacement = GetRemotePlacement();

            (string itemName, int itemId) = LanguageStringManager.ExtractItemID(dataReceivedEvent.Content);
            foreach (AbstractItem _item in remotePlacement.Items)
            {
                if (_item.name == itemName && _item.GetTag(out ReceivedItemTag tag) && tag.IdEquals(itemId) && tag.CanBeGiven())
                {
                    tag.GiveThisItem(remotePlacement, dataReceivedEvent.From);
                    dataReceivedEvent.Handled = true;
                    return;
                }
            }
        }

        private static AbstractPlacement GetRemotePlacement()
        {
            if (s_remotePlacement == null)
            {
                s_remotePlacement = ItemChanger.Internal.Ref.Settings.GetPlacements()
                    .Where(p => p.Name == RemotePlacement.REMOTE_PLACEMENT_NAME).FirstOrDefault();
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
