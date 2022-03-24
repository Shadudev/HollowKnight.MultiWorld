using MultiWorldLib;

namespace MultiWorldMod
{
	public class MultiWorldSettings
	{
		public List<string> PlayersNames { get; set; } = new();
		public List<MWItem> UnconfirmedItems { get; set; } = new();

		public void Setup()
		{
			if (IsMW)
			{
				try
				{
                    EjectMenuHandler.Initialize();
                }
				catch (Exception) { }
			}
		}

		public bool IsMW { get; set; } = false;
		public string URL { get; set; }
		public int PlayerId { get; set; }

		public int MWRandoId { get; set; }

        internal void SetPlayersNames(string[] nicknames)
		{
			PlayersNames = nicknames.ToList();
		}

		public string GetPlayerName(int playerId)
        {
			return PlayersNames[playerId];
        }

		public void AddSentItem(MWItem item)
		{
			UnconfirmedItems.Add(item);
		}

		public void MarkItemConfirmed(MWItem item)
		{
			UnconfirmedItems.Remove(item);
		}
    }
}
