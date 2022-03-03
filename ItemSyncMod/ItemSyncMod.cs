using ItemSyncMod.Extras;
using ItemSyncMod.Items;
using ItemSyncMod.Randomizer;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItemSyncMod
{
	public class ItemSyncMod : Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<ItemSyncSettings>, IMenuMod
	{
		public static GlobalSettings GS { get; private set; } = new();
		public static ItemSyncSettings ISSettings { get; set; } = new();
        internal static ItemSyncController Controller { get; set; }

        internal static ClientConnection Connection;
		internal static SettingsSyncer SettingsSyncer;
		internal static AdditionalFeatures AdditionalFeatures;

		public override string GetVersion()
		{
			string ver = "2.2.2";
#if (DEBUG)
			ver += "-Debug";           
#endif
			return ver;
		}

		public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			LogHelper.OnLog += Log;
			base.Initialize();

			LogDebug("ItemSync Initializing...");

			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnMainMenu;
			RandomizerMod.Menu.RandomizerMenuAPI.AddStartGameOverride(MenuHolder.ConstructMenu, MenuHolder.GetItemSyncMenuButton);
			Connection = new();
			SettingsSyncer = new();

			if (!GS.ReducePreload)
				AdditionalFeatures.SavePreloads(preloadedObjects);
		}

		public override List<(string, string)> GetPreloadNames()
        {
			AdditionalFeatures = new AdditionalFeatures();

			if (!GS.ReducePreload)
			{
				return AdditionalFeatures.GetPreloadNames();
			}
			return new();
        }

        private void OnMainMenu(Scene from, Scene to)
        {
			if (to.name != "Menu_Title") return;

			Controller?.SessionSyncUnload();
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
				ItemSyncController.SessionSyncSetup();
				Connection.Connect(ISSettings.URL);
				Connection.JoinRando(ISSettings.MWRandoId, ISSettings.MWPlayerId);
            }
        }

        public ItemSyncSettings OnSaveLocal()
        {
			if (ISSettings.IsItemSync) Connection.NotifySave();

			return ISSettings;
        }

		public bool ToggleButtonInsideMenu => false;
		public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
			return new List<IMenuMod.MenuEntry>()
			{
				new IMenuMod.MenuEntry
				{
					Name = "Reduce Preloads",
					Description = string.Empty,
					Values = new string[] { "True", "False"},
					Saver = opt => GS.ReducePreload = opt == 0,
					Loader = () => GS.ReducePreload ? 0 : 1
				}
			};
        }
    }
}
