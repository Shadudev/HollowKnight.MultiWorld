﻿using ItemChanger;
using ItemChanger.Tags;
using MultiWorldLib.Messaging.Definitions.Messages;
using RandomizerMod.IC;

namespace ItemSyncMod.Items
{
    public class ItemManager
    {
        private static readonly string PLACEMENT_ITEM_SEPERATOR = ";";

        public static Action<string> OnGiveItem;

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

        internal static void AddVanillaItemsToICPlacements(List<RandomizerCore.GeneralizedPlacement> vanilla)
        {
            List<AbstractPlacement> vanillaPlacements = new();
            foreach (RandomizerCore.GeneralizedPlacement placement in vanilla)
            {

                AbstractPlacement abstractPlacement = Finder.GetLocation(placement.Location.Name)?.Wrap();
                if (abstractPlacement == null) continue;

                AbstractItem item = Finder.GetItem(placement.Item.Name);
                if (item == null) continue;

                abstractPlacement.Add(item);
                LogHelper.LogDebug($"Adding {placement.Item.Name} to {placement.Location.Name}");
                vanillaPlacements.Add(abstractPlacement);
            }

            ItemChangerMod.AddPlacements(vanillaPlacements, PlacementConflictResolution.MergeKeepingOld);
        }

        internal static void AddSyncedTags(bool shouldSyncVanillaItems)
        {
            HashSet<string> existingItemIds = new();
            foreach (AbstractPlacement placement in ItemChanger.Internal.Ref.Settings.GetPlacements())
            {
                foreach (AbstractItem item in placement.Items)
                {
                    if (item.HasTag<RandoItemTag>() || shouldSyncVanillaItems)
                    {
                        string itemId = GenerateUniqueItemId(placement, item, existingItemIds);
                        existingItemIds.Add(itemId);
                        item.AddTag<SyncedItemTag>().ItemID = itemId;
                    }
                }
            }
        }

        internal static void SubscribeEvents()
        {
            AbstractPlacement.OnVisitStateChangedGlobal += SyncPlacementVisitStateChanged;
        }

        internal static void UnsubscribeEvents()
        {
            AbstractPlacement.OnVisitStateChangedGlobal -= SyncPlacementVisitStateChanged;
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

        internal static void GiveItem(string itemId, string from)
        {
            foreach (AbstractItem item in ItemChanger.Internal.Ref.Settings.GetItems())
            {
                if (item.GetTag(out SyncedItemTag tag) && tag.ItemID == itemId)
                {
                    tag.GiveThisItem(from);
                    break;
                }
            }
            OnGiveItem?.Invoke(itemId);
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
            if (args.NoChange)
            {
                return;
            }

            if (args.Placement.GetTag(out SyncedVisitStateTag visitStateTag) && args.NewFlags == visitStateTag.Change) 
            {
                args.Placement.RemoveTags<SyncedVisitStateTag>();
            }

            if (args.Placement.GetTag(out PreviewRecordTag tag))
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
    }
}
