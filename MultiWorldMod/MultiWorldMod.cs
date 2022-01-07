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
        internal static MultiWorldController Controller { get; set; }

        internal static ClientConnection Connection;

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
			
			if (MWS.IsMW)
            {
				Connection.Connect(MWS.URL);
				Connection.JoinRando(MWS.MWRandoId, MWS.MWPlayerId);
            }
		}

        public MultiWorldSettings OnSaveLocal()
        {
			if (MWS.IsMW) Connection.NotifySave();

			return MWS;
        }
    }
}
