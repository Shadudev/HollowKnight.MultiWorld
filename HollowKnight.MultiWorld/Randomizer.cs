using System;
using System.Collections.Generic;

namespace MWRandomizer_Logic
{
    class Program
    {
        static Random rand = new Random(Guid.NewGuid().GetHashCode());

        static void Main(string[] args)
        {
            (int, string, string)[] itemPool1 = { (1, "a", "a"), (2, "b", "b"), (3, "c", "c"), (4, "d", "d") };
            (int, string, string)[] itemPool2 = { (1, "A", "A"), (2, "B", "B"), (3, "C", "C"), (4, "D", "D"), (5, "E", "E"), (6, "F", "F") };
            (int, string, string)[] itemPool3 = { (1, "1", "1"), (2, "2", "2"), (3, "3", "3"), (4, "4", "4"), (5, "5", "5") };

            List<(int, string, string)[]> itemsPool = new List<(int, string, string)[]>() { itemPool1, itemPool2, itemPool3 };

            PrintItemsPool(itemsPool);

            RandomizeItemsPool(itemsPool);

            PrintItemsPool(itemsPool);
        }

        static void PrintItemsPool(List<(int, string, string)[]> itemsPool)
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

        static void RandomizeItemsPool(List<(int, string, string)[]> itemsPool)
        {
            Queue<(int, (int, string, string))> itemsToDistributePool = new Queue<(int, (int, string, string))>();
            int index = 0;
            bool exists;
            do
            {
                exists = false;
                for (int itemPoolIndex = 0; itemPoolIndex < itemsPool.Count; itemPoolIndex++)
                {
                    (int, string, string)[] itemPool = itemsPool[itemPoolIndex];
                    if (index < itemPool.Length)
                    {
                        exists = true;
                        itemsToDistributePool.Enqueue((itemPoolIndex, itemPool[index]));
                    }
                }
                index++;
            } while (exists);

            List<(int, int)> availableLocations = new List<(int, int)>();
            for (int i = 0; i < itemsPool.Count; i++)
                availableLocations.Add((i, 0));

            while (availableLocations.Count > 0)
            {
                int randomLocationIndex = rand.Next(availableLocations.Count);
                (int, int) randomAvailableLocation = availableLocations[randomLocationIndex];
                availableLocations.RemoveAt(randomLocationIndex);

                int playerIndex;
                (int, string, string) item;
                (playerIndex, item) = itemsToDistributePool.Dequeue();

                itemsPool[randomAvailableLocation.Item1][randomAvailableLocation.Item2] = item;

                if (itemsPool[playerIndex].Length > item.Item1)
                {
                    availableLocations.Add((playerIndex, item.Item1));
                }
            }
        }
    }
}
