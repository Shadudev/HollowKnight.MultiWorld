using ItemSyncMod.Items;
using ItemSyncMod.Randomizer;
using Modding;
using UnityEngine.SceneManagement;

namespace ItemSyncMod
{
	public class ItemSyncMod : Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<ItemSyncSettings>
	{
		public static GlobalSettings GS { get; private set; } = new();
		public static ItemSyncSettings ISSettings { get; set; } = new();
        internal static ItemSyncController Controller { get; set; }

        internal static ClientConnection Connection;
		internal static SettingsSyncer SettingsSyncer;
		// Maybe one day private AdditionalFeatures additionalFeatures;

		public override string GetVersion()
		{
			string ver = "2.1.0";
#if (DEBUG)
			ver += "-Debug";           
#endif
			return ver;
		}

		public override void Initialize()
		{
			LogHelper.OnLog += Log;
			base.Initialize();

			LogDebug("ItemSync Initializing...");

			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnMainMenu;
			RandomizerMod.Menu.RandomizerMenuAPI.AddStartGameOverride(MenuHolder.ConstructMenu, MenuHolder.GetItemSyncMenuButton);
			Connection = new();
			SettingsSyncer = new();

			// ObjectCache.GetPrefabs(preloaded);
			// additionalFeatures = new AdditionalFeatures();
			// additionalFeatures.Hook();
		}

        private void OnMainMenu(Scene from, Scene to)
        {
			if (to.name != "Menu_Title") return;

			ItemManager.UnsubscribeEvents();
		}

		public void OnLoadGlobal(GlobalSettings s)
        {
			GS = s ?? new();
		}

        public GlobalSettings OnSaveGlobal()
        {
			return GS;
        }

        public void OnLoadLocal(ItemSyncSettings s)
        {
			ISSettings = s;
			ISSettings?.Setup();

			if (ISSettings.IsItemSync)
            {
				ItemManager.SubscribeEvents();
				Controller.SessionSyncSetup();
				Connection.Connect(ISSettings.URL);
				Connection.JoinRando(ISSettings.MWRandoId, ISSettings.MWPlayerId);
            }
        }

        public ItemSyncSettings OnSaveLocal()
        {
			if (ISSettings.IsItemSync) Connection.NotifySave();

			return ISSettings;
        }
    }
}
