using Modding;
using SereCore;
using System;
using System.Collections;
using System.Threading;
using UnityEngine.SceneManagement;

namespace MultiWorldMod
{
	public class ItemSync : Mod
	{
        internal ClientConnection Connection;

		private readonly object _randomizationLock = new object();
        private SettingsSync settingsSync;

        public SaveSettings Settings { get; set; } = new SaveSettings();
		public MultiWorldSettings MultiWorldSettings { get; set; } = new MultiWorldSettings();

		public override ModSettings SaveSettings
		{
			get => Settings = Settings ?? new SaveSettings();
			set => Settings = value is SaveSettings saveSettings ? saveSettings : Settings;
		}

		public override ModSettings GlobalSettings
        {
            get => MultiWorldSettings = MultiWorldSettings ?? new MultiWorldSettings();
            set => MultiWorldSettings = value is MultiWorldSettings globalSettings ? globalSettings : MultiWorldSettings;
        }

		public static ItemSync Instance
		{
			get; private set;
		}

		public override string GetVersion()
		{
			string ver = "1.2.0";
			return ver;
		}

		public override void Initialize()
		{
			if (Instance != null)
			{
				LogWarn("Initialized twice... Stop that.");
				return;
			}

			Instance = this;

			if (!DoesLoadedRandoSupportMW())
			{
				LogWarn("Loaded rando doesn't support multiworld, not doing a thing.");
            }
            else
            {
				LogDebug("ItemSync Initializing...");
				UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnMainMenu;
				Instance.Connection = new ClientConnection();
				MenuChanger.AddMultiWorldMenu();

				ModHooks.Instance.BeforeSavegameSaveHook += OnSave;
				ModHooks.Instance.ApplicationQuitHook += OnQuit;
				On.QuitToMenu.Start += OnQuitToMenu;

				RandomizerMod.SaveSettings.PreAfterDeserialize += (settings) =>
						ItemManager.LoadMissingItems(settings.ItemPlacements);

				settingsSync = new SettingsSync();
				settingsSync.AddSpoilerInitListener();
				SetSettingsSync(false);
			}
		}

        private bool DoesLoadedRandoSupportMW()
		{
			try
			{
				Type[] types = typeof(RandomizerMod.RandomizerMod).GetInterfaces();
				return Array.Exists(types, type => type == typeof(RandomizerMod.MultiWorld.IMultiWorldCompatibleRandomizer));
			}
			catch (TypeLoadException)
            {
				// Old RandomizerMod version (pre RandomizerMod.MultiWorld.IMultiWorldCompatibleRandomizer commit)
				return false;
			}
			catch (Exception e)
			{
				LogWarn("Failed to check for loaded Randomizer MultiWorld support: " + e.Message);
				return false;
			}
		}

		private void OnMainMenu(Scene from, Scene to)
		{
			if (Ref.GM.GetSceneNameString() == SceneNames.Menu_Title)
            {
				MenuChanger.AddMultiWorldMenu();
				settingsSync.Reset();
			}
		}

		internal void StartGame()
		{
            ModHooks.Instance.BeforeSceneLoadHook += WaitForRandomization;
			MenuChanger.StartGame();
            ModHooks.Instance.BeforeSceneLoadHook -= WaitForRandomization;
		}

		internal void InitiateGame()
		{
			new Thread(settingsSync.UploadRandomizerSettings).Start();
			Connection.InitiateGame();
		}

		internal void SetSettingsSync(bool value)
		{
			settingsSync.ToggleSync(value);
		}

		internal void UploadRandomizerSettings()
		{
			settingsSync.UploadRandomizerSettings();
		}

		internal void ApplyRandomizerSettings(string settingsJson)
		{
			settingsSync.ApplyRandomizerSettings(settingsJson);
		}

		internal string WaitForRandomization(string dummy)
        {
			lock (_randomizationLock)
            {
				Monitor.Wait(_randomizationLock);
            }
			return dummy;
        }

        internal void NotifyRandomizationFinished()
		{
			lock (_randomizationLock)
			{
				Monitor.Pulse(_randomizationLock);
			}
		}

		private void OnSave(SaveGameData data)
		{
			if (Connection.IsConnected())
            {
				try
				{
					Instance.Connection.NotifySave();
				} 
				catch (Exception) { }
			}
		}

		private void OnQuit()
		{
			try
			{
				if (Connection.IsConnected())
					Instance.Connection.Leave();
			}
			catch (Exception) { }

			try
			{
				Instance.Connection.Disconnect();
			}
			catch (Exception) { }
		
			CharmNotchCostsObserver.ResetLogicDoneFlag();
		}

		private IEnumerator OnQuitToMenu(On.QuitToMenu.orig_Start orig, QuitToMenu self)
		{
			try
			{
				if (Connection.IsConnected())
					Instance.Connection.Leave();
			}
			catch (Exception) { }

			try
			{
				Instance.Connection.Disconnect();
			}
			catch (Exception) { }
			
			CharmNotchCostsObserver.ResetLogicDoneFlag();
			GiveItem.ClearReceivedItemsList();
			return orig(self);
		}

		internal void SaveMultiWorldSettings()
		{
			SaveGlobalSettings();
		}

		public override int LoadPriority()
        {
			return 2;
        }
    }
}
