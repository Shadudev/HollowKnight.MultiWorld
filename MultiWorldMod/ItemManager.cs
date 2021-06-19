using MultiWorldLib;
using RandomizerMod.Randomization;
using System.Collections.Generic;
using System.Linq;

namespace MultiWorldMod
{
    class ItemManager
    {
        private readonly static string[] replaceWithShinyItemPools = { "Grub", "Rock" };
        private readonly static ItemType[] replaceItemTypeWithCharm = { ItemType.Geo, ItemType.Lifeblood, ItemType.Soul, ItemType.Lore };

        internal static void LoadItems()
        {
            UpdatePlayerItems(RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements.Select(pair => (0, pair.Item1, pair.Item2)).ToArray());
        }

        internal static void ApplyRemoteItemDefModifications(ref ReqDef def)
        {
            if (replaceItemTypeWithCharm.Contains(def.type))
                def.type = ItemType.Charm;

            if (replaceWithShinyItemPools.Contains(def.pool))
                def.pool = "MW_" + def.pool;

            if (def.action == RandomizerMod.GiveItemActions.GiveAction.SpawnGeo)
                def.action = RandomizerMod.GiveItemActions.GiveAction.AddGeo;

            if (def.action == RandomizerMod.GiveItemActions.GiveAction.Charm)
                def.charmNum = -1;
        }

        internal static void UpdatePlayerItems((int, string, string)[] items)
        {
            CreateMissingItemDefinitions(items);
            UpdateItemsVariables(items);
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

                    int cost = oldShopItemsCosts[oldItem];
                    RandomizerMod.RandomizerMod.Instance.Settings.AddShopCost(item.Item2, cost);
                    newShopItems.Add(item.Item2);
                }
            }
        }

        private static void CreateMissingItemDefinitions((int, string, string)[] items)
        {
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                (int playerId, string itemName) = LanguageStringManager.ExtractPlayerID(item);
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
    }
}
