using MultiWorldLib;
using RandomizerMod.Randomization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MultiWorldMod
{
    class ItemManager
    {
        private readonly static string[] replaceWithShinyItemPools = { "Rock", "Charm" };
        private readonly static ItemType[] replaceItemTypeWithTrinket = { ItemType.Geo, ItemType.Lifeblood, ItemType.Soul, ItemType.Lore };

        private static int additionalCharmsId = 41;

        internal static void LoadMissingItems((string, string)[] itemPlacements)
        {
            CreateMissingItemDefinitions(itemPlacements.
                Select(pair => (0, pair.Item1, pair.Item2)).ToArray());
        }

        internal static void ApplyRemoteItemDefModifications(ref ReqDef def)
        {
            if (replaceItemTypeWithTrinket.Contains(def.type))
                def.type = ItemType.Trinket;

            if (replaceWithShinyItemPools.Contains(def.pool))
                def.pool = "MW_" + def.pool;

            if (def.action == RandomizerMod.GiveItemActions.GiveAction.SpawnGeo)
                def.action = RandomizerMod.GiveItemActions.GiveAction.AddGeo;

            if (def.action == RandomizerMod.GiveItemActions.GiveAction.Charm)
            {
                /* TODO replace with
                 * def.charmNum = additionalCharmsId;
                 * def.notchCost = $"notchCost_{additionalCharmsId++}";
                 */
                def.charmNum = -1;
                def.notchCost = "notchCost_0"; // Remove notch cost icons from shop entries
            }
        }

        internal static void UpdatePlayerItems((int, string, string)[] items)
        {
            RemoveLocationMWPrefixes(items);
            CreateMissingItemDefinitions(items);
            UpdateItemsVariables(items);
        }

        private static void RemoveLocationMWPrefixes((int, string, string)[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                (_, items[i].Item3) = LanguageStringManager.ExtractPlayerID(items[i].Item3);
                (int playerId, string itemName) = LanguageStringManager.ExtractPlayerID(items[i].Item2);
                if (playerId == -1 || playerId == MultiWorldMod.Instance.Settings.MWPlayerId)
                    items[i].Item2 = itemName;
            }
        }

        private static void CreateMissingItemDefinitions((int, string, string)[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                (int playerId, string itemName) = LanguageStringManager.ExtractPlayerID(item.Item2);
                if (playerId != -1 && playerId != MultiWorldMod.Instance.Settings.MWPlayerId)
                {
                    ReqDef def = LogicManager.GetItemDef(itemName);
                    ReqDef copy = def;
                    copy.nameKey = LanguageStringManager.AddPlayerId(copy.nameKey, playerId);
                    ApplyRemoteItemDefModifications(ref copy);

                    string newNameKey = LogicManager.RemoveDuplicateSuffix(LanguageStringManager.GetItemName(item));
                    LogicManager.EditItemDef(newNameKey, copy);

                    string itemDisplayName = LanguageStringManager.GetLanguageString(def.nameKey, "UI");
                    string fullItemDisplayName = LanguageStringManager.AddItemOwnerNickname(playerId, itemDisplayName);
                    RandomizerMod.LanguageStringManager.SetString("UI", copy.nameKey, fullItemDisplayName);
                }
            }
        }

        private static void UpdateItemsVariables((int, string, string)[] items)
        {
            bool[] alreadyPlacedWell = new bool[items.Length];
            string[] itemsLocations = items.Select(orderedItemLocation => orderedItemLocation.Item3).ToArray();
            List<(string, string)> originalItems = RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements.Where(
                pair => itemsLocations.Contains(pair.Item2)).ToList();
            List<string> newPlacedItems = new List<string>();

            Dictionary<string, int> oldShopItemsCosts = RandomizerMod.RandomizerMod.Instance.Settings.ShopCosts.ToDictionary(
                kvp => kvp.Item1, kvp => kvp.Item2);
            List<string> newShopItems = new List<string>();

            foreach (var item in items)
            {
                (string oldItem, string location) = originalItems.Find(pair => pair.Item2 == item.Item3);

                if (!newPlacedItems.Contains(oldItem))
                    RandomizerMod.RandomizerMod.Instance.Settings.RemoveItem(oldItem);

                RandomizerMod.RandomizerMod.Instance.Settings.AddItemPlacement(item.Item2, location);
                newPlacedItems.Add(item.Item2);
                originalItems.Remove((oldItem, location));

                if (oldShopItemsCosts.ContainsKey(oldItem))
                {
                    if (!newShopItems.Contains(oldItem))
                        RandomizerMod.RandomizerMod.Instance.Settings.RemoveShopCost(oldItem);

                    (_, string itemName) = LanguageStringManager.ExtractPlayerID(item.Item2);
                    int cost = PostRandomizer.GetRandomizedShopCost(itemName);

                    RandomizerMod.RandomizerMod.Instance.Settings.AddShopCost(item.Item2, cost);
                    newShopItems.Add(item.Item2);
                }
            }
        }

        internal static void UpdateOthersCharmNotchCosts(int playerId, int[] costs)
        {
            foreach (var item in RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements.Where(item => LogicManager.GetItemDef(item.Item1).pool == "MW_Charm"))
            {
                try
                {
                    (int _playerId, string itemName) = LanguageStringManager.ExtractPlayerID(item.Item1);
                    if (_playerId == playerId)
                    {
                        ReqDef originalCharm = LogicManager.GetItemDef(itemName);
                        ReqDef mwCharm = LogicManager.GetItemDef(item.Item1);

                        int newCost = costs[originalCharm.charmNum - 1];

                        string mwItemDisplayName = LanguageStringManager.GetLanguageString(mwCharm.nameKey, "UI");
                        mwItemDisplayName = ReplaceCostInDisplayString(mwItemDisplayName, newCost);
                        mwItemDisplayName = LanguageStringManager.AddItemOwnerNickname(playerId, mwItemDisplayName);
                        RandomizerMod.LanguageStringManager.SetString("UI", mwCharm.nameKey, mwItemDisplayName);
                        LogHelper.Log($"Updated remote charm display {mwCharm.nameKey}->\"{mwItemDisplayName}\"");
                    }
                }
                catch (Exception)
                {
                    // Happens for white fragments
                }
            }
        }

        private static string ReplaceCostInDisplayString(string itemDisplayName, int newCost)
        {
            Regex notchCostSuffix = new Regex(@" \[(\d+)\]$");
            // When notch costs are not randomized locally
            if (!notchCostSuffix.IsMatch(itemDisplayName))
                return itemDisplayName + $" [{newCost}]";

            return notchCostSuffix.Replace(itemDisplayName, $" [{newCost}]");
        }
    }
}
