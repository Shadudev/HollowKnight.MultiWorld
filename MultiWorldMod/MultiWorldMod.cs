using Modding;
using MultiWorldMod.Items.Remote;
using MultiWorldMod.Randomizer;
using UnityEngine.SceneManagement;

namespace MultiWorldMod
{
	public class MultiWorldMod : Mod, IGlobalSettings<GlobalSettings>, ILocalSettings<MultiWorldSettings>
	{
		public static GlobalSettings GS { get; private set; } = new();
		public static MultiWorldSettings MWS { get; set; } = new();
        internal static MultiWorldController Controller { get; set; }

        internal static ClientConnection Connection;
		internal static bool RecentItemsInstalled = false;

		public override void Initialize()
		{
			base.Initialize();

			LogDebug("MultiWorld Initializing...");
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnMainMenu;
			RandomizerMod.Menu.RandomizerMenuAPI.AddStartGameOverride(MenuHolder.ConstructMenu, MenuHolder.GetMultiWorldMenuButton);
			Connection = new ClientConnection();
			LogHelper.OnLog += Log;

			ItemChanger.Finder.DefineCustomLocation(RemoteLocation.CreateDefault());

			RecentItemsInstalled = ModHooks.GetMod("RecentItems") is Mod;
		}

		public override string GetVersion()
		{
			string ver = "1.0.0";
#if (DEBUG)
			ver += "-Debug";
#endif
			return ver;
		}
		private void OnMainMenu(Scene from, Scene to)
		{
			if (to.name != "Menu_Title") return;

			Controller?.UnloadMultiSetup();
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
				Connection.JoinRando(MWS.MWRandoId, MWS.PlayerId);
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

			return modMenuEntries;
		}
	}
}
