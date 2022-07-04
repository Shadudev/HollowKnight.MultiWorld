using MultiWorldLib;
using MultiWorldServer.ItemsRandomizers.OnlyOthersItemsRandomizers;
using System.Collections.Generic;

namespace MultiWorldServer.ItemsRandomizers
{
    internal class ItemsRandomizersCollection
    {
        public static IItemsRandomizer Get(List<PlayerItemsPool> playersItemsPools, MultiWorldSettings settings)
        {
            switch (settings.RandomizationAlgorithm)
            {
                case RandomizationAlgorithm.OnlyOthersItemsLeastFillers: 
                    return new LeastFillersRandomizer(playersItemsPools, settings);
                case RandomizationAlgorithm.Balanced:
                    return new BalancedRandomizer(playersItemsPools, settings);
                case RandomizationAlgorithm.Default:
                default:
                    return new SimpleItemsRandomizer(playersItemsPools, settings);

            }
        }
    }
}
