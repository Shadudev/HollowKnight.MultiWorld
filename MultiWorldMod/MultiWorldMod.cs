using Modding;

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
			RandomizerMod.Menu.RandomizerMenuAPI.AddStartGameOverride(MenuHolder.ConstructMenu, MenuHolder.GetMultiWorldMenuButton);
			Connection = new ClientConnection();
			LogHelper.OnLog += Log;
		}

		public override string GetVersion()
		{
			string ver = "1.0.0";
			return ver;
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
