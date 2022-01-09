using ItemChanger;

namespace ItemSyncMod.Items
{
    class ItemManager
    {
        private static readonly string PLACEMENT_ITEM_SEPERATOR = ";";
        internal static string GenerateItemId(AbstractPlacement placement, AbstractItem randoItem)
        {
            return $"{placement.Name}{PLACEMENT_ITEM_SEPERATOR}{randoItem.name}";
        }

        internal static AbstractPlacement GetItemPlacement(string itemId)
        {
            string placementName = itemId.Substring(0, itemId.IndexOf(PLACEMENT_ITEM_SEPERATOR));
            return ItemChanger.Internal.Ref.Settings.GetPlacements().
                Where(placement => placement.Name == placementName).First();
        }

        internal static bool ShouldItemBeIgnored(string itemID)
        {
            // Drop start items
            return itemID.StartsWith("Start;");
        }

        internal static GiveInfo GetItemSyncStandardGiveInfo()
        {
            return new GiveInfo()
            {
                Container = "ItemSync",
                FlingType = FlingType.DirectDeposit,
                MessageType = MessageType.Corner,
                Transform = null,
                Callback = null
            };
        }

        internal static void GiveItem(string itemId)
        {
            foreach (AbstractItem item in ItemChanger.Internal.Ref.Settings.GetItems())
                if (item.GetTag(out SyncedItemTag tag) && tag.ItemID == itemId && !tag.Given)
                {
                    tag.GiveThisItem();
                    break;
                }
        }
    }
}
