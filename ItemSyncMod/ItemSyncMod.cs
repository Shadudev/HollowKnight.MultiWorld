using ItemSyncMod.Menu;
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
		internal static ItemSyncController Controller { get; set; } = new();

        public static ClientConnection Connection;

		internal static bool RecentItemsInstalled = false;

		public override string GetVersion()
		{
			string ver = "2.7.0";

#if (DEBUG)
			ver += "-Debug";
#endif
			return ver;
		}
		
		public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
		{
			base.Initialize();

			LogDebug("ItemSync Initializing...");

			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnMainMenu;
			
			Connection = new();
			RandomizerMod.Menu.RandomizerMenuAPI.AddStartGameOverride(MenuHolder.ConstructMenu, MenuHolder.GetItemSyncMenuButton);
			On.GameManager.ContinueGame += DisposeMenu;

			RecentItemsInstalled = ModHooks.GetMod("RecentItems") is Mod;
		}

        private void OnMainMenu(Scene from, Scene to)
        {
			if (to.name != "Menu_Title") return;

			Controller?.SessionSyncUnload();
			Connection = new();
		}

		private void DisposeMenu(On.GameManager.orig_ContinueGame orig, GameManager self)
		{
			orig(self);

			MenuHolder.DisposeMenu();
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

			if (ISSettings.IsItemSync)
            {
				Connection.Connect(ISSettings.URL);
				Controller.SessionSyncSetup();
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
			string[] recentItemsInfoOptions = { "Both", "Sender Only", "Area Only" };
			List<IMenuMod.MenuEntry> modMenuEntries = new()
			{
				new IMenuMod.MenuEntry
                {
					Name = "Corner Pop-up Info",
					Description = "Info shown for received items (in bottom left corner)",
					Values = new string[] { "With Sender", "Item Only" },
					Saver = opt => GS.CornerMessagePreference = opt == 0 ? GlobalSettings.InfoPreference.Both : GlobalSettings.InfoPreference.ItemOnly,
					Loader = () => (int)(GS.CornerMessagePreference == GlobalSettings.InfoPreference.Both ? GlobalSettings.InfoPreference.Both : GlobalSettings.InfoPreference.ItemOnly)
                }
			};

			if (RecentItemsInstalled)
				modMenuEntries.Add(new IMenuMod.MenuEntry
				{
					Name = "Recent Items Info",
					Description = "Info shown for received items (recent items)",
					Values = recentItemsInfoOptions,
					Saver = opt => GS.RecentItemsPreference = (GlobalSettings.InfoPreference)opt,
					Loader = () => (int)GS.RecentItemsPreference
				});

			return modMenuEntries;
        }
    }
}
