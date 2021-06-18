using MultiWorldLib;
using RandomizerMod.Randomization;
using System.Collections.Generic;
using System.Linq;

namespace MultiWorldMod
{
    class ItemManager
    {
        internal static void UpdatePlayerItems((int, string, string)[] items)
        {
            foreach (var item in RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements)
                LogHelper.Log($"Current {item.Item1}->{item.Item2}");
            foreach (var item in items)
                LogHelper.Log($"Received {item.Item2}->{item.Item3}");
            LogHelper.Log("Now we got shop costs: " + RandomizerMod.RandomizerMod.Instance.Settings.ShopCosts.Length);
            CreateMissingItemDefinitions(items);
            UpdateItemsVariables(items);
            foreach (var item in RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements)
                LogHelper.Log($"Post {item.Item1}->{item.Item2}");
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
                    LogHelper.Log($"Replacing shop {oldItem} with {item.Item2}");
                    if (!newShopItems.Contains(oldItem))
                        RandomizerMod.RandomizerMod.Instance.Settings.RemoveShopCost(oldItem);

                    int cost = oldShopItemsCosts[oldItem];
                    RandomizerMod.RandomizerMod.Instance.Settings.AddShopCost(item.Item2, cost);
                    newShopItems.Add(item.Item2);
                    LogHelper.Log("Now we got shop costs: " + RandomizerMod.RandomizerMod.Instance.Settings.ShopCosts.Length);
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
                    string newNameKey = LogicManager.RemoveDuplicateSuffix(LanguageStringManager.GetItemName(item));
                    LogicManager.EditItemDef(newNameKey, copy);

                    string itemDisplayName = LanguageStringManager.GetMWLanguageString(def.nameKey, "UI");
                    string fullItemDisplayName = LanguageStringManager.AddItemOwnerNickname(playerId, itemDisplayName);
                    RandomizerMod.LanguageStringManager.SetString("UI", copy.nameKey, fullItemDisplayName);
                }
            }
        }
    }
}
