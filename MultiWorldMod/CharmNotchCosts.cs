namespace MultiWorldMod
{
    class CharmNotchCosts
    {
        internal static int[] Get()
        {
            int[] costs = new int[40];
            for (int i = 0; i < costs.Length; i++)
                costs[i] = PlayerData.instance.GetInt($"charmCost_{i + 1}");

            return costs;
        }
    }
}
