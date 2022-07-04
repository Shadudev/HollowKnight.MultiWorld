using ItemChanger;
using ItemChanger.Placements;
using ItemChanger.Tags;
using MultiWorldLib;
using MultiWorldLib.Messaging.Definitions.Messages;
using RandomizerMod.IC;

namespace ItemSyncMod.Items
{
    public class ItemManager
    {
        public class ItemReceivedEvent
        {
            public Item Item { get; set; }
            public string From { get; set; }
            public bool Handled { get; set; }
        }
        public static Action<ItemReceivedEvent> OnItemReceived;

        internal static AbstractPlacement GetItemPlacement(string placementName)
        {
            return ItemChanger.Internal.Ref.Settings.GetPlacements().
                Where(placement => placement.Name == placementName).First();
        }

        internal static void AddSyncedTags(bool syncVanillaItems, ref int globalItemID)
        {
            foreach (AbstractPlacement placement in ItemChanger.Internal.Ref.Settings.GetPlacements())
            {
                if (!IsStartLocation(placement))
                {
                    foreach (AbstractItem item in placement.Items)
                    {
                        if (item.HasTag<RandoItemTag>() || syncVanillaItems)
                        {
                            AddSyncedTag(placement, item, ref globalItemID);
                        }
                    }
                }
            }
        }

        public static void AddSyncedTag(AbstractPlacement placement, AbstractItem item, ref int globalItemID)
        {
            var tag = item.AddTag<SyncedItemTag>();
            tag.Item = new Item { OwnerID = Consts.ITEMSYNC_ITEM_ID, Name = item.name, Index = globalItemID++ };
            tag.PlacementName = placement.Name;
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

        internal static bool TryGiveItem(Item receivedItem, string from)
        {
            LogHelper.LogDebug($"{receivedItem.Name} from {from}");

            ItemReceivedEvent itemReceivedEvent = new() { Item = receivedItem, From = from, Handled = false };
            InvokeItemReceived(itemReceivedEvent);
            if (itemReceivedEvent.Handled) return true;

            foreach (AbstractItem item in ItemChanger.Internal.Ref.Settings.GetItems())
            { 
                if (item.GetTag(out SyncedItemTag tag) && tag.Item.Index == receivedItem.Index)
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
