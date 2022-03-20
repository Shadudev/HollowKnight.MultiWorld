using System;

namespace MultiWorldMod
{
    class CharmNotchCostsObserver
    {
        internal static int[] GetCharmNotchCosts()
        {
            // TODO check if ItemChanger provides a nicer way to read the notch costs
            int[] costs = new int[40];
            for (int i = 0; i < costs.Length; i++)
                costs[i] = PlayerData.instance.GetInt($"charmCost_{i + 1}");
            return costs;
        }
    }
}
