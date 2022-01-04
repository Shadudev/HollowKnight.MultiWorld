using System;

namespace MultiWorldMod
{
    class CharmNotchCostsObserver
    {
        private static bool isLogicDone = false;

        internal static bool IsRandomizationLogicDone()
        {
            if (!RandomizerMod.RandomizerMod.RS.GenerationSettings.MiscSettings.RandomizeNotchCosts) return true;

            return isLogicDone;
        }

        internal static int[] GetCharmNotchCosts()
        {
            int[] costs = new int[40];
            for (int i = 0; i < costs.Length; i++)
                costs[i] = PlayerData.instance.GetInt($"charmCost_{i + 1}");
            return costs;
        }

        internal static void SetCharmNotchCostsLogicDone()
        {
            isLogicDone = true;
        }

        internal static void ResetLogicDoneFlag()
        {
            isLogicDone = false;
        }
    }
}
