using ItemChanger;
using MenuChanger;
using MultiWorldLib.MultiWorldSettings;
using MultiWorldMod.Items;
using MultiWorldMod.Items.Remote.UIDefs;
using MultiWorldMod.Menu;
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
            randoController = rc;
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
                Finder.GetLocationOverride += ItemManager.GetRemoteLocation;

                randoController.Save();
            }
            finally
            {
                Finder.GetItemOverride -= ItemManager.GetRemoteItem;
                Finder.GetLocationOverride -= ItemManager.GetRemoteLocation;
            }

            ItemManager.RerollShopCosts();

            ItemManager.AddRemoteNotchCostUI();
        }

        public void InitiateGame()
        {
            MultiWorldMod.Connection.InitiateGame(GetSerializedSettings());
        }

        internal void SetupMultiSession()
        {
            ItemManager.SubscribeEvents();

            MultiWorldMod.Connection.FlushReceivedMessagesQueue();
            MultiWorldMod.Connection.JoinRando(MultiWorldMod.MWS.MWRandoId, MultiWorldMod.MWS.PlayerId);
            
            if (MultiWorldMod.RecentItemsInstalled)
                RemoteItemUIDef.RegisterRecentItemsCallback();

            ForfeitButton.Enable();
            MultiWorldMod.VoteEjectMenuInstance.LoadNames(MultiWorldMod.MWS.GetNicknames().ToList());
        }

        internal void UnloadMultiSetup()
        {
            ItemManager.UnloadCache();
            ItemManager.UnsubscribeEvents();

            MultiWorldMod.Connection.Disconnect();
            
            if (MultiWorldMod.RecentItemsInstalled)
                RemoteItemUIDef.UnregisterRecentItemsCallback();

            ForfeitButton.Disable();
            MultiWorldMod.VoteEjectMenuInstance.Reset();
        }

        internal Dictionary<string, (string, string)[]> GetShuffledItemsPlacementsInOrder()
        {
            ItemManager.LoadShuffledItemsPlacementsInOrder(randoController);
            return ItemManager.GetShuffledItemsPlacementsInOrder();
        }

        private string GetSerializedSettings()
        {
            return JsonConvert.SerializeObject(new MultiWorldGenerationSettings()
            {
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
