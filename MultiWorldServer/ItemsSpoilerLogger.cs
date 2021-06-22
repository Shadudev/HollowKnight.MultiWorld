using MultiWorldLib;
using System;
using System.Collections.Generic;

namespace MultiWorldServer
{
    internal class ItemsSpoilerLogger
    {
        internal static string GetLog(List<PlayerItemsPool> playersItemsPools)
        {
            string log = LogAllItems(playersItemsPools);

            foreach (PlayerItemsPool playerItemsPool in playersItemsPools)
            {
                log += Environment.NewLine + $"ITEMS IN {playerItemsPool.Nickname}'S WORLD";
                log += LogPoolItems(playerItemsPool);
            }

            return log;
        }

        private static string LogAllItems(List<PlayerItemsPool> playersItemsPools)
        {
            string log = "";
            void AddToLog(string message) => log += message + Environment.NewLine;
            
            List<(int, string, string)> allItems = new List<(int, string, string)>();
            foreach (var playerItemsPool in playersItemsPools)
            {
                foreach (var item in playerItemsPool.ItemsPool)
                {
                    (int playerId, string itemName) = LanguageStringManager.ExtractPlayerID(item.Item2);
                    if (playerId == -1)
                        playerId = playerItemsPool.PlayerId;
                    string fullItemName = LanguageStringManager.AddItemOwnerNickname(playerId, itemName);

                    string location;
                    (playerId, location) = LanguageStringManager.ExtractPlayerID(item.Item3);
                    if (playerId == -1)
                        playerId = playerItemsPool.PlayerId;
                    string fullItemLocation = LanguageStringManager.AddItemOwnerNickname(playerId, location);

                    allItems.Add((item.Item1, fullItemName, fullItemLocation));
                }
            }

            allItems.Sort((item1, item2) => 
                LanguageStringManager.GetItemOrder(item1) - LanguageStringManager.GetItemOrder(item2));

            foreach (var item in allItems)
            {
                AddToLog($"({item.Item1}) {item.Item2}<---at--->{item.Item3}");
            }

            return log;
        }

        private static string LogPoolItems(PlayerItemsPool playerItemsPool)
        {
            string log = "";
            void AddToLog(string message) => log += message + Environment.NewLine;

            List<(int, string, string)> playerWorldItems = new List<(int, string, string)>();

            foreach (var item in playerItemsPool.ItemsPool)
            {
                (int playerId, string itemName) = LanguageStringManager.ExtractPlayerID(item.Item2);
                if (playerId == -1)
                    playerId = playerItemsPool.PlayerId;
                string fullItemName = LanguageStringManager.AddItemOwnerNickname(playerId, itemName);

                (int _, string location) = LanguageStringManager.ExtractPlayerID(item.Item3);

                playerWorldItems.Add((item.Item1, fullItemName, location));
            }

            foreach (var item in playerWorldItems)
            {
                AddToLog($"({item.Item1}) {item.Item2}<---at--->{item.Item3}");
            }

            return log;
        }
    }
}
