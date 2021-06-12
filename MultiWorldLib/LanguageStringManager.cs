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

        public static (int PlayerId, string Item) ExtractPlayerID(string idItem)
        {
            Regex prefix = new Regex(@"^MW\((\d+)\)_");
            if (!prefix.IsMatch(idItem)) return (-1, idItem);
            int id = Int32.Parse(prefix.Match(idItem).Groups[1].Value) - 1;
            return (id, prefix.Replace(idItem, ""));
        }

        public static void SetMWNames(List<string> nicknames)
        {
            MWNicknames = new Dictionary<int, string>();
            for (int i = 0; i < nicknames.Count; i++)
            {
                MWNicknames[i] = nicknames[i];
            }
        }

        public static void SetMWNames(IDictionary<int, string> nicknames)
        {
            if (nicknames == null) return;
            MWNicknames = new Dictionary<int, string>(nicknames);
        }

        public static string addPlayerId(string item, int playerId)
        {
            return "MW(" + (playerId + 1) + ")_" + item;
        }

        public static (string, string) ExtractSuffix(string input)
        {
            Regex suffix = new Regex(@"_\(\d+\)$");
            if (!suffix.IsMatch(input)) return (input, "");
            Match m = suffix.Match(input);
            return (suffix.Replace(input, ""), m.Groups[0].Value);
        }
    }
}
