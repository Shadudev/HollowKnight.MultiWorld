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
		private SerializableDictionary<string, (RandomizerMod.Randomization.ReqDef, string)> _addedItems = 
			new SerializableDictionary<string, (RandomizerMod.Randomization.ReqDef, string)>();

		// TODO make a serializable ReqDef assuming Tuple is serializable already?
		internal (string, RandomizerMod.Randomization.ReqDef, string)[] AddedItems => 
			_addedItems.Select(kvp => (kvp.Key, kvp.Value.Item1, kvp.Value.Item2)).ToArray();

		public bool IsMW => MWNumPlayers >= 1;

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
						ItemManager.LoadItems();

						MultiWorldMod.Instance.Connection.Connect();
						MultiWorldMod.Instance.Connection.JoinRando(MWRandoId, MWPlayerId);
					}
					catch (Exception) { }
				}
			};
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
			return RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements.First(pair => pair.Item1 == item).Item2;
		}

        internal void AddItem(string nameKey, RandomizerMod.Randomization.ReqDef def, string displayName)
        {
			_addedItems[nameKey] = (def, displayName);
        }
    }
}
