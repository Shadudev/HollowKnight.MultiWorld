namespace ItemSyncMod
{
	public class ItemSyncSettings
	{
		public readonly List<(string, string, int)> sentUnconfirmedDatas = new();
		public List<string> nicknames = new();

		public bool IsItemSync { get; set; } = false;
		public string URL { get; set; }
		public int MWRandoId { get; set; }
		public int MWPlayerId { get; set; }
		public string UserName { get; set; }

		// Menu Settings
		public bool SyncVanillaItems { get; set; } = false;
		public bool SyncSimpleKeysUsages { get; set; } = false;

        public List<(string, string, int)> GetUnconfirmedDatas()
		{
			return sentUnconfirmedDatas.ToList();
		}

		public void AddSentData((string label, string data, int to) v)
		{
			sentUnconfirmedDatas.Add(v);
		}

		public void MarkDataConfirmed((string label, string data, int to) v)
		{
			sentUnconfirmedDatas.Remove(v);
		}

        internal void SetNicknames(string[] nicknames)
        {
            this.nicknames = nicknames.ToList();
        }

		public string[] GetNicknames() => nicknames.ToArray();
    }
}
