using MenuChanger;
using MultiWorldLib.MultiWorldSettings;
using MultiWorldMod.Items;
using MultiWorldMod.Items.Remote.UIDefs;
using Newtonsoft.Json;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer
{
    internal class MultiWorldController
    {
        private readonly RandoController rc;
        private readonly MenuHolder menu;

        public MultiWorldController() : this(null, null) { }

        public MultiWorldController(RandoController rc, MenuHolder menu)
        {
            this.rc = rc;
            this.menu = menu;
        }

        // Based on RandomizerMod.Menu.RandomizerMenu.StartRandomizerGame
        public void StartGame()
        {
            try
            {
                //RandomizerMenuAPI.Menu.StartRandomizerGame();
                rc.Save();

                InitialMultiSetup();
                SetupMultiSession();

                MenuChangerMod.HideAllMenuPages();

                UIManager.instance.StartNewGame();
                EjectMenuHandler.Initialize();
            }
            catch (Exception e)
            {
                LogHelper.LogError("Start Game terminated due to error:\n" + e);
                menu.ShowStartGameFailure();
            }
        }

        public void InitialMultiSetup()
        {
            ItemManager.AddRemoteNotchCostUI();
            ItemManager.SetupPlacements();
        }

        public void InitiateGame()
        {
            MultiWorldMod.Connection.InitiateGame(GetSerializedSettings(rc.gs.Seed));
        }

        internal void SetupMultiSession()
        {
            ItemManager.SubscribeEvents();

            MultiWorldMod.Connection.JoinRando(MultiWorldMod.MWS.MWRandoId, MultiWorldMod.MWS.PlayerId);
            
            if (MultiWorldMod.RecentItemsInstalled)
                RemoteItemUIDef.RegisterRecentItemsCallback();
        }

        internal void UnloadMultiSetup()
        {
            ItemManager.UnloadCache();
            ItemManager.UnsubscribeEvents();

            MultiWorldMod.Connection.Disconnect();
            
            if (MultiWorldMod.RecentItemsInstalled)
                RemoteItemUIDef.UnregisterRecentItemsCallback();
        }

        internal (string, string)[] GetShuffledItemsPlacementsInOrder()
        {
            ItemManager.LoadShuffledItemsPlacementsInOrder(rc);
            return ItemManager.GetShuffledItemsPlacementsInOrder();
        }

        private string GetSerializedSettings(int seed)
        {
            return JsonConvert.SerializeObject(new MultiWorldGenerationSettings()
            {
                Seed = seed,
                RandomizationAlgorithm = RandomizationAlgorithm.Default
            });
        }
    }
}
