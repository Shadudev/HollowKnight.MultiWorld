using MultiWorldLib;
using System;
using System.Collections.Generic;

namespace MultiWorldServer.ItemsRandomizers
{
    internal class SimpleItemsRandomizer : IItemsRandomizer
    {
        public SimpleItemsRandomizer(List<PlayerItemsPool> playersItemsPools, MultiWorldSettings settings) :
            base(playersItemsPools, settings) { }

        public override List<PlayerItemsPool> Randomize()
        {
            RandomizeItemsPools();
            return playersItemsPools;
        }

        public override Placement[] GetPlayerItems(int playerId)
        {
            return playersItemsPools[playerId].Placements;
        }

        private void RandomizeItemsPools()
        {
            Queue<Item>[] unplacedItems = GetPlayersItems(playersItemsPools);
            List<Location> availableLocations = new List<Location>();
            
            while (availableLocations.Count > 0)
            {
                Item item = GetRandomPlayerItem(unplacedItems);
                bool valid = PopRandomLocation(availableLocations, out Location location);
                if (!valid) continue;

                SetItemAtLocation(item, location);

                unplacedItems[item.OwnerID].Dequeue();
                totalItemsAmount--;

                if (unplacedItems[item.OwnerID].Count > 0)
                    AddNewAvailableLocation(unplacedItems, availableLocations, item.OwnerID);
            }
        }

        private Queue<Item>[] GetPlayersItems(List<PlayerItemsPool> playersItemsPools)
        {
            Queue<Item>[] playersItems = new Queue<Item>[playersItemsPools.Count];
            for (int i = 0; i < playersItems.Length; i++)
            {
                playersItems[i] = new Queue<Item>();
                Array.ForEach(playersItemsPools[i].Placements, placement => playersItems[i].Enqueue(placement.Item));
            }

            return playersItems;
        }

        private Item GetRandomPlayerItem(Queue<Item>[] unplacedItems)
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

            return unplacedItems[playerIndex].Peek();
        }

        private bool PopRandomLocation(List<Location> availableLocations, out Location location)
        {
            int randomLocationIndex = random.Next(availableLocations.Count);
            location = availableLocations[randomLocationIndex];

            availableLocations.RemoveAt(randomLocationIndex);
            return true;
        }

        private void SetItemAtLocation(Item item, Location location)
        {
            playersItemsPools[location.OwnerID].Placements[location.Index].Item = item;
        }
        
        private void AddNewAvailableLocation(Queue<Item>[] unplacedItems, List<Location> availableLocations, int playerId)
        {
            int nextItemIndex = playersItemsPools[playerId].Placements.Length - unplacedItems[playerId].Count;
            availableLocations.Add(playersItemsPools[playerId].Placements[nextItemIndex].Location);
        }
    }
}
