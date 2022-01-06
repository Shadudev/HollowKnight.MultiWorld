﻿using MultiWorldLib;

namespace MultiWorldMod
{
	public class MultiWorldSettings
	{
		private readonly Dictionary<int, string> _mwPlayerNames = new();
		private readonly Dictionary<string, bool> _sentItems = new();

		public string[] UnconfirmedItems => _sentItems.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToArray();

		public void Setup()
		{
			if (IsMW)
			{
				try
				{
					LanguageStringManager.SetMWNames(_mwPlayerNames);

					MultiWorldMod.Connection.Connect(URL);
					MultiWorldMod.Connection.JoinRando(MWRandoId, MWPlayerId);
					CharmNotchCostsObserver.SetCharmNotchCostsLogicDone();
					//TODO EjectMenuHandler.Initialize();
				}
				catch (Exception) { }
			}
		}

		public bool IsMW { get; set; } = false;
		public string URL { get; set; }
		public int MWNumPlayers { get; set; } = 1;
		public int MWPlayerId { get; set; }

		public int MWRandoId { get; set; }

        internal void SetMWNames(string[] nicknames)
		{
			for (int i = 0; i < nicknames.Length; i++)
			{
				_mwPlayerNames[i] = nicknames[i];
			}
		}

		public void AddSentItem(string item)
		{
			_sentItems[item] = false;
		}

		public void MarkItemConfirmed(string item)
		{
			_sentItems[item] = true;
		}

		public string GetMWPlayerName(int playerId)
		{
			string name = "Player " + (playerId + 1);
			if (_mwPlayerNames != null && _mwPlayerNames.ContainsKey(playerId))
			{
				name = _mwPlayerNames[playerId];
			}
			return name;
		}

        internal string GetItemLocation(string item)
        {
			return "placeholder";
			// TODO ItemChanger.
			//return RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements.First(pair => pair.Item1 == item).Item2;
		}
    }
}
