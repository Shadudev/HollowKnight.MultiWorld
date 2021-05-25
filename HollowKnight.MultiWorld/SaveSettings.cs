using Modding;
using SereCore;
using System;
using System.Linq;

namespace MultiWorld
{
	public class SaveSettings : BaseSettings
	{
		private SerializableDictionary<int, string> _mwPlayerNames = new SerializableDictionary<int, string>();
		private SerializableBoolDictionary _sentItems = new SerializableBoolDictionary();

		public bool IsMW => MWNumPlayers > 1;

		public string[] UnconfirmedItems => _sentItems.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToArray();

		public SaveSettings()
		{
			AfterDeserialize += () =>
			{
				LanguageStringManager.SetMWNames(_mwPlayerNames);
				if (IsMW && RandomizerMod.RandomizerMod.Instance.Settings.Randomizer)
				{
					try
					{
						// TODO mwConnection.Connect();
						// TODO mwConnection.JoinRando(MWRandoId, MWPlayerId);
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
	}
}
