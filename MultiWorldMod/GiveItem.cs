using MultiWorldLib;
using MultiWorldLib.Messaging.Definitions.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiWorldMod
{
    class GiveItem
    {
        private static object s_giveItemLock = new object();
        private static List<string> s_receivedItems = new List<string>();

        // Stop the GiveItem flow and send item to server
        private static bool TryHandlePickedUpItem(RandomizerMod.GiveItemActions.GiveAction action, 
            string itemName, string location, int geo)
        {
            // Bypass for items that are not really placed (allows giving items from server easily)
            if (RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements.
                Where(ILpair => ILpair.Item1 == itemName).Count() == 0) return false;

            lock (s_giveItemLock)
            {
                if (s_receivedItems.Contains(itemName))
                {
                    s_receivedItems.Remove(itemName);
                }
                else if (!RandomizerMod.RandomizerMod.Instance.Settings.CheckItemFound(itemName))
                {
                    ItemSync.Instance.Settings.AddSentItem(itemName);
                    ItemSync.Instance.Connection.SendItemToAll(location, itemName);
                    // Avoid race where item is also received before MarkItemFound is called in RandomizerMod
                    RandomizerMod.RandomizerMod.Instance.Settings.MarkItemFound(itemName);
                }
                else // Trying to pick up an item which was received earlier
                {
                    RandomizerMod.Randomization.ReqDef def = RandomizerMod.Randomization.LogicManager.GetItemDef(itemName);
                    if (def.action == RandomizerMod.GiveItemActions.GiveAction.Grub)
                    {
                        PlayerData.instance.grubsCollected--;
                    }
                    return true;
                }
            }
            return false;
        }

        internal static void HandleReceivedItem(MWItemReceiveMessage item)
        {
            // Ensure item->location matches with sender
            string localItemLocation = RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements.
                FirstOrDefault(ILpair => ILpair.Item1 == item.Item).Item2;

            // Bypass for items that are not really placed (allows giving items from server easily)
            if (string.IsNullOrEmpty(localItemLocation) || localItemLocation != item.Location) return;

            lock (s_giveItemLock)
            {
                if (RandomizerMod.RandomizerMod.Instance.Settings.CheckItemFound(item.Item)) return;

                RandomizerMod.RandomizerMod.Instance.Settings.MarkItemFound(item.Item);
                s_receivedItems.Add(item.Item);
            }

            GiveReceivedItem(item);
        }

        private static void GiveReceivedItem(MWItemReceiveMessage item)
        {
            string itemName = RandomizerMod.RandomizerMod.Instance.Settings.GetEffectiveItem(
                RandomizerMod.Randomization.LogicManager.RemoveDuplicateSuffix(item.Item));
            RandomizerMod.Randomization.ReqDef def = RandomizerMod.Randomization.LogicManager.GetItemDef(itemName);

            string originalName = RandomizerMod.LanguageStringManager.GetLanguageString(def.nameKey, "UI");
            string itemFromPlayer = LanguageStringManager.AddSourcePlayerNickname(item.From, originalName);
            RandomizerMod.LanguageStringManager.SetString("UI", def.nameKey, itemFromPlayer);
            RandomizerMod.GiveItemActions.ShowEffectiveItemPopup(itemName);
            RandomizerMod.LanguageStringManager.SetString("UI", def.nameKey, originalName);

            RandomizerMod.Randomization.ReqDef modifiedDef = def;
            // Geo spawning is normally handled in the shiny, so just add geo instead
            if (def.action == RandomizerMod.GiveItemActions.GiveAction.SpawnGeo)
            {
                modifiedDef.action = RandomizerMod.GiveItemActions.GiveAction.AddGeo;
            }

            RandomizerMod.Randomization.LogicManager.EditItemDef(itemName, modifiedDef);

            // Give bonus 300 geo if item is a duplicate
            if (RandomizerMod.RandomizerMod.Instance.Settings.GetAdditiveCount(itemName) > GetMaxAdditiveLevel(itemName))
            {
                HeroController.instance.AddGeo(300);
            }

            try
            {
                RandomizerMod.GiveItemActions.GiveItem(modifiedDef.action, item.Item, item.Location);
            }
            catch (Exception e)
            {
                LogHelper.LogError($"Failed to give item, {e.Message}");
                LogHelper.LogError(e.StackTrace);
            }

            // Revert
            RandomizerMod.Randomization.LogicManager.EditItemDef(itemName, def);
        }

        // Based on RandomizerMod.SaveSettings.GetEffectiveItem
        private static int GetMaxAdditiveLevel(string item)
        {
            string[] additiveSet = RandomizerMod.Randomization.LogicManager.AdditiveItemSets.FirstOrDefault(set => set.Contains(item));
            if (additiveSet != null)
                return additiveSet.Length - 1;
            
            return 0;
        }

        internal static void AddMultiWorldItemHandlers()
        {
            RandomizerMod.GiveItemActions.ExternItemHandlers.Insert(0, TryHandlePickedUpItem);
        }

        internal static void ClearReceivedItemsList()
        {
            s_receivedItems.Clear();
        }
    }
}
