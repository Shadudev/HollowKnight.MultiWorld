using MultiWorldLib;
using MultiWorldLib.MultiWorldSettings;
using System;
using System.Collections.Generic;

namespace MultiWorldServer
{
    internal class ItemsRandomizer
    {
        private readonly Random random;

        // Provided Data
        private readonly List<PlayerItemsPool> playersItemsPools;
        private readonly MultiWorldGenerationSettings settings;

        // Generated Data
        private readonly List<List<(int, int)>> playersItemsLocations;
        private int totalItemsAmount;
        public string FullOrderedItemsLog;

        public ItemsRandomizer(List<PlayerItemsPool> playersItemsPools, MultiWorldGenerationSettings settings)
        {
            this.playersItemsPools = playersItemsPools;
            this.settings = settings;
            // This allows for consistent randomization using seed, settings and who sent first? *work by ready order
            this.playersItemsPools.Sort((p1, p2) => p1.ItemsPool.Length - p2.ItemsPool.Length);
            
            playersItemsLocations = new List<List<(int, int)>>();
            for (int i = 0; i < this.playersItemsPools.Count; i++)
            {
                this.playersItemsPools[i].PlayerId = i;
                playersItemsLocations.Add(new List<(int, int)>());
            }
            random = new Random(settings.Seed);
        }

        internal List<PlayerItemsPool> Randomize()
        {
            FullOrderedItemsLog = "";
            foreach (var playerItemsLocations in playersItemsLocations)
                playerItemsLocations.Clear();

            RandomizeItemsPools();
            return playersItemsPools;
        }

        internal List<(string, string)> GetPlayerItems(int playerId)
        {
            List<(string, string)> playerItems = new List<(string, string)>();
            foreach ((int player, int itemIndex) in playersItemsLocations[playerId])
                playerItems.Add(playersItemsPools[player].ItemsPool[itemIndex]);

            return playerItems;
        }

        private void RandomizeItemsPools()
        {
            Queue<string>[] unplacedItems = GetPlayersItems(playersItemsPools);
            List<(int player, int locationIndex)> availableLocations = new List<(int, int)>();
            totalItemsAmount = 0;

            for (int i = 0; i < playersItemsPools.Count; i++)
            {
                if (playersItemsPools[i].ItemsPool.Length > 0)
                {
                    availableLocations.Add((i, 0));
                    totalItemsAmount += playersItemsPools[i].ItemsPool.Length;
                }
            }

            while (availableLocations.Count > 0)
            {
                (int itemOwnerId, string item) = GetRandomPlayerItem(unplacedItems);
                (int, int) location = PopRandomLocation(availableLocations);

                SetItemAtLocation(location, item, itemOwnerId);

                unplacedItems[itemOwnerId].Dequeue();
                totalItemsAmount--;

                if (unplacedItems[itemOwnerId].Count > 0)
                    AddNewAvailableLocation(unplacedItems, availableLocations, itemOwnerId);
            }
        }

        private Queue<string>[] GetPlayersItems(List<PlayerItemsPool> playersItemsPools)
        {
            Queue<string>[] playersItems = new Queue<string>[playersItemsPools.Count];
            for (int i = 0; i < playersItems.Length; i++)
            {
                playersItems[i] = new Queue<string>();
                Array.ForEach(playersItemsPools[i].ItemsPool, placement => playersItems[i].Enqueue(placement.Item1));
            }

            return playersItems;
        }

        private (int, string) GetRandomPlayerItem(Queue<string>[] unplacedItems)
        {
            int playerIndex = -1, index = random.Next(totalItemsAmount);

            for (int i = 0; i < unplacedItems.Length; i++)
            {
                if (unplacedItems[i].Count > 0 && index < unplacedItems[i].Count)
                {
                    playerIndex = i;
                    break;
                }

                index -= unplacedItems[i].Count;
            }

            return (playerIndex, unplacedItems[playerIndex].Peek());
        }

        private (int player, int itemIndex) PopRandomLocation(List<(int, int)> availableLocations)
        {
            int randomLocationIndex = random.Next(availableLocations.Count);
            (int, int) location = availableLocations[randomLocationIndex];

            availableLocations.RemoveAt(randomLocationIndex);
            return location;
        }

        private void SetItemAtLocation((int player, int itemIndex) location, string item, int playerGivenItem)
        {
            string mwItem = LanguageStringManager.AddPlayerId(item, playerGivenItem);
            playersItemsPools[location.player].ItemsPool[location.itemIndex].Item1 = mwItem;

            playersItemsLocations[playerGivenItem].Add(location);

            FullOrderedItemsLog += $"{playersItemsPools[playerGivenItem].Nickname}'s {item} -> " +
                $"{playersItemsPools[location.player].Nickname}'s {playersItemsPools[location.player].ItemsPool[location.itemIndex].Item2}" +
                $"{Environment.NewLine}";
        }
        
        private void AddNewAvailableLocation(Queue<string>[] unplacedItems, List<(int, int)> availableLocations, int playerIndex)
        {
            int itemIndex = playersItemsPools[playerIndex].ItemsPool.Length - unplacedItems[playerIndex].Count;
            availableLocations.Add((playerIndex, itemIndex));
        }
    }
}