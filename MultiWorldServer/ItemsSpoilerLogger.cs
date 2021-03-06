using MultiWorldLib;
using System;
using System.Collections.Generic;

namespace MultiWorldServer
{
    internal class ItemsSpoilerLogger
    {
        public static string GetLog(ItemsRandomizer itemsRandomizer, List<PlayerItemsPool> playersItemsPools)
        {
            string log = itemsRandomizer.FullOrderedItemsLog + Environment.NewLine + Environment.NewLine;
            
            foreach (var playerItemsPool in playersItemsPools)
            {
                log += $"{playerItemsPool.Nickname}'s World:{Environment.NewLine}";
                foreach ((string mwItem, string location) in playerItemsPool.ItemsPool)
                {
                    (int playerId, string item) = LanguageStringManager.ExtractPlayerID(mwItem);
                    log += $"{playersItemsPools[playerId].Nickname}'s {item} -> {location}{Environment.NewLine}";
                }

                log += "\n";
            }

            return log;
        }
    }
}
