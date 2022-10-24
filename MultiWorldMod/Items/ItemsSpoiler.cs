using MultiWorldLib.MultiWorld;
using UnityEngine;

namespace MultiWorldMod.Items
{
    // TODO finish this
    internal class ItemsSpoiler
    {
        public static void Save(SpoilerLogs spoilerLogs)
        {
            string multiworldDirPath = Path.Combine(Application.persistentDataPath, "MultiWorld");
            Directory.CreateDirectory(multiworldDirPath);
            
            // if config.separate individual worlds spoiler files
            // create a directory or wipe the existing one's content for the instance id within multiworld dir, with the name "save{profileID}-indiviual-worlds"
            // Iterate worlds and 
            // else concat worlds to full items log with log += $"{logs.IndividualWorldSpoilers.key}'s World:{Environment.NewLine}";

            string itemsSpoilerPath = Path.Combine(multiworldDirPath, $"save{GameManager._instance.profileID}-spoiler.txt");
            File.WriteAllText(itemsSpoilerPath, spoilerLogs.FullOrderedItemsLog);
        }
    }
}
