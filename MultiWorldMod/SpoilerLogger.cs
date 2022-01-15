using MultiWorldLib;
using System;
using System.Collections.Generic;

namespace MultiWorldMod
{
    internal class SpoilerLogger
    {
        private static string storedItemsSpoiler;
        private static (int, string, string)[] storedPlayerItems;

        internal static void StoreItemsSpoiler(string itemsSpoiler)
        {
            storedItemsSpoiler = itemsSpoiler;
        }

        internal static string GetIemsSpoiler()
        {
            return storedItemsSpoiler;
        }

        internal static void StorePlayerItems((int, string, string)[] playerItems)
        {
            storedPlayerItems = playerItems;
        }

        internal static void LogItemsSpoiler()
        {
            //RandomizerMod.RandoLogger.LogSpoiler(storedItemsSpoiler);
            storedItemsSpoiler = null;
        }

        internal static void LogCondensedSpoiler()
        {
            List<string> addedDummyShopItems = new List<string>();
            
            for (int i = 0; i < storedPlayerItems.Length; i++)
            {
                (int _, string itemName) = LanguageStringManager.ExtractPlayerID(storedPlayerItems[i].Item2);
                storedPlayerItems[i].Item2 = itemName;

                (int playerId, string location) = LanguageStringManager.ExtractPlayerID(storedPlayerItems[i].Item3);
                storedPlayerItems[i].Item3 = LanguageStringManager.AddItemOwnerNickname(playerId, location);
            }

            //RandomizerMod.RandoLogger.LogItemsToCondensedSpoiler(storedPlayerItems);
            storedPlayerItems = null;
        }
    }
}