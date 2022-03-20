using MultiWorldLib;
using System;
using System.Collections.Generic;

namespace MultiWorldServer
{
    internal class ItemsSpoilerLogger
    {
        internal static string GetLog(List<PlayerItemsPool> playersItemsPools)
        {
            Dictionary<int, string> nicknames = new Dictionary<int, string>();
            foreach (var playerItemsPool in playersItemsPools)
            {
                nicknames[playerItemsPool.PlayerId] = playerItemsPool.Nickname;
            }

            string log = LogAllItems(playersItemsPools, nicknames);


            foreach (PlayerItemsPool playerItemsPool in playersItemsPools)
            {
                log += Environment.NewLine + $"ITEMS IN {playerItemsPool.Nickname}'S WORLD" + Environment.NewLine; ;
                log += LogPoolItems(playerItemsPool, nicknames);
            }

            return log;
        }

        private static string LogAllItems(List<PlayerItemsPool> playersItemsPools, Dictionary<int, string> nicknames)
        {
            string log = Environment.NewLine;
            void AddToLog(string message) => log += message + Environment.NewLine;
            
            List<(string, string)> allItems = new List<(string, string)>();
            foreach (var playerItemsPool in playersItemsPools)
            {
                foreach (var item in playerItemsPool.ItemsPool)
                {
                    (int playerId, string itemName) = LanguageStringManager.ExtractPlayerID(item.Item1);
                    string fullItemName = $"{nicknames[playerId]}'s {itemName}";

                    string fullItemLocation = $"{nicknames[playerItemsPool.PlayerId]}'s {item.Item2}";

                    allItems.Add((fullItemName, fullItemLocation));
                }
            }

            foreach (var item in allItems)
            {
                AddToLog(GetItemLogLine(item));
            }

            return log;
        }

        private static string LogPoolItems(PlayerItemsPool playerItemsPool, Dictionary<int, string> nicknames)
        {
            string log = "";
            void AddToLog(string message) => log += message + Environment.NewLine;

            List<(string, string)> playerWorldItems = new List<(string, string)>();

            foreach (var item in playerItemsPool.ItemsPool)
            {
                (int playerId, string itemName) = LanguageStringManager.ExtractPlayerID(item.Item1);
                if (playerId == -1)
                    playerId = playerItemsPool.PlayerId;
                string nickname = nicknames[playerId];
                string fullItemName = $"{nickname}'s {itemName}";

                (int _, string location) = LanguageStringManager.ExtractPlayerID(item.Item2);

                playerWorldItems.Add((fullItemName, location));
            }

            foreach (var item in playerWorldItems)
            {
                AddToLog(GetItemLogLine(item));
            }

            return log;
        }

        private static string GetItemLogLine((string, string) item)
        {
            return $"{item.Item1.Replace('_', ' ')}<---at--->{item.Item2.Replace('_', ' ')}";
        }
    }
}
