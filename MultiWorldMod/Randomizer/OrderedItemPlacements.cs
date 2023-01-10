using ItemChanger;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer
{
    // Logic here is based on RandomizerMod.Settings.TrackerData.Setup
    internal class OrderedItemPlacements
    {
        public static Dictionary<string, List<GeneralizedPlacement>> Get(RandoController rc)
        {
            ProgressionManager pm = new(rc.ctx.LM, rc.ctx);
            MainUpdater mu = InitializeUpdater(rc, pm);

            List<OrderedItemUpdateEntry> entries = new();
            Dictionary<string, List<GeneralizedPlacement>> orderedItemPlacements = new();
            foreach (GeneralizedPlacement p in rc.ctx.itemPlacements)
            {
                OrderedItemUpdateEntry e = new(p, p => AddPlacementToMultiWorldRando(p, orderedItemPlacements, rc));
                entries.Add(e);
                mu.AddEntry(e);
            }

            mu.StartUpdating();

            return orderedItemPlacements;
        }

        private static MainUpdater InitializeUpdater(RandoController rc, ProgressionManager pm)
        {
            MainUpdater mu = pm.mu;
            mu.AddWaypoints(rc.ctx.LM.Waypoints);
            mu.AddPlacements(rc.ctx.Vanilla);

            if (rc.ctx.transitionPlacements is not null)
                mu.AddEntries(rc.ctx.transitionPlacements.Select(t => new PrePlacedItemUpdateEntry(t)));
            return mu;
        }

        private static void AddPlacementToMultiWorldRando(GeneralizedPlacement placement, Dictionary<string, List<GeneralizedPlacement>> orderedItemPlacements, RandoController rc)
        {
            if (!IsPlacementExcludedFromMultiWorld(placement, rc, out string itemGroup))
            {
                if (!orderedItemPlacements.ContainsKey(itemGroup))
                    orderedItemPlacements[itemGroup] = new List<GeneralizedPlacement>();

                orderedItemPlacements[itemGroup].Add(placement);
            }
        }

        private static bool IsPlacementExcludedFromMultiWorld(GeneralizedPlacement placement, RandoController rc, out string itemGroup)
        {
            itemGroup = null;
            if (placement.Location.Name == LocationNames.Start) return true;
            if (!IsItemInIncludedGroup(placement, rc, out itemGroup)) return true;

            return false;
        }

        private static bool IsItemInIncludedGroup(GeneralizedPlacement placement, RandoController rc, out string itemGroup)
        {
            for (int i = 0; i < rc.randomizer.stages.Length; i++)
            {
                foreach (var group in rc.randomizer.stages[i].groups.Where(
                    group => MultiWorldMod.Controller.IncludedGroupsLabels.Contains(group.Label)))
                {
                    if (group.Items.Any(item => item.Name == placement.Item.Name))
                    {
                        itemGroup = group.Label;
                        return true;
                    }
                }
            }

            itemGroup = null;
            return false;
        }
    }
}
