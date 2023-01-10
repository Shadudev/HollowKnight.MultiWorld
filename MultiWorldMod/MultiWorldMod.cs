using Modding;
using MultiWorldMod.Menu;
using MultiWorldMod.Randomizer;
using UnityEngine.SceneManagement;

namespace MultiWorldMod
{
	public class MultiWorldMod : Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<MultiWorldSettings>, ICustomMenuMod
	{
		public static GlobalSettings GS { get; private set; } = new();
		public static MultiWorldSettings MWS { get; set; } = new();
        internal static MultiWorldController Controller { get; set; }
		internal static ObtainRemotePlacementsMenu VoteEjectMenuInstance { get; set; }

		internal static ClientConnection Connection;
		internal static bool RecentItemsInstalled = false;

		public override void Initialize()
		{
			base.Initialize();

			LogDebug("MultiWorld Initializing...");
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnMainMenu;
			RandomizerMod.Menu.RandomizerMenuAPI.AddStartGameOverride(MenuHolder.ConstructMenu, MenuHolder.GetMultiWorldMenuButton);

			Connection = new ClientConnection();

			RecentItemsInstalled = ModHooks.GetMod("RecentItems") is Mod;

			ForfeitButton.Initialize();
			VoteEjectMenuInstance = new();
		}

		public override string GetVersion()
		{
			string ver = "1.2.0";
#if (DEBUG)
			ver += "-Debug";
#endif
			return ver;
		}
		private void OnMainMenu(Scene from, Scene to)
		{
			if (to.name != "Menu_Title") return;

			Controller?.UnloadMultiSetup();
			Connection.Disconnect();
			VoteEjectMenuInstance?.Reset();
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

			if (MWS.IsMW)
            {
				Connection.Connect(MWS.URL);

				Controller = new();
				Controller.SetupMultiSession();
            }
		}

        public MultiWorldSettings OnSaveLocal()
        {
			if (MWS.IsMW) Connection.NotifySave();

			return MWS;
        }

		public bool ToggleButtonInsideMenu => false;

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
		{
			string[] recentItemsInfoOptions = { "Both", "Owner Only", "Area Only" };
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
			{
				modMenuEntries.Add(new IMenuMod.MenuEntry
				{
					Name = "Recent Sent Items Info",
					Description = "Info shown for items sent",
					Values = recentItemsInfoOptions,
					Saver = opt => GS.RecentItemsPreferenceForRemoteItems = (GlobalSettings.InfoPreference)opt,
					Loader = () => (int)GS.RecentItemsPreferenceForRemoteItems
				});
				modMenuEntries.Add(new IMenuMod.MenuEntry
				{
					Name = "Recent Received Items Info",
					Description = "Show sender of received items (recent items)",
					Values = new string[] { "Yes", "No" },
					Saver = opt => GS.RecentItemsPreferenceShowSender = opt == 0,
					Loader = () => GS.RecentItemsPreferenceShowSender ? 0 : 1
				});
			}

			modMenuEntries.Add(new IMenuMod.MenuEntry
			{
				Name = "Separate Single Worlds' Spoilers",
				Description = "Split items spoiler in a single world to other files",
				Values = new string[] { "Yes", "No" },
				Saver = opt => GS.SeparateIndividualWorldsSpoilers = opt == 0,
				Loader = () => GS.SeparateIndividualWorldsSpoilers ? 0 : 1
			});

			return modMenuEntries;
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
			return ModOptionsMenu.GetMultiWorldMenuScreen(modListMenu, GetMenuData(null));
        }
    }
}
