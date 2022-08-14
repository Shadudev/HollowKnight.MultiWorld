using ItemChanger;
using ItemChanger.Tags;
using ItemSyncMod.Items;
using MultiWorldLib;
using Newtonsoft.Json;

namespace ItemSyncMod.SyncFeatures.VisitStateChangesSync
{
    public enum PreviewRecordTagType
    {
        None = 0,
        Single = 1,
        Multi = 2
    }

    public class VisitStateChanged
    {
        public string Name;
        public string[] PreviewTexts;
        public PreviewRecordTagType PreviewRecordTagType;
        public VisitState NewVisitFlags;
    }

    public class VisitStateUpdater
    {
        public static readonly string VISIT_STATE_MESSAGE_LABEL = "ItemSync-VisitState";

        internal static void SubscribeEvents()
        {
            AbstractPlacement.OnVisitStateChangedGlobal += SyncPlacementVisitStateChanged;
            ItemSyncMod.Connection.OnDataReceived += PlacementVisitChanged;
        }

        internal static void UnsubscribeEvents()
        {
            AbstractPlacement.OnVisitStateChangedGlobal -= SyncPlacementVisitStateChanged;
            ItemSyncMod.Connection.OnDataReceived -= PlacementVisitChanged;
        }

        public static void PlacementVisitChanged(DataReceivedEvent dataReceivedEvent)
        {
            if (dataReceivedEvent.Label != VISIT_STATE_MESSAGE_LABEL) return;

            VisitStateChanged visitStateChanged = JsonConvert.DeserializeObject<VisitStateChanged>(dataReceivedEvent.Content);

            AbstractPlacement placement = ItemChanger.Internal.Ref.Settings.GetPlacements().
                First(placement => placement.Name == visitStateChanged.Name);

            switch (visitStateChanged.PreviewRecordTagType)
            {
                case PreviewRecordTagType.Single:
                    placement.GetOrAddTag<PreviewRecordTag>().previewText =
                        visitStateChanged.PreviewTexts[0];
                    break;
                case PreviewRecordTagType.Multi:
                    placement.GetOrAddTag<MultiPreviewRecordTag>().previewTexts =
                        visitStateChanged.PreviewTexts;
                    break;
            }

            if (!placement.CheckVisitedAll(visitStateChanged.NewVisitFlags))
            {
                placement.AddTag<SyncedVisitStateTag>().Change = visitStateChanged.NewVisitFlags;
                placement.AddVisitFlag(visitStateChanged.NewVisitFlags);
            }

            dataReceivedEvent.Handled = true;
        }

        private static void SyncPlacementVisitStateChanged(VisitStateChangedEventArgs args)
        {
            if (args.NoChange || ItemManager.IsStartLocation(args.Placement)) return;

            if (args.Placement.GetTag(out SyncedVisitStateTag visitStateTag) && args.NewFlags == visitStateTag.Change)
            {
                args.Placement.RemoveTags<SyncedVisitStateTag>();
            }
            else if (args.Placement.GetTag(out PreviewRecordTag tag))
            {
                SendVisitStateChanged(args.Placement.Name, new string[] { tag.previewText }, PreviewRecordTagType.Single, args.NewFlags);
            }
            else if (args.Placement.GetTag(out MultiPreviewRecordTag tag2))
            {
                SendVisitStateChanged(args.Placement.Name, tag2.previewTexts, PreviewRecordTagType.Multi, args.NewFlags);
            }
            else
            {
                SendVisitStateChanged(args.Placement.Name, new string[] { "" }, PreviewRecordTagType.None, args.NewFlags);
            }
        }

        private static void SendVisitStateChanged(string name, string[] previewTexts, PreviewRecordTagType previewRecordTag, VisitState newVisitFlags)
        {
            LogHelper.LogDebug($"Sending visit state changed name: {name}, previews: {string.Join(", ", previewTexts)}, " +
                $"isMulti: {previewRecordTag}, newFlags: {newVisitFlags}");

            ItemSyncMod.Connection.SendDataToAll(VISIT_STATE_MESSAGE_LABEL,
                JsonConvert.SerializeObject(new VisitStateChanged()
                {
                    Name = name,
                    PreviewTexts = previewTexts,
                    PreviewRecordTagType = previewRecordTag,
                    NewVisitFlags = newVisitFlags
                }));
        }
    }
}
