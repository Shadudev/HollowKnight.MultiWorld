using Modding;
using SereCore;
using System;
using System.Collections;
using System.Threading;
using UnityEngine.SceneManagement;

namespace MultiWorldMod
{
	public class MultiWorldMod : Mod
	{
        internal ClientConnection Connection;

		private object _randomizationLock = new object();

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

		public static MultiWorldMod Instance
		{
			get; private set;
		}

		public override string GetVersion()
		{
			string ver = "0.0.8";
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
				LogDebug("MultiWorld Initializing...");
				UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnMainMenu;
				Instance.Connection = new ClientConnection();
				MenuChanger.AddMultiWorldMenu();
				GiveItem.AddMultiWorldItemHandlers();

				ModHooks.Instance.BeforeSavegameSaveHook += OnSave;
				ModHooks.Instance.ApplicationQuitHook += OnQuit;
				On.QuitToMenu.Start += OnQuitToMenu;

				RandomizerMod.SaveSettings.PreAfterDeserialize += (settings) =>
						ItemManager.LoadMissingItems(settings.ItemPlacements);
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
			}
		}

		internal void StartGame()
		{
            ModHooks.Instance.BeforeSceneLoadHook += WaitForRandomization;
			MenuChanger.StartGame();
            ModHooks.Instance.BeforeSceneLoadHook -= WaitForRandomization;
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
			if (Settings.IsMW)
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
				Instance.Connection.Leave();
			}
			catch (Exception) { }

			try
			{
				Instance.Connection.Disconnect();
			}
			catch (Exception) { }
			
			CharmNotchCostsObserver.ResetLogicDoneFlag();
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