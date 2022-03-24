using ItemChanger;
using MultiWorldLib;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer
{
    // Logic here is based on RandomizerMod.Settings.TrackerData.Setup
    internal class OrderedItemPlacements
    {
        public static (string, string)[] Get(RandoController rc)
        {
            ProgressionManager pm = new(rc.ctx.LM, rc.ctx);
            MainUpdater mu = InitializeUpdater(rc);

            List<OrderedItemUpdateEntry> entries = new();
            List<GeneralizedPlacement> orderedItemPlacements = new();
            foreach (GeneralizedPlacement p in rc.ctx.itemPlacements)
            {
                OrderedItemUpdateEntry e = new(p, p.Location.Name != LocationNames.Start ? orderedItemPlacements.Add : (s) => { });
                entries.Add(e);
                mu.AddEntry(e);
            }

            mu.Hook(pm);

            int itemIncrementalId = 1;
            return orderedItemPlacements.Select(itemPlacement =>
                (LanguageStringManager.AddItemId(itemPlacement.Item.Name, itemIncrementalId++), itemPlacement.Location.Name)).ToArray();
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
