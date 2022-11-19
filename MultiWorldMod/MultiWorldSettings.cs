namespace MultiWorldMod
{
	public class MultiWorldSettings
	{
		public List<string> nicknames = new();
		public List<(string label, string data, int to)> UnconfirmedDatas { get; set; } = new();

		public bool IsMW { get; set; } = false;
		public string URL { get; set; }
		public int PlayerId { get; set; }

		public int MWRandoId { get; set; }

        internal void SetPlayersNames(string[] nicknames)
		{
			this.nicknames = nicknames.ToList();
		}

		public string GetPlayerName(int playerId)
        {
			return nicknames[playerId];
        }
		
		public string[] GetNicknames() => nicknames.ToArray();

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
