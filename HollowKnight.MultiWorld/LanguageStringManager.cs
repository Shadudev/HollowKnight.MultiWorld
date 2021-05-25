using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiWorld
{
    internal static class LanguageStringManager
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

        internal static void SetMWNames(List<string> nicknames)
        {
            MWNicknames = new Dictionary<int, string>();
            for (int i = 0; i < nicknames.Count; i++)
            {
                MWNicknames[i] = nicknames[i];
            }
        }

        internal static void SetMWNames(IDictionary<int, string> nicknames)
        {
            if (nicknames == null) return;
            MWNicknames = new Dictionary<int, string>(nicknames);
        }
    }
}
