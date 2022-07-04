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
        internal static void AddDoorsUnlockPlacements(ref int globalItemID)
        {
            ItemChangerMod.AddPlacements(new AbstractPlacement[]
            {
                GeneratetDoorUnlockPlacement(SimpleKeyUsageLocation.Waterways, ref globalItemID),
                GeneratetDoorUnlockPlacement(SimpleKeyUsageLocation.Jiji, ref globalItemID),
                GeneratetDoorUnlockPlacement(SimpleKeyUsageLocation.PleasureHouse, ref globalItemID),
                GeneratetDoorUnlockPlacement(SimpleKeyUsageLocation.Godhome, ref globalItemID)
            }, PlacementConflictResolution.MergeKeepingOld);
        }

        private static AbstractPlacement GeneratetDoorUnlockPlacement(SimpleKeyUsageLocation location, ref int globalItemID)
        {
            AbstractPlacement placement = DoorUnlockLocation.New(location).Wrap();
            DoorUnlockItem item = DoorUnlockItem.New(location);
            ItemManager.AddSyncedTag(placement, item, ref globalItemID);
            item.GetTag<SyncedItemTag>().Formatter = new DoorUnlockedFormatter();
            placement.Items.Add(item);
            return placement;
        }
    }
}
