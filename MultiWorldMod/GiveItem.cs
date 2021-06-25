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
                RandomizerMod.RandomizerMod.Instance.Settings.MarkItemFound(rawItem);
                RandomizerMod.RandomizerMod.Instance.Settings.MarkLocationFound(location);
                RandomizerMod.RandoLogger.UpdateHelperLog();
                
                MultiWorldMod.Instance.Settings.AddSentItem(rawItem);
                MultiWorldMod.Instance.Connection.SendItem(location, item, playerId);

                return true;
            }

            return false;
        }

        internal static void HandleReceivedItem(MWItemReceiveMessage item)
        {
            if (RandomizerMod.RandomizerMod.Instance.Settings.CheckItemFound(item.Item)) return;

            string itemName = RandomizerMod.RandomizerMod.Instance.Settings.GetEffectiveItem(
                RandomizerMod.Randomization.LogicManager.RemoveDuplicateSuffix(item.Item));
            RandomizerMod.Randomization.ReqDef def = RandomizerMod.Randomization.LogicManager.GetItemDef(itemName);
            string originalName = RandomizerMod.LanguageStringManager.GetLanguageString(def.nameKey, "UI");

            LogHelper.Log($"Received {originalName} from {item.From}");
            GiveReceivedItem(def, itemName, originalName, item);
        }

        private static void GiveReceivedItem(RandomizerMod.Randomization.ReqDef def, 
            string itemName, string originalName, MWItemReceiveMessage item)
        {
            // Edit according to player
            RandomizerMod.Randomization.ReqDef modifiedDef = def;

            // Show popup with from (doesn't work for specific item handling in RandomizerMod.LanguageStringManager.GetString)
            string itemFromPlayer = LanguageStringManager.AddSourcePlayerNickname(item.From, originalName);
            RandomizerMod.LanguageStringManager.SetString("UI", def.nameKey, itemFromPlayer);
            RandomizerMod.GiveItemActions.ShowEffectiveItemPopup(itemName);
            RandomizerMod.LanguageStringManager.SetString("UI", def.nameKey, originalName);

            // Geo spawning is normally handled in the shiny, so just add geo instead
            if (def.action == RandomizerMod.GiveItemActions.GiveAction.SpawnGeo)
            {
                modifiedDef.action = RandomizerMod.GiveItemActions.GiveAction.AddGeo;
            }

            RandomizerMod.Randomization.LogicManager.EditItemDef(itemName, modifiedDef);

            // Fake the area name as the player's name to show "from {player}"
            RandomizerMod.Randomization.ReqDef locationDef = new RandomizerMod.Randomization.ReqDef
            {
                areaName = item.From
            };
            RandomizerMod.Randomization.LogicManager.EditItemDef("", locationDef);

            try
            {
                RandomizerMod.GiveItemActions.GiveItem(modifiedDef.action, item.Item, "");
            } 
            catch (Exception e)
            {
                LogHelper.LogError($"Failed to give item, {e.Message}");
                LogHelper.LogError(e.StackTrace);
            }

            // Revert
            RandomizerMod.Randomization.LogicManager.EditItemDef(itemName, def);
        }

        internal static void AddMultiWorldItemHandlers()
        {
            RandomizerMod.GiveItemActions.ExternItemHandlers.Add(TryHandlePickedUpItem);
        }
    }
}
