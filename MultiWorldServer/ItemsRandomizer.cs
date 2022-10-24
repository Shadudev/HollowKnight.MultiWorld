using MultiWorldLib;
using MultiWorldLib.MultiWorldSettings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MultiWorldServer
{
    internal class ItemsRandomizer
    {
        private readonly Random random;

        // Provided Data
        private readonly List<PlayerItemsPool> playersItemsPools;
        private readonly MultiWorldGenerationSettings settings;

        // Generated Data
        private readonly List<Dictionary<string, List<(int, int)>>> playersItemsLocations;
        public string FullOrderedItemsLog;

        public ItemsRandomizer(List<PlayerItemsPool> playersItemsPools, MultiWorldGenerationSettings settings)
        {
            // Done for consistency purposes
            playersItemsPools.Sort((pool1, pool2) => pool1.RandoHash - pool2.RandoHash);

            this.playersItemsPools = playersItemsPools;
            this.settings = settings;
            
            playersItemsLocations = new List<Dictionary<string, List<(int, int)>>>();
            for (int i = 0; i < this.playersItemsPools.Count; i++)
            {
                this.playersItemsPools[i].PlayerId = i;
                playersItemsLocations.Add(new Dictionary<string, List<(int, int)>>());
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

        internal Dictionary<string, (string, string)[]> GetPlayerItems(int playerId)
        {
            Dictionary<string, (string, string)[]> playerItems = new Dictionary<string, (string, string)[]>();
            foreach (string group in playersItemsLocations[playerId].Keys)
            {
                List<(string, string)> items = new List<(string, string)>();
                foreach ((int player, int itemIndex) in playersItemsLocations[playerId][group])
                    items.Add(playersItemsPools[player].ItemsPool[group][itemIndex]);
                playerItems[group] = items.ToArray();
            }

            return playerItems;
        }

        private void RandomizeItemsPools()
        {
            HashSet<string> iteratedGroups = new HashSet<string>();
            foreach (var playerItemsPool in playersItemsPools)
            {
                foreach (string group in playerItemsPool.ItemsPool.Keys) 
                {
                    if (iteratedGroups.Contains(group)) continue;

                    iteratedGroups.Add(group);
                    RandomizeItemsGroup(group);
                }
            }
        }

        private void RandomizeItemsGroup(string group)
        {
            Queue<string>[] unplacedItems = GetPlayersItemsFromGroup(group);
            List<(int player, int locationIndex)> availableLocations = new List<(int, int)>();
            int totalItemsAmount = 0;

            for (int i = 0; i < playersItemsPools.Count; i++)
            {
                if (playersItemsPools[i].ItemsPool.TryGetValue(group, out var playerItemGroup) && 
                    playerItemGroup.Length > 0)
                {
                    availableLocations.Add((i, 0));
                    totalItemsAmount += playerItemGroup.Length;
                }
            }

            while (availableLocations.Count > 0)
            {
                (int itemOwnerId, string item) = GetRandomPlayerItem(unplacedItems, totalItemsAmount);
                (int, int) location = PopRandomLocation(availableLocations);

                SetItemAtLocation(location, item, itemOwnerId, group);

                unplacedItems[itemOwnerId].Dequeue();
                totalItemsAmount--;

                if (unplacedItems[itemOwnerId].Count > 0)
                    AddNewAvailableLocation(unplacedItems, availableLocations, itemOwnerId, group);
            }
        }

        private Queue<string>[] GetPlayersItemsFromGroup(string group)
        {
            Queue<string>[] playersItems = new Queue<string>[playersItemsPools.Count];
            for (int i = 0; i < playersItems.Length; i++)
            {
                playersItems[i] = new Queue<string>();
                if (playersItemsPools[i].ItemsPool.ContainsKey(group))
                    Array.ForEach(playersItemsPools[i].ItemsPool[group],
                        placement => playersItems[i].Enqueue(placement.Item1));
            }

            return playersItems;
        }

        private (int, string) GetRandomPlayerItem(Queue<string>[] unplacedItems, int totalItemsAmount)
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

        private void SetItemAtLocation((int player, int itemIndex) location, string item, int playerGivenItem, string group)
        {
            string mwItem = LanguageStringManager.AddPlayerId(item, playerGivenItem);
            playersItemsPools[location.player].ItemsPool[group][location.itemIndex].Item1 = mwItem;

            playersItemsLocations[playerGivenItem].GetOrCreateDefault(group).Add(location);

            FullOrderedItemsLog += $"{playersItemsPools[playerGivenItem].Nickname}'s {item} -> " +
                $"{playersItemsPools[location.player].Nickname}'s {playersItemsPools[location.player].ItemsPool[group][location.itemIndex].Item2}" +
                $"{Environment.NewLine}";
        }
        
        private void AddNewAvailableLocation(Queue<string>[] unplacedItems, List<(int, int)> availableLocations, int playerIndex, string group)
        {
            int itemIndex = playersItemsPools[playerIndex].ItemsPool[group].Length - unplacedItems[playerIndex].Count;
            availableLocations.Add((playerIndex, itemIndex));
        }

        internal string GetGenerationHash()
        {
            using (SHA256Managed sHA256Managed = new SHA256Managed())
            using (StringWriter stringWriter = new StringWriter())
            {
                JsonSerializer jsonSerializer = new JsonSerializer
                {
                    DefaultValueHandling = DefaultValueHandling.Include,
                    Formatting = Formatting.None,
                    TypeNameHandling = TypeNameHandling.Auto
                };
                jsonSerializer.Serialize(stringWriter, playersItemsLocations);

                StringBuilder stringBuilder = stringWriter.GetStringBuilder();
                stringBuilder.Replace("\r", string.Empty);
                stringBuilder.Replace("\n", string.Empty);
                byte[] array = sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));

                int hash = 17;
                for (int i = 0; i < array.Length; i++)
                    hash = ((31 * hash) ^ array[i]);

                return hash.ToString("X");
            }
        }
    }
}