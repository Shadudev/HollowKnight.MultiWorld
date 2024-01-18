using MultiWorldLib;
using MultiWorldLib.MultiWorld;
using System;
using System.Collections.Generic;

namespace MultiWorldServer.Loggers
{
    internal class ItemsSpoilerLogger
    {
        public static SpoilerLogs GetLogs(ItemsRandomizer itemsRandomizer, List<PlayerItemsPool> playersItemsPools)
        {
            SpoilerLogs logs = new SpoilerLogs
            {
                FullOrderedItemsLog = itemsRandomizer.FullOrderedItemsLog + Environment.NewLine + Environment.NewLine,
            };

            foreach (var playerItemsPool in playersItemsPools)
            {
                string log = "";
                foreach (string group in playerItemsPool.ItemsPool.Keys)
                {
                    log += $"Iterating items group `{group}`{Environment.NewLine}";
                    foreach ((string mwItem, string location) in playerItemsPool.ItemsPool[group])
                    {
                        (int playerId, string item) = LanguageStringManager.ExtractPlayerID(mwItem);
                        log += $"{playersItemsPools[playerId].Nickname}'s {item} -> {location}{Environment.NewLine}";
                    }
                }

                log += Environment.NewLine;
                logs.IndividualWorldSpoilers[playerItemsPool.Nickname] = log;
            }

            return logs;
        }
    }
}
