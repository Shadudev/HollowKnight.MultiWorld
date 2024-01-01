using ItemSyncMod.Menu;
using ItemSyncMod.Randomizer;
using MenuChanger.MenuElements;
using MenuChanger;
using Modding;
using RandomizerMod.RC;
using UnityEngine;
using UnityEngine.SceneManagement;
using ItemSyncMod.ICDL;

namespace ItemSyncMod
{
	public class ItemSyncMod : Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<ItemSyncSettings>, IMenuMod
	{
		public static GlobalSettings GS { get; private set; } = new();
		public static ItemSyncSettings ISSettings { get; set; } = new();
		internal static BaseController Controller { get; set; }

        public static ClientConnection Connection;

		internal static bool RecentItemsInstalled = false;

		public override string GetVersion()
		{
			string ver = "2.6.3";

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

            List<ItemSyncMenu> randoMenuHolder = new();
            RandomizerMod.Menu.RandomizerMenuAPI.AddStartGameOverride(
                page => randoMenuHolder.Add(new(page)),
                (RandoController rc, MenuPage landingPage, out BaseButton button) =>
                {
                    Controller = new ItemSyncRandoController(rc, randoMenuHolder[0]);
                    return randoMenuHolder[0].GetMenuButton(out button);
                });

            List<ItemSyncMenu> icdlMenuHolder = new();
            if (ModHooks.GetMod("ICDL Mod") is Mod) ICDLInterop.Hook(icdlMenuHolder);

            On.GameManager.ContinueGame += (orig, self) =>
            {
                orig(self);

                randoMenuHolder.ForEach(m => m.Dispose());
                randoMenuHolder.Clear();

                icdlMenuHolder.ForEach(m => m.Dispose());
                icdlMenuHolder.Clear();
            };

            RecentItemsInstalled = ModHooks.GetMod("RecentItems") is Mod;
        }

        private void OnMainMenu(Scene from, Scene to)
        {
			if (to.name != "Menu_Title") return;

			Controller?.SessionSyncUnload();
			Connection = new();
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
