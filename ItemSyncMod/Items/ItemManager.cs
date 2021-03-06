using ItemChanger;
using ItemChanger.Placements;
using ItemChanger.Tags;
using MultiWorldLib.Messaging.Definitions.Messages;
using RandomizerMod.IC;

namespace ItemSyncMod.Items
{
    public class ItemManager
    {
        private static readonly string PLACEMENT_ITEM_SEPERATOR = ";";

        public class ItemReceivedEvent
        {
            public string ItemId { get; set; }
            public string From { get; set; }
            public bool Handled { get; set; }
        }
        public static Action<ItemReceivedEvent> OnItemReceived;

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
            AbstractPlacement.OnVisitStateChangedGlobal += SyncPlacementVisitStateChanged;
        }

        internal static void UnsubscribeEvents()
        {
            AbstractPlacement.OnVisitStateChangedGlobal -= SyncPlacementVisitStateChanged;
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

        internal static bool TryGiveItem(string itemId, string from)
        {
            LogHelper.LogDebug($"{itemId} from {from}");

            ItemReceivedEvent itemReceivedEvent = new() { ItemId = itemId, From = from, Handled = false };
            InvokeItemReceived(itemReceivedEvent);
            if (itemReceivedEvent.Handled) return true;

            foreach (AbstractItem item in ItemChanger.Internal.Ref.Settings.GetItems())
            { 
                if (item.GetTag(out SyncedItemTag tag) && tag.ItemID == itemId)
                {
                    tag.GiveThisItem(from);
                    return true;
                }
            }
            return false;
        }

        private static void InvokeItemReceived(ItemReceivedEvent itemReceivedEvent)
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

        internal static void PlacementVisitChanged(MWVisitStateChangedMessage placementVisitChanged)
        {
            AbstractPlacement placement = ItemChanger.Internal.Ref.Settings.GetPlacements().
                First(placement => placement.Name == placementVisitChanged.Name);

            switch (placementVisitChanged.PreviewRecordTagType)
            {
                case PreviewRecordTagType.Single:
                    placement.GetOrAddTag<PreviewRecordTag>().previewText = 
                        placementVisitChanged.PreviewTexts[0];
                    break;
                case PreviewRecordTagType.Multi:
                    placement.GetOrAddTag<MultiPreviewRecordTag>().previewTexts = 
                        placementVisitChanged.PreviewTexts;
                    break;
            }
            
            if (!placement.CheckVisitedAll(placementVisitChanged.NewVisitFlags))
            {
                placement.AddTag<SyncedVisitStateTag>().Change = placementVisitChanged.NewVisitFlags;
                placement.AddVisitFlag(placementVisitChanged.NewVisitFlags);
            }
        }

        private static void SyncPlacementVisitStateChanged(VisitStateChangedEventArgs args)
        {
            if (args.NoChange || IsStartLocation(args.Placement)) return;

            if (args.Placement.GetTag(out SyncedVisitStateTag visitStateTag) && args.NewFlags == visitStateTag.Change) 
            {
                args.Placement.RemoveTags<SyncedVisitStateTag>();
            }
            else if (args.Placement.GetTag(out PreviewRecordTag tag))
            {
                ItemSyncMod.ISSettings.AddSentVisitChange(args.Placement.Name, new string[] { tag.previewText }, PreviewRecordTagType.Single, args.NewFlags);
                ItemSyncMod.Connection.SendVisitStateChanged(args.Placement.Name, new string[] { tag.previewText }, PreviewRecordTagType.Single, args.NewFlags);
            }
            else if (args.Placement.GetTag(out MultiPreviewRecordTag tag2))
            {
                ItemSyncMod.ISSettings.AddSentVisitChange(args.Placement.Name, tag2.previewTexts, PreviewRecordTagType.Multi, args.NewFlags);
                ItemSyncMod.Connection.SendVisitStateChanged(args.Placement.Name, tag2.previewTexts, PreviewRecordTagType.Multi, args.NewFlags);
            } 
            else 
            {
                ItemSyncMod.ISSettings.AddSentVisitChange(args.Placement.Name, new string[] { "" }, PreviewRecordTagType.None, args.NewFlags);
                ItemSyncMod.Connection.SendVisitStateChanged(args.Placement.Name, new string[] { "" }, PreviewRecordTagType.None, args.NewFlags);
            }
        }

        internal static bool IsStartLocation(AbstractPlacement placement)
        {
            return placement is IPrimaryLocationPlacement locpmt && 
                locpmt.Location is ItemChanger.Locations.StartLocation;
        }
    }
}
