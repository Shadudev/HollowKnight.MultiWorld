using ItemChanger;
using ItemChanger.Placements;
using MultiWorldLib;
using RandomizerMod.IC;

namespace ItemSyncMod.Items
{
    public class ItemManager
    {
        private static readonly string PLACEMENT_ITEM_SEPERATOR = ";";

        public static Action<DataReceivedEvent> OnItemReceived;

        internal static string GenerateUniqueItemId(AbstractPlacement placement, AbstractItem randoItem, HashSet<string> existingItemIds)
        {
            string itemId = $"{placement.Name}{PLACEMENT_ITEM_SEPERATOR}{randoItem.name}";
            int i = 2;
            while (existingItemIds.Contains(itemId))
                itemId = $"{placement.Name}{PLACEMENT_ITEM_SEPERATOR}{randoItem.name}{i++}";

            return itemId;
        }

        internal static AbstractPlacement GetItemPlacement(string itemId)
        {
            string placementName = itemId.Substring(0, itemId.IndexOf(PLACEMENT_ITEM_SEPERATOR));
            return ItemChanger.Internal.Ref.Settings.GetPlacements().
                Where(placement => placement.Name == placementName).First();
        }

        internal static void AddSyncedTags(HashSet<string> existingItemIds, bool shouldSyncVanillaItems)
        {
            foreach (AbstractPlacement placement in ItemChanger.Internal.Ref.Settings.GetPlacements())
            {
                if (!IsStartLocation(placement))
                {
                    foreach (AbstractItem item in placement.Items)
                    {
                        if (item.HasTag<RandoItemTag>() || shouldSyncVanillaItems)
                        {
                            AddSyncedTag(existingItemIds, placement, item);
                        }
                    }
                }
            }
        }

        public static void AddSyncedTag(HashSet<string> existingItemIds, AbstractPlacement placement, AbstractItem item)
        {
            string itemId = GenerateUniqueItemId(placement, item, existingItemIds);
            existingItemIds.Add(itemId);
            item.AddTag<SyncedItemTag>().ItemID = itemId;
        }

        internal static void SubscribeEvents()
        {
            ItemSyncMod.Connection.OnDataReceived += TryGiveItem;
        }

        internal static void UnsubscribeEvents()
        {
            ItemSyncMod.Connection.OnDataReceived -= TryGiveItem;
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

        public static void SendItemToAll(string item)
        {
            LogHelper.LogDebug("Sending " + item);
            ItemSyncMod.Connection.SendDataToAll(Consts.ITEMSYNC_ITEM_MESSAGE_LABEL, item);
        }

        internal static void TryGiveItem(DataReceivedEvent dataReceivedEvent)
        {
            if (dataReceivedEvent.Label != Consts.ITEMSYNC_ITEM_MESSAGE_LABEL) return;

            string itemId = dataReceivedEvent.Content, from = dataReceivedEvent.From;
            LogHelper.LogDebug($"Received {itemId} from {from}");

            InvokeItemReceived(dataReceivedEvent);
            if (dataReceivedEvent.Handled) return;
            
            foreach (AbstractItem item in ItemChanger.Internal.Ref.Settings.GetItems())
            { 
                if (item.GetTag(out SyncedItemTag tag) && tag.ItemID == itemId)
                {
                    tag.GiveThisItem(from);
                    dataReceivedEvent.Handled = true;
                    return;
                }
            }
        }

        private static void InvokeItemReceived(DataReceivedEvent itemReceivedEvent)
        {
            try
            {
                OnItemReceived?.Invoke(itemReceivedEvent);
            }
            catch (Exception ex)
            {
                LogHelper.LogError("OnItemReceived threw an exception, " + ex.Message);
                LogHelper.LogError(ex.StackTrace);
            }
        }

        public static bool IsStartLocation(AbstractPlacement placement)
        {
            return placement is IPrimaryLocationPlacement locpmt && 
                locpmt.Location is ItemChanger.Locations.StartLocation;
        }
    }
}
