using MultiWorldLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWorldServer.ItemsRandomizers.OnlyOthersItemsRandomizers
{
    /*
     * Given a case of 3 players, A=300; B=240; C=180 items
     * Distribution will be:
     * A's world: 189(B) + 111(C)
     * B's world: 171(A) + 69(C)
     * C's world: 129(A) + 51(B)
     */
    internal class LeastFillersRandomizer : IItemsRandomizer
    {
        public LeastFillersRandomizer(List<PlayerItemsPool> playersItemsPools, MultiWorldSettings settings) :
            base(playersItemsPools, settings) { }

        public override Placement[] GetPlayerItems(int playerId) => playersItemsPools[playerId].Placements;

        public override List<PlayerItemsPool> Randomize()
        {
            // Calculate items distributions, player's items per world.
            // Usage: playersItemsPerWorld[player id] has a list of items from different players to distribute.
            int[,] playersItemsPerWorld = CalculateItemsDistributions();

            // Collect unplaced items, initially available locations
            Queue<Item>[] unplacedItems = GetPlayersItems();
            List<Location> availableLocations = new List<Location>();

            // Loop till done
            // Randomize location (don't remove location from availableLocations)
            // Calculate how many items (0-X) to place in the player's location, calculate max given the remaining locations and extras/fillers
            // If 0 - make sure placing a filler item is valid:
            //      availableLocations should include at least one location of an other player.
            //      If valid - place random filler item (random player's 1 geo/essence/nothing, TODO add code to handle these crafted items)
            //      Else change items count to 1.
            // Per item, place random player's unplaced item and unlock its original location.

            // TODO, try to prevent fillers within the early 20% placements (define as threshold)

            return playersItemsPools;
        }

        private int[,] CalculateItemsDistributions()
        {
            int[] playersItems = playersItemsPools.Select(pool => pool.Placements.Length).ToArray();
            int[,] playersItemsPerWorld = new int[playersItemsPools.Count, playersItemsPools.Count];
            for (int itemsOwnerId = 0; itemsOwnerId < playersItemsPools.Count - 1; itemsOwnerId++)
            {
                int playerItemsAmount = playersItemsPools[itemsOwnerId].Placements.Length;
                int totalItemsOnOtherWorlds = totalItemsAmount - playerItemsAmount;
                double playerItemsPortion = 1.0 * playerItemsAmount / totalItemsOnOtherWorlds;

                int worldId = itemsOwnerId + 1;
                for (; worldId < playersItemsPools.Count - 1; worldId++)
                {
                    int distributedPlayerItems = (int)(playersItemsPools[worldId].Placements.Length * playerItemsPortion);
                    playersItemsPerWorld[worldId, itemsOwnerId] = distributedPlayerItems;
                    playersItemsPerWorld[itemsOwnerId, worldId] = distributedPlayerItems;
                    playersItems[itemsOwnerId] -= distributedPlayerItems;
                }

                int itemsToTakeFromLastWorld = 
                playersItemsPerWorld[worldId, itemsOwnerId] = playersItems[itemsOwnerId];

                // Fill the rest of the items from the last player
                // If items are not sufficent, go back through the list to fill items from.
                // For cases where one player has more items than the rest of the players, loop will finish with an uncomplete pool.
                // Put fillers to fill it
            }

            // Fill the last player with the leftover items from all the players

            return playersItemsPerWorld;
        }

        private Queue<Item>[] GetPlayersItems()
        {
            Queue<Item>[] playersItems = new Queue<Item>[playersItemsPools.Count];
            for (int i = 0; i < playersItems.Length; i++)
            {
                playersItems[i] = new Queue<Item>();
                Array.ForEach(playersItemsPools[i].Placements, placement => playersItems[i].Enqueue(placement.Item));
            }

            return playersItems;
        }
    }
}
