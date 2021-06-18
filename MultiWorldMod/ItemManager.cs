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
            CreateMissingItemDefinitions(items);
            UpdateItemsVariables(items);
        }

        private static void UpdateItemsVariables((int, string, string)[] items)
        {
            string[] itemsLocations = items.Select(orderedItemLocation => orderedItemLocation.Item3).ToArray();
            List<(string, string)> originalItems = RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements.Where(
                pair => itemsLocations.Contains(pair.Item2)).ToList();
            string[] shopItems = RandomizerMod.RandomizerMod.Instance.Settings.ShopCosts.Select(pair => pair.Item1).ToArray();

            foreach (var item in items)
            {
                (string oldItem, string location) = originalItems.Find(pair => pair.Item2 == item.Item3);
                RandomizerMod.RandomizerMod.Instance.Settings.AddItemPlacement(item.Item2, location);
                originalItems.Remove((oldItem, location));

                if (shopItems.Contains(oldItem))
                {
                    int cost = RandomizerMod.RandomizerMod.Instance.Settings.GetShopCost(oldItem);
                    RandomizerMod.RandomizerMod.Instance.Settings.RemoveShopCost(oldItem);
                    RandomizerMod.RandomizerMod.Instance.Settings.AddShopCost(item.Item2, cost);
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
                    string newNameKey = LanguageStringManager.GetItemName(item);
                    LogicManager.EditItemDef(newNameKey, copy);

                    string itemDisplayName = LanguageStringManager.GetMWLanguageString(def.nameKey, "UI");
                    string fullItemDisplayName = LanguageStringManager.AddItemOwnerNickname(playerId, itemDisplayName);
                    RandomizerMod.LanguageStringManager.SetString("UI", copy.nameKey, fullItemDisplayName);
                }
            }
        }
    }
}
