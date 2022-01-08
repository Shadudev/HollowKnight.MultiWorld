namespace ItemSyncMod
{
	public class ItemSyncSettings
	{
		private readonly List<string> sentUnconfirmedItems = new();

		public void Setup()
		{
			if (IsItemSync)
			{
				try
				{
					ItemSyncMod.Connection.Connect(URL);
					ItemSyncMod.Connection.JoinRando(MWRandoId, MWPlayerId);
				}
				catch (Exception) { }
			}
		}

		public bool IsItemSync { get; set; } = false;
		public string URL { get; set; }
		public int MWRandoId { get; set; }
		public int MWPlayerId { get; set; }
		public string UserName { get; set; }

		public List<string> GetUnconfirmedItems()
		{
			return sentUnconfirmedItems.ToList();
		}

		public void AddSentItem(string item)
		{
			sentUnconfirmedItems.Add(item);
		}

		public void MarkItemConfirmed(string item)
		{
			sentUnconfirmedItems.Remove(item);
		}
	}
}
