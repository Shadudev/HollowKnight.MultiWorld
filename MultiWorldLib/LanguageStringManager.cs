using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MultiWorldLib
{
    public static class LanguageStringManager
    {
        private static Dictionary<int, string> MWNicknames = new Dictionary<int, string>();

        public static string GetMWPlayerName(int playerId)
        {
            string name = "Player " + (playerId + 1);
            if (MWNicknames != null && MWNicknames.ContainsKey(playerId))
            {
                name = MWNicknames[playerId];
            }
            return name;
        }

        public static bool IsMWItem(string item)
        {
            Regex prefix = new Regex(@"^MW\((\d+)\)_");
            return prefix.IsMatch(item);
        }

        public static (int PlayerId, string Item) ExtractPlayerID(string idItem)
        {
            Regex prefix = new Regex(@"^MW\((\d+)\)_");
            if (!prefix.IsMatch(idItem)) return (-1, idItem);
            int id = Int32.Parse(prefix.Match(idItem).Groups[1].Value) - 1;
            return (id, prefix.Replace(idItem, ""));
        }

        public static (int PlayerId, string Item) ExtractPlayerID((int, string, string) item)
        {
            return ExtractPlayerID(GetItemName(item));
        }

        public static void SetMWNames(string[] nicknames)
        {
            MWNicknames = new Dictionary<int, string>();
            for (int i = 0; i < nicknames.Length; i++)
            {
                MWNicknames[i] = nicknames[i];
            }
        }

        public static void SetMWNames(IDictionary<int, string> nicknames)
        {
            if (nicknames == null) return;
            MWNicknames = new Dictionary<int, string>(nicknames);
        }

        public static string GetLanguageString(string key, string sheetTitle)
        {
            int playerId;
            (playerId, key) = ExtractPlayerID(key);
            if (key.StartsWith("RANDOMIZER_NAME_GRUB"))
            {
                return "Grub";
            } 
            if (key.StartsWith("RANDOMIZER_NAME_GRIMMKIN_FLAME"))
            {
                return "Grimmkin Flame";
            }
            
            return RandomizerMod.LanguageStringManager.GetLanguageString(key, sheetTitle);
        }

        public static string AddSourcePlayerNickname(string playerName, string itemDisplayName)
        {
            return itemDisplayName + $"\nfrom {playerName}";
        }

        public static string AddItemOwnerNickname(int playerId, string itemDisplayName)
        { 
            return MWNicknames[playerId] + "'s " + itemDisplayName;
        }

        public static string AddPlayerId(string item, int playerId)
        {
            return "MW(" + (playerId + 1) + ")_" + item;
        }
        
        public static string AddPlayerId((int, string, string) item, int playerId)
        {
            return AddPlayerId(GetItemName(item), playerId);
        }

        public static (string, string) ExtractSuffix(string input)
        {
            Regex suffix = new Regex(@"_\(\d+\)$");
            if (!suffix.IsMatch(input)) return (input, "");
            Match m = suffix.Match(input);
            return (suffix.Replace(input, ""), m.Groups[0].Value);
        }

        public static int GetItemOrder((int, string, string) item)
        {
            return item.Item1;
        }

        public static string GetItemName((int, string, string) item)
        {
            return item.Item2;
        }

        public static string GetItemLocation((int, string, string) item)
        {
            return item.Item3;
        }

        public static void SetItemName(ref (int, string, string) item, string newName)
        {
            item.Item2 = newName;
        }
    }
}
