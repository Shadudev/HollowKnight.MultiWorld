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

		public override void Initialize()
		{
			base.Initialize();

			LogDebug("MultiWorld Initializing...");
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += MenuHolder.OnMainMenu;
			RandomizerMod.Menu.RandomizerMenuAPI.AddStartGameOverride(MenuHolder.ConstructMenu, MenuHolder.GetMultiWorldMenuButton);
			Connection = new ClientConnection();
			LogHelper.OnLog += Log;

			// TODO add IC things if relevant GiveItem.AddMultiWorldItemHandlers();

			// TODO add IC? things if relevant. probably mod hooks
			//ModHooks.Instance.BeforeSavegameSaveHook += OnSave;
			//ModHooks.Instance.ApplicationQuitHook += OnQuit;
			//On.QuitToMenu.Start += OnQuitToMenu;

			//RandomizerMod.SaveSettings.PreAfterDeserialize += (settings) =>
			//		ItemManager.LoadMissingItems(settings.ItemPlacements);
		}

		internal void StartGame()
		{
			//On.GameManager.BeginSceneTransition += WaitForRandomization;
			//MenuHolder.StartGame();
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

		public override string GetVersion()
		{
			string ver = "1.0.0";
			return ver;
		}

		private void OnSave(SaveGameData data)
		{
			if (MWS.IsMW)
            {
				try
				{
					Connection.NotifySave();
				} 
				catch (Exception) { }
			}
		}

		private void OnQuit()
		{
			try
			{
				Connection.Leave();
			}
			catch (Exception) { }

			try
			{
				Connection.Disconnect();
			}
			catch (Exception) { }
		
			CharmNotchCostsObserver.ResetLogicDoneFlag();
		}

		private IEnumerator OnQuitToMenu(On.QuitToMenu.orig_Start orig, QuitToMenu self)
		{
			try
			{
				Connection.Leave();
			}
			catch (Exception) { }

			try
			{
				Connection.Disconnect();
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
