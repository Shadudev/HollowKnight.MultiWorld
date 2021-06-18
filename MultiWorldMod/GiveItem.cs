using MultiWorldLib;
using MultiWorldLib.Messaging.Definitions.Messages;
using System;

namespace MultiWorldMod
{
    class GiveItem
    {
        // Stop the GiveItem flow and send item to server
        private static bool TryHandlePickedUpItem(RandomizerMod.GiveItemActions.GiveAction action, 
            string rawItem, string location, int geo)
        {
            (int playerId, string item) = LanguageStringManager.ExtractPlayerID(rawItem);
            if (playerId > -1)
            {
                RandomizerMod.RandoLogger.LogItemToTracker(item, location);
                RandomizerMod.RandomizerMod.Instance.Settings.MarkItemFound(item);
                RandomizerMod.RandomizerMod.Instance.Settings.MarkLocationFound(location);
                RandomizerMod.RandoLogger.UpdateHelperLog();
                
                MultiWorldMod.Instance.Settings.AddSentItem(rawItem);
                MultiWorldMod.Instance.Connection.SendItem(location, item, playerId);

                return true;
            }

            return false;
        }

        // Show a pop up for received items and continue the GiveItem flow
        internal static void HandleReceivedItem(MWItemReceiveMessage item)
        {
            if (RandomizerMod.RandomizerMod.Instance.Settings.CheckItemFound(item.Item)) return;

            string itemName = RandomizerMod.Randomization.LogicManager.RemoveDuplicateSuffix(item.Item);
            RandomizerMod.GiveItemActions.ShowEffectiveItemPopup(itemName);
            
            
            RandomizerMod.Randomization.ReqDef def = RandomizerMod.Randomization.LogicManager.GetItemDef(item.Item);
            string originalName = RandomizerMod.LanguageStringManager.GetLanguageString(def.nameKey, "UI");
            
            // Edit according to player
            string itemFromPlayer = LanguageStringManager.AddSourcePlayerNickname(item.From, originalName);
            RandomizerMod.LanguageStringManager.SetString("UI", def.nameKey, itemFromPlayer);

            RandomizerMod.GiveItemActions.GiveAction action = def.action;
            // Geo spawning is normally handled in the shiny, so just add geo instead
            if (def.action == RandomizerMod.GiveItemActions.GiveAction.SpawnGeo)
                def.action = RandomizerMod.GiveItemActions.GiveAction.AddGeo;

            bool originalValue = RandomizerMod.GiveItemActions.RecentItemsShowArea;
            RandomizerMod.GiveItemActions.RecentItemsShowArea = false;
            RandomizerMod.GiveItemActions.GiveItem(action, item.Item, "");
            RandomizerMod.GiveItemActions.RecentItemsShowArea = originalValue;

            // Revert
            RandomizerMod.LanguageStringManager.SetString("UI", def.nameKey, originalName);
        }

        internal static void AddMultiWorldItemHandlers()
        {
            RandomizerMod.GiveItemActions.ExternItemHandlers.Add(TryHandlePickedUpItem);
        }
    }
}
