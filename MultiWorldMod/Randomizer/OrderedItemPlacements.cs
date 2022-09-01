using ItemChanger;
using MultiWorldLib;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.Randomization;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer
{
    // Logic here is based on RandomizerMod.Settings.TrackerData.Setup
    internal class OrderedItemPlacements
    {
        public static Dictionary<string, List<GeneralizedPlacement>> Get(RandoController rc)
        {
            ProgressionManager pm = new(rc.ctx.LM, rc.ctx);
            MainUpdater mu = InitializeUpdater(rc);

            List<OrderedItemUpdateEntry> entries = new();
            Dictionary<string, List<GeneralizedPlacement>> orderedItemPlacements = new();
            foreach (GeneralizedPlacement p in rc.ctx.itemPlacements)
            {
                OrderedItemUpdateEntry e = new(p, p => AddPlacementToMultiWorldRando(p, orderedItemPlacements, rc));
                entries.Add(e);
                mu.AddEntry(e);
            }

            mu.Hook(pm);

            return orderedItemPlacements;
        }

        private static MainUpdater InitializeUpdater(RandoController rc)
        {
            MainUpdater mu = new(rc.ctx.LM);
            mu.AddPlacements(rc.ctx.LM.Waypoints);
            mu.AddPlacements(rc.ctx.Vanilla);

            //for (int i = 0; i < rc.randomizer.stages.Length; i++)
            //    for (int j = 0; j < rc.randomizer.stages[i].groups.Length; j++)
            //    {
            //        RandomizationGroup group = rc.randomizer.stages[i].groups[j];
            //        LogHelper.LogDebug($"stage {i}, group {j} named {group.Label}, items: {group.Items.Length}," +
            //            $" locations: {group.Locations.Length}");
            //        for (int k = 0; k < group.Items.Length; k++)
            //            LogHelper.LogDebug($"{k}: {group.Items[k].Name} @ {group.Locations[k].Name}");
            //    }

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
