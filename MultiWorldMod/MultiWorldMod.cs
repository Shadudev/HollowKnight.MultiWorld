using Modding;
using System;
using System.Collections;
using System.Threading;
using UnityEngine.SceneManagement;

namespace MultiWorldMod
{
	public class MultiWorldMod : Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<MultiWorldSettings>
	{
		public static GlobalSettings GS { get; private set; } = new();
		public static MultiWorldSettings MWS { get; set; } = new();
		internal static ClientConnection Connection;

		private object _randomizationLock = new();
		private bool waitingForRandomization = true;



		/*public override GlobalSettings GlobalSettings
        {
            get => MultiWorldSettings = MultiWorldSettings ?? new MultiWorldSettings();
            set => MultiWorldSettings = value is MultiWorldSettings globalSettings ? globalSettings : MultiWorldSettings;
        }*/

		public static MultiWorldMod Instance
		{
			get; private set;
		}

		public override string GetVersion()
		{
			string ver = "0.1.1";
			return ver;
		}

		public override void Initialize()
		{
			base.Initialize();

			LogDebug("MultiWorld Initializing...");
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnMainMenu;
			Connection = new ClientConnection();
			MenuChanger.AddMultiWorldMenu();
			GiveItem.AddMultiWorldItemHandlers();

			ModHooks.Instance.BeforeSavegameSaveHook += OnSave;
			ModHooks.Instance.ApplicationQuitHook += OnQuit;
			On.QuitToMenu.Start += OnQuitToMenu;

			RandomizerMod.SaveSettings.PreAfterDeserialize += (settings) =>
					ItemManager.LoadMissingItems(settings.ItemPlacements);
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
				waitingForRandomization = true;
				MenuChanger.AddMultiWorldMenu();
			}
		}

		internal void StartGame()
		{
			On.GameManager.BeginSceneTransition += WaitForRandomization;
			MenuChanger.StartGame();
		}

        internal void WaitForRandomization(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
		{
			lock (_randomizationLock)
            {
				if (waitingForRandomization)
					Monitor.Wait(_randomizationLock);
			}
			orig(self, info);
        }

        internal void NotifyRandomizationFinished()
		{
			lock (_randomizationLock)
			{
				On.GameManager.BeginSceneTransition -= WaitForRandomization;
				waitingForRandomization = false;
				Monitor.Pulse(_randomizationLock);
			}
		}

		private void OnSave(SaveGameData data)
		{
			if (MWS.IsMW)
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

		void IGlobalSettings<GlobalSettings>.OnLoadGlobal(GlobalSettings s)
		{
			GS = s ?? new();
		}

		GlobalSettings IGlobalSettings<GlobalSettings>.OnSaveGlobal()
		{
			return GS;
		}

        public void OnLoadLocal(MultiWorldSettings s)
        {
			MWS = s;
			MWS?.Setup();
		}

        public MultiWorldSettings OnSaveLocal()
        {
			return MWS;
        }
    }
}
