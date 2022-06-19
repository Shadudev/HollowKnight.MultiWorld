using ItemChanger;
using ItemSyncMod.Items;
using ItemSyncMod.Items.DisplayMessageFormatter;

namespace ItemSyncMod.SyncFeatures.SimpleKeysUsages
{
    public enum SimpleKeyUsageLocation
    {
        Waterways = 0,
        Jiji,
        PleasureHouse,
        Godhome,
    }

    internal class SimpleKeysUsages
    {
        internal static void AddDoorsUnlockPlacements(HashSet<string> existingItemIds)
        {
            ItemChangerMod.AddPlacements(new AbstractPlacement[]
            {
                GeneratetDoorUnlockPlacement(existingItemIds, SimpleKeyUsageLocation.Waterways),
                GeneratetDoorUnlockPlacement(existingItemIds, SimpleKeyUsageLocation.Jiji),
                GeneratetDoorUnlockPlacement(existingItemIds, SimpleKeyUsageLocation.PleasureHouse),
                GeneratetDoorUnlockPlacement(existingItemIds, SimpleKeyUsageLocation.Godhome)
            }, PlacementConflictResolution.MergeKeepingOld);
        }

        private static AbstractPlacement GeneratetDoorUnlockPlacement(HashSet<string> existingItemIds, SimpleKeyUsageLocation location)
        {
            AbstractPlacement placement = DoorUnlockLocation.New(location).Wrap();
            DoorUnlockItem item = DoorUnlockItem.New(location);
            ItemManager.AddSyncedTag(existingItemIds, placement, item);
            item.GetTag<SyncedItemTag>().Formatter = new DoorUnlockedFormatter();
            placement.Items.Add(item);
            return placement;
        }
    }
}
