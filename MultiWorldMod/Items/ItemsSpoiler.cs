using UnityEngine;

namespace MultiWorldMod.Items
{
    internal class ItemsSpoiler
    {
        public static void Save(string itemsSpoiler)
        {
            string multiworldDirPath = Path.Combine(Application.persistentDataPath, "MultiWorld");
            Directory.CreateDirectory(multiworldDirPath);
            
            string itemsSpoilerPath = Path.Combine(multiworldDirPath, $"save{GameManager._instance.profileID}-spoiler.txt");
            File.WriteAllText(itemsSpoilerPath, itemsSpoiler);
        }
    }
}
