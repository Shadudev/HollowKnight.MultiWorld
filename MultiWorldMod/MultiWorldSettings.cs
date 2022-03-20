using MultiWorldLib;

namespace MultiWorldMod
{
	public class MultiWorldSettings
	{
		private readonly Dictionary<int, string> _mwPlayerNames = new();
		private readonly List<string> unconfirmedItems = new();

		public string[] UnconfirmedItems => unconfirmedItems.ToArray();

		public void Setup()
		{
			if (IsMW)
			{
				try
				{
					LanguageStringManager.SetMWNames(_mwPlayerNames);
                    EjectMenuHandler.Initialize();
                }
				catch (Exception) { }
			}
		}

		public bool IsMW { get; set; } = false;
		public string URL { get; set; }
		public int MWPlayerId { get; set; }

		public int MWRandoId { get; set; }

        internal void SetMWNames(string[] nicknames)
		{
			nicknames.Select((nickname, index) => _mwPlayerNames[index] = nickname);
		}

		public void AddSentItem(string item)
		{
			unconfirmedItems.Add(item);
		}

		public void MarkItemConfirmed(string item)
		{
			unconfirmedItems.Remove(item);
		}
    }
}
