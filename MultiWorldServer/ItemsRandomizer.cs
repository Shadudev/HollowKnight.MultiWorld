using MultiWorldLib;
using System;
using System.Collections.Generic;

namespace MultiWorldServer
{
    internal class ItemsRandomizer
    {
        private List<(int, string, string)[]> playersItems;
        private List<string> nicknames;

        public ItemsRandomizer(List<(int, string, string)[]> playersItems, List<string> nicknames)
        {
            this.playersItems = playersItems;
            this.nicknames = nicknames;
        }

        internal List<RandoResult> RandomizeItems()
        {
            List<(int, string, string)[]> itemsPools = new List<(int, string, string)[]>();
            /* Base on this code
             * make results persistent given initiator's seed?
             * making the items list ignore the order it was received by and work by amount of items.
             * static Random rand = new Random(Guid.NewGuid().GetHashCode());

        static void Main1(string[] args)
        {
            (int, string, string)[] itemsPool1 = { (1, "a", "a"), (2, "b", "b"), (3, "c", "c"), (4, "d", "d") };
            (int, string, string)[] itemsPool2 = { (1, "A", "A"), (2, "B", "B"), (3, "C", "C"), (4, "D", "D"), (5, "E", "E"), (6, "F", "F") };
            (int, string, string)[] itemsPool3 = { (1, "1", "1"), (2, "2", "2"), (3, "3", "3"), (4, "4", "4"), (5, "5", "5") };

            List<(int, string, string)[]> itemsPools = new List<(int, string, string)[]>() { itemsPool1, itemsPool2, itemsPool3 };

            PrintItemsPools(itemsPools);

            RandomizeItemsPools(itemsPools);

            PrintItemsPools(itemsPools);
        }

        static void PrintItemsPools(List<(int, string, string)[]> itemsPool)
        {
            int index = 0;
            bool exists;
            Console.WriteLine("P1\tP2\tP3");
            do
            {
                exists = false;
                foreach (var itemPool in itemsPool)
                {
                    if (index < itemPool.Length)
                    {
                        exists = true;
                        Console.Write(itemPool[index].Item2);
                    }
                    Console.Write("\t");
                }
                Console.WriteLine();
                index++;
            } while (exists);
        }

        static void RandomizeItemsPools(List<(int, string, string)[]> itemsPool)
        {
            Queue<(int, string, string)>[] unplacedItemsPools = new Queue<(int, string, string)>[itemsPool.Count];
            for (int i = 0; i < unplacedItemsPools.Length; i++)
            {
                unplacedItemsPools[i] = new Queue<(int, string, string)>(itemsPool[i]);
            }

            List<(int, int)> availableLocations = new List<(int, int)>();
            for (int i = 0; i < itemsPool.Count; i++)
            {
                availableLocations.Add((i, 0));
            }

            while (availableLocations.Count > 0)
            {
                int randomLocationIndex = rand.Next(availableLocations.Count);
                (int, int) randomAvailableLocation = availableLocations[randomLocationIndex];
                availableLocations.RemoveAt(randomLocationIndex);

                int playerIndex;
                (int, string, string) item;
                (playerIndex, item) = getRandomPlayerItem(unplacedItemsPools);

                itemsPool[randomAvailableLocation.Item1][randomAvailableLocation.Item2] = item;

                if (itemsPool[playerIndex].Length > item.Item1)
                {
                    availableLocations.Add((playerIndex, item.Item1));
                }
            }
        }

        static (int, (int, string, string)) getRandomPlayerItem(Queue<(int, string, string)>[] unplacedItemsPools)
        {
            int sum = 0;
            foreach (var unplacedItemsPool in unplacedItemsPools)
            {
                sum += unplacedItemsPool.Count;
            }

            int number = rand.Next(sum);

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
             */
            return new List<RandoResult>(); // TODO Fix?
        }
    }
}