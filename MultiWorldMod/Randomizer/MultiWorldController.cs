using ItemChanger;
using MenuChanger;
using MultiWorldLib.MultiWorldSettings;
using MultiWorldMod.Items;
using MultiWorldMod.Items.Remote.UIDefs;
using Newtonsoft.Json;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer
{
    public class MultiWorldController
    {
        public readonly RandoController randoController;
        public List<string> IncludedGroupsLabels { get; set; } = new List<string>() { RBConsts.MainItemGroup };
        private readonly MenuHolder menu;

        public MultiWorldController() : this(null, null) { }

        public MultiWorldController(RandoController rc, MenuHolder menu)
        {
            this.randoController = rc;
            this.menu = menu;
        }

        // Based on RandomizerMod.Menu.RandomizerMenu.StartRandomizerGame
        public void StartGame()
        {
            try
            {
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
            ItemManager.SetupPlacements(randoController);
            try
            {
                // Just for the duration of rc.Save
                Finder.GetItemOverride += ItemManager.GetRemoteItem;
                // ItemManager.RegisterRemoteLocation is called once during mod initialization
                randoController.Save();
            }
            finally
            {
                Finder.GetItemOverride -= ItemManager.GetRemoteItem;
            }

            ItemManager.RerollShopCosts();

            ItemManager.AddRemoteNotchCostUI();
        }

        public void InitiateGame()
        {
            MultiWorldMod.Connection.InitiateGame(GetSerializedSettings(randoController.gs.Seed));
        }

        internal void SetupMultiSession()
        {
            ItemManager.SubscribeEvents();

            MultiWorldMod.Connection.FlushReceivedMessagesQueue();
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

        internal Dictionary<string, (string, string)[]> GetShuffledItemsPlacementsInOrder()
        {
            ItemManager.LoadShuffledItemsPlacementsInOrder(randoController);
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

        internal int GetRandoHash()
        {
            return randoController.Hash();
        }

        internal void SetGeneratedHash(string generationHash)
        {
            menu.SetGeneratedHash(generationHash);
        }
    }
}
