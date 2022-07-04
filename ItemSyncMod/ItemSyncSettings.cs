using ItemChanger;
using MultiWorldLib;
using MultiWorldLib.Messaging.Definitions.Messages;

namespace ItemSyncMod
{
	public class ItemSyncSettings
	{
		private readonly List<Item> sentUnconfirmedItems = new();
		private readonly List<(string, string[], PreviewRecordTagType, VisitState)> sentUnconfirmedVisitStateChanges = new();
        private readonly List<(string, string)> sentUnconfirmedTransitionsFound = new();

		public bool IsItemSync { get; set; } = false;
		public string URL { get; set; }
		public int MWRandoId { get; set; }
		public int MWPlayerId { get; set; }
		public string UserName { get; set; }

		// Menu Settings
		public bool SyncVanillaItems { get; set; } = false;
		public bool SyncSimpleKeysUsages { get; set; } = false;
		public bool AdditionalFeaturesEnabled { get; set; } = true;

        public List<Item> GetUnconfirmedItems()
		{
			return sentUnconfirmedItems.ToList();
		}

		public void AddSentItem(Item item)
		{
			sentUnconfirmedItems.Add(item);
		}

		public void MarkItemConfirmed(Item item)
		{
			sentUnconfirmedItems.Remove(item);
		}

		public List<(string, string[], PreviewRecordTagType, VisitState)> GetUnconfirmedStateChanges()
        {
			return sentUnconfirmedVisitStateChanges.ToList();
        }

		public void AddSentVisitChange(string name, string[] previewTexts, PreviewRecordTagType previewRecordTagType, VisitState newVisitFlags)
        {
			sentUnconfirmedVisitStateChanges.Add((name, previewTexts, previewRecordTagType, newVisitFlags));
		}

		public void MarkVisitChangeConfirmed(string name, PreviewRecordTagType previewRecordTagType, VisitState newVisitFlags)
		{
			sentUnconfirmedVisitStateChanges.RemoveAll(entry => 
				entry.Item1 == name && entry.Item3 == previewRecordTagType && 
				entry.Item4 == newVisitFlags);
		}

        public List<(string, string)> GetUnconfirmedTransitionsFound()
        {
			return sentUnconfirmedTransitionsFound.ToList();
        }

		public void AddTransitionFound(string source, string target)
        {
			sentUnconfirmedTransitionsFound.Add((source, target));
        }

		public void MarkTransitionFoundConfirmed(string source, string target)
        {
			sentUnconfirmedTransitionsFound.Remove((source, target));
        }
    }
}
