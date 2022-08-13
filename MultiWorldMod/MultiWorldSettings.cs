namespace MultiWorldMod
{
	public class MultiWorldSettings
	{
		public List<string> PlayersNames { get; set; } = new();
		public List<(string label, string data, int to)> UnconfirmedDatas { get; set; } = new();

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

		public void AddSentData((string label, string data, int to) data)
		{
			UnconfirmedDatas.Add(data);
		}

		public void MarkDataConfirmed((string label, string data, int to) data)
		{
			UnconfirmedDatas.Remove(data);
		}
    }
}
