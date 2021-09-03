using Modding;
using MultiWorldLib;
using SereCore;
using System;
using System.Linq;

namespace MultiWorldMod
{
	public class SaveSettings : BaseSettings
	{
		private SerializableDictionary<int, string> _mwPlayerNames = new SerializableDictionary<int, string>();
		private SerializableBoolDictionary _sentItems = new SerializableBoolDictionary();

		public string[] UnconfirmedItems => _sentItems.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToArray();

		public SaveSettings()
		{
			AfterDeserialize += () =>
			{
				if (IsMW)
				{
					try
					{
						LanguageStringManager.SetMWNames(_mwPlayerNames);
						
						ItemSync.Instance.Connection.Connect();
						ItemSync.Instance.Connection.JoinRando(MWRandoId, MWPlayerId);
						CharmNotchCostsObserver.SetCharmNotchCostsLogicDone();
					}
					catch (Exception) { }
				}
			};
		}

		public bool IsMW 
		{ 
			get => GetBool(false);
			set => SetBool(value); 
		}
		public int MWNumPlayers
		{
			get => GetInt(1);
			set => SetInt(value);
		}
		public int MWPlayerId
		{
			get => GetInt(0);
			set => SetInt(value);
		}

		public int MWRandoId
		{
			get => GetInt();
			set => SetInt(value);
		}
        public int LastUsedSeed
        {
			get => GetInt();
			set => SetInt(value);
		}

		public void AddSentItem(string item)
		{
			_sentItems[item] = false;
		}

		public void MarkItemConfirmed(string item)
		{
			_sentItems[item] = true;
		}

        internal string GetItemLocation(string item)
        {
			return RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements.First(pair => pair.Item1 == item).Item2;
		}
		internal void SetMWNames(string[] nicknames)
		{
			for (int i = 0; i < nicknames.Length; i++)
			{
				_mwPlayerNames[i] = nicknames[i];
			}
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
	}
}
