using MultiWorldLib;
using MultiWorldLib.Messaging.Definitions.Messages;
using System;
using System.Linq;

namespace MultiWorldMod
{
    class GiveItem
    {
        // Stop the GiveItem flow and send item to server
        private static bool TryHandlePickedUpItem(RandomizerMod.GiveItemActions.GiveAction action, 
            string itemName, string location, int geo)
        {
            if (!RandomizerMod.RandomizerMod.Instance.Settings.CheckItemFound(itemName))
            {
                MultiWorldMod.Instance.Settings.AddSentItem(itemName);
                MultiWorldMod.Instance.Connection.SendItemToAll(location, itemName);
            }

            return false;
        }

        internal static void HandleReceivedItem(MWItemReceiveMessage item)
        {
            if (RandomizerMod.RandomizerMod.Instance.Settings.CheckItemFound(item.Item)) return;
            LogHelper.Log($"Received {item.Item} from {item.From}");

            RandomizerMod.Randomization.ReqDef def = RandomizerMod.Randomization.LogicManager.GetItemDef(item.Item);
            GiveReceivedItem(def, item.Item, item);
        }

        private static void GiveReceivedItem(RandomizerMod.Randomization.ReqDef def, 
            string itemName, MWItemReceiveMessage item)
        {
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
            RandomizerMod.GiveItemActions.ExternItemHandlers.Add(TryHandlePickedUpItem);
        }
    }
}
