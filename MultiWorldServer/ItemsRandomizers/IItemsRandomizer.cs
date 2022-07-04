using MultiWorldLib;
using System;
using System.Collections.Generic;

namespace MultiWorldServer.ItemsRandomizers
{
    internal abstract class IItemsRandomizer
    {
        protected List<PlayerItemsPool> playersItemsPools;
        protected int totalItemsAmount;
        protected Random random;

        protected IItemsRandomizer(List<PlayerItemsPool> playersItemsPools, MultiWorldSettings multiWorldSettings)
        {
            this.playersItemsPools = playersItemsPools;
            // This allows for consistent randomization using seed, settings and who sent first? *work by ready order
            this.playersItemsPools.Sort((p1, p2) => p1.Placements.Length - p2.Placements.Length);

            random = new Random(multiWorldSettings.Seed);

            totalItemsAmount = 0;
            playersItemsPools.ForEach(p => totalItemsAmount += p.Placements.Length);
        }

        public abstract List<PlayerItemsPool> Randomize();
        public abstract Placement[] GetPlayerItems(int playerId);
    }
}
