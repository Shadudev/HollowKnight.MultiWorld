using MultiWorldLib;
using System;
using System.Collections.Generic;

namespace MultiWorldServer
{
    internal class ItemsRandomizer
    {
        private List<PlayerItemsPool> playersItemsPools;
        private Random random;

        public ItemsRandomizer(List<PlayerItemsPool> playersItemsPools, int seed)
        {
            this.playersItemsPools = playersItemsPools;
            // This allows for consistent randomization using seed, settings and who sent first? *work by ready order
            this.playersItemsPools.Sort((p1, p2) => p1.ItemsPool.Length - p2.ItemsPool.Length);
            // We sort the items by order since the provided array may not be in order
            this.playersItemsPools.ForEach(
                playerItemsPool => Array.Sort(playerItemsPool.ItemsPool, 
                (item1, item2) => LanguageStringManager.GetItemOrder(item1) - LanguageStringManager.GetItemOrder(item2)));
            for (int i = 0; i < this.playersItemsPools.Count; i++)
            {
                this.playersItemsPools[i].PlayerId = i;
            }
            random = new Random(seed);
        }

        internal List<PlayerItemsPool> Randomize()
        {
            RandomizeItemsPools();
            return playersItemsPools;
        }

        private void RandomizeItemsPools()
        {
            Queue<(int, string, string)>[] unplacedItemsPools = new Queue<(int, string, string)>[playersItemsPools.Count];
            int itemOrder = 1;
            for (int i = 0; i < unplacedItemsPools.Length; i++)
            {
                unplacedItemsPools[i] = new Queue<(int, string, string)>(playersItemsPools[i].ItemsPool);
            }

            List<(int, int)> availableLocations = new List<(int, int)>();
            for (int i = 0; i < playersItemsPools.Count; i++)
            {
                availableLocations.Add((i, 0));
            }

            while (availableLocations.Count > 0)
            {
                int randomLocationIndex = random.Next(availableLocations.Count);
                (int, int) randomAvailableLocation = availableLocations[randomLocationIndex];
                availableLocations.RemoveAt(randomLocationIndex);

                int playerIndex;
                (int, string, string) item;
                (playerIndex, item) = getRandomPlayerItem(unplacedItemsPools);

                SetItemAtLocation(randomAvailableLocation, item, playerIndex, itemOrder);
                itemOrder++;

                if (unplacedItemsPools[playerIndex].Count > 0)
                {
                    int itemIndex = playersItemsPools[playerIndex].ItemsPool.Length - unplacedItemsPools[playerIndex].Count;
                    availableLocations.Add((playerIndex, itemIndex));
                }
            }
        }

        private (int, (int, string, string)) getRandomPlayerItem(Queue<(int, string, string)>[] unplacedItemsPools)
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

        private void SetItemAtLocation((int player, int itemIndex) location, (int, string, string) newItem, int playerGivenItem, int itemOrder)
        {
            if (location.player != playerGivenItem)
                LanguageStringManager.SetItemName(ref newItem, LanguageStringManager.AddPlayerId(newItem, playerGivenItem));
            playersItemsPools[location.player].ItemsPool[location.itemIndex].Item2 = newItem.Item2;
            playersItemsPools[location.player].ItemsPool[location.itemIndex].Item1 = itemOrder;
        }
    }
}