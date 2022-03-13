using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer
{
    // Logic here is based on RandomizerMod.Settings.TrackerData.Setup
    internal class OrderedItemPlacements
    {
        public static (int, string, string)[] Get(RandoController rc)
        {
            ProgressionManager pm = new(rc.ctx.LM, rc.ctx);
            MainUpdater mu = InitializeUpdater(rc);

            List<OrderedItemUpdateEntry> entries = new();
            List<GeneralizedPlacement> orderedItemPlacements = new();
            foreach (GeneralizedPlacement p in rc.ctx.itemPlacements)
            {
                OrderedItemUpdateEntry e = new(p, orderedItemPlacements.Add);
                entries.Add(e);
                mu.AddEntry(e);
            }

            mu.Hook(pm);
            return orderedItemPlacements.Select((itemPlacement, index) =>
                (index, itemPlacement.Item.Name, itemPlacement.Location.Name)).ToArray();
        }

        private static MainUpdater InitializeUpdater(RandoController rc)
        {
            MainUpdater mu = new(rc.ctx.LM);
            mu.AddPlacements(rc.ctx.LM.Waypoints);
            mu.AddPlacements(rc.ctx.Vanilla);
            if (rc.ctx.transitionPlacements is not null)
                mu.AddEntries(rc.ctx.transitionPlacements.Select(t => new PrePlacedItemUpdateEntry(t)));
            return mu;
        }
    }
}
