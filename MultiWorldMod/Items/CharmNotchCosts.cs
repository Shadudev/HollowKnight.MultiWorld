namespace MultiWorldMod.Items
{
    class CharmNotchCosts
    {
        internal static Dictionary<int, int> Get()
        {
            Dictionary<int, int> costs = new();
            // Collect vanilla charms costs
            for (int i = 1; i <= 40; i++)
                costs[i] = GetCharmCost(i);

            // Collect modded charms costs
            CollectCustomCharms(costs);

            return costs;
        }

        private static void CollectCustomCharms(Dictionary<int, int> costs)
        {
            foreach (ItemChanger.Items.CharmItem charm in 
                ItemChanger.Internal.Ref.Settings.GetItems().Where(item => item is ItemChanger.Items.CharmItem)) {

                if (charm.charmNum > 40 || charm.charmNum < 1)
                    costs[charm.charmNum] = GetCharmCost(charm.charmNum);
            }
        }

        private static int GetCharmCost(int i)
        {
            return PlayerData.instance.GetInt($"charmCost_{i}");
        }
    }
}
