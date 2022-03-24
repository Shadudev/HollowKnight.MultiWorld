using MultiWorldLib;
using System;
using System.Collections.Generic;

namespace MultiWorldServer
{
    internal class ItemsRandomizer
    {
        // Provided Data
        private readonly List<PlayerItemsPool> playersItemsPools;
        private readonly Random random;

        // Generated Data
        private readonly List<List<(int, int)>> playersItemsLocations;

        public ItemsRandomizer(List<PlayerItemsPool> playersItemsPools, int seed)
        {
            this.playersItemsPools = playersItemsPools;
            // This allows for consistent randomization using seed, settings and who sent first? *work by ready order
            this.playersItemsPools.Sort((p1, p2) => p1.ItemsPool.Length - p2.ItemsPool.Length);
            
            playersItemsLocations = new List<List<(int, int)>>();
            for (int i = 0; i < this.playersItemsPools.Count; i++)
            {
                this.playersItemsPools[i].PlayerId = i;
                playersItemsLocations.Add(new List<(int, int)>());
            }
            random = new Random(seed);

        }

        internal List<PlayerItemsPool> Randomize()
        {
            foreach (var playerItemsLocations in playersItemsLocations)
                playerItemsLocations.Clear();

            RandomizeItemsPools();
            return playersItemsPools;
        }

        internal List<(string, string)> GetPlayerItems(int playerId)
        {
            List<(string, string)> playerItems = new List<(string, string)>();
            foreach ((int player, int itemIndex) in playersItemsLocations[playerId])
            {
                (string item, string location) placement, originalPlacement = playersItemsPools[player].ItemsPool[itemIndex];
                
                placement.location = originalPlacement.location;
                if (player == playerId)
                    placement.item = LanguageStringManager.ExtractPlayerID(originalPlacement.item).Item;
                else
                    placement.item = originalPlacement.item;

                playerItems.Add(placement);
            }

            return playerItems;
        }

        private void RandomizeItemsPools()
        {
            Queue<(string, string)>[] unplacedItemsPools = new Queue<(string, string)>[playersItemsPools.Count];
            for (int i = 0; i < unplacedItemsPools.Length; i++)
            {
                unplacedItemsPools[i] = new Queue<(string, string)>(playersItemsPools[i].ItemsPool);
            }

            List<(int, int)> availableLocations = new List<(int, int)>();
            for (int i = 0; i < playersItemsPools.Count; i++)
            {
                if (playersItemsPools[i].ItemsPool.Length > 0)
                {
                    availableLocations.Add((i, 0));
                }
            }

            while (availableLocations.Count > 0)
            {
                int randomLocationIndex = random.Next(availableLocations.Count);
                (int, int) randomAvailableLocation = availableLocations[randomLocationIndex];
                availableLocations.RemoveAt(randomLocationIndex);

                (int playerIndex, (string, string) placement) = GetRandomPlayerItem(unplacedItemsPools);

                SetItemAtLocation(randomAvailableLocation, placement, playerIndex);

                if (unplacedItemsPools[playerIndex].Count > 0)
                {
                    int itemIndex = playersItemsPools[playerIndex].ItemsPool.Length - unplacedItemsPools[playerIndex].Count;
                    availableLocations.Add((playerIndex, itemIndex));
                }
            }
        }

        private (int, (string, string)) GetRandomPlayerItem(Queue<(string, string)>[] unplacedItemsPools)
        {
            int sum = 0;
            foreach (var unplacedItemsPool in unplacedItemsPools)
            {
                sum += unplacedItemsPool.Count;
            }

            int number = random.Next(sum);

            int playerIndex = -1;
            for (int i = 0; i < unplacedItemsPools.Length; i++)
            {
                if (unplacedItemsPools[i].Count > 0 && number < unplacedItemsPools[i].Count)
                {
                    playerIndex = i;
                    break;
                }

                number -= unplacedItemsPools[i].Count;
            }

            return (playerIndex, unplacedItemsPools[playerIndex].Dequeue());
        }

        private void SetItemAtLocation((int player, int itemIndex) location, (string, string) newItem, int playerGivenItem)
        {
            newItem.Item1 = LanguageStringManager.AddPlayerId(newItem.Item1, playerGivenItem);

            playersItemsPools[location.player].ItemsPool[location.itemIndex].Item1 = newItem.Item1;

            playersItemsLocations[playerGivenItem].Add(location);
        }
    }
}