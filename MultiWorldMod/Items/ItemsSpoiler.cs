using MultiWorldLib.MultiWorld;
using UnityEngine;

namespace MultiWorldMod.Items
{
    internal class ItemsSpoiler
    {
        public static void Save(SpoilerLogs spoilerLogs)
        {
            string multiworldDirPath = Path.Combine(Application.persistentDataPath, "MultiWorld");
            Directory.CreateDirectory(multiworldDirPath);
            
            if (MultiWorldMod.GS.SeparateIndividualWorldsSpoilers)
            {
                string multiworldIndividualWorldsDirPath = Path.Combine(
                    multiworldDirPath, $"save{GameManager._instance.profileID}-worlds");
                
                if (Directory.Exists(multiworldIndividualWorldsDirPath)) 
                    Directory.Delete(multiworldIndividualWorldsDirPath, false);
                Directory.CreateDirectory(multiworldIndividualWorldsDirPath);

                List<string> addedNicknames = new();
                foreach (string nickname in spoilerLogs.IndividualWorldSpoilers.Keys)
                {
                    string safeNickname = GetPathSafeNickname(nickname);
                    string chosenNickname = GetUniqueNickname(safeNickname, addedNicknames);
                    
                    string worldSpoilerPath = Path.Combine(multiworldIndividualWorldsDirPath, chosenNickname + ".txt");
                    File.WriteAllText(worldSpoilerPath, spoilerLogs.IndividualWorldSpoilers[nickname]);
                    
                    addedNicknames.Add(nickname);
                }
            }
            else
            {
                foreach (string nickname in spoilerLogs.IndividualWorldSpoilers.Keys)
                {
                    spoilerLogs.FullOrderedItemsLog += $"{nickname}'s World:{Environment.NewLine}" +
                        $"{spoilerLogs.IndividualWorldSpoilers[nickname]}";
                }
            }
            
            string itemsSpoilerPath = Path.Combine(multiworldDirPath, $"save{GameManager._instance.profileID}-spoiler.txt");
            File.WriteAllText(itemsSpoilerPath, spoilerLogs.FullOrderedItemsLog);
        }

        private static string GetPathSafeNickname(string nickname)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                nickname = nickname.Replace(c, '-');

            return nickname;
        }

        private static string GetUniqueNickname(string baseNickname, List<string> addedNicknames)
        {
            if (!addedNicknames.Contains(baseNickname)) return baseNickname;

            int suffix = 1;
            string nickname;
            do
            {
                nickname = $"{baseNickname} ({suffix++}";
            } while (addedNicknames.Contains(nickname));
            
            return nickname;
        }
    }
}
