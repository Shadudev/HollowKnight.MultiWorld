using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer
{
    internal class OrderedItemPlacements
    {
        public static (int, string, string)[] Get(RandoController rc)
        {
            ProgressionManager pm = new(rc.ctx.LM, rc.ctx);
            MainUpdater mu = InitializeUpdater(rc);

            List<OrderedItemUpdateEntry> entries = new();
            List<ItemPlacement> orderedItemPlacements = new();
            foreach (ItemPlacement p in rc.ctx.itemPlacements)
            {
                OrderedItemUpdateEntry e = new(p, orderedItemPlacements.Add);
                entries.Add(e);
                mu.AddEntry(e);
            }

            mu.Hook(pm);
            return orderedItemPlacements.Select((itemPlacement, index) =>
                (index, itemPlacement.item.Name, itemPlacement.location.Name)).ToArray();
        }

        private static MainUpdater InitializeUpdater(RandoController rc)
        {
            MainUpdater mu = new(rc.ctx.LM);
            mu.AddPlacements(rc.ctx.LM.Waypoints);
            mu.AddPlacements(rc.ctx.Vanilla);
            if (rc.ctx.transitionPlacements is not null)
                mu.AddPlacements(rc.ctx.transitionPlacements);
            return mu;
        }
    }
}
