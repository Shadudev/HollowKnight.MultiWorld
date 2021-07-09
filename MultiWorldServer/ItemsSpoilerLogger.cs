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
            
            List<(int, string, string)> allItems = new List<(int, string, string)>();
            foreach (var playerItemsPool in playersItemsPools)
            {
                foreach (var item in playerItemsPool.ItemsPool)
                {
                    (int playerId, string itemName) = LanguageStringManager.ExtractPlayerID(item.Item2);
                    string fullItemName = $"{nicknames[playerId]}'s {itemName}";

                    string fullItemLocation = $"{nicknames[playerItemsPool.PlayerId]}'s {item.Item3}";

                    allItems.Add((item.Item1, fullItemName, fullItemLocation));
                }
            }

            allItems.Sort((item1, item2) => 
                LanguageStringManager.GetItemOrder(item1) - LanguageStringManager.GetItemOrder(item2));

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

            List<(int, string, string)> playerWorldItems = new List<(int, string, string)>();

            foreach (var item in playerItemsPool.ItemsPool)
            {
                (int playerId, string itemName) = LanguageStringManager.ExtractPlayerID(item.Item2);
                if (playerId == -1)
                    playerId = playerItemsPool.PlayerId;
                string nickname = nicknames[playerId];
                string fullItemName = $"{nickname}'s {itemName}";

                (int _, string location) = LanguageStringManager.ExtractPlayerID(item.Item3);

                playerWorldItems.Add((item.Item1, fullItemName, location));
            }

            foreach (var item in playerWorldItems)
            {
                AddToLog(GetItemLogLine(item));
            }

            return log;
        }

        private static string GetItemLogLine((int, string, string) item)
        {
            return $"({item.Item1}) {item.Item2.Replace('_', ' ')}<---at--->{item.Item3.Replace('_', ' ')}";
        }
    }
}
