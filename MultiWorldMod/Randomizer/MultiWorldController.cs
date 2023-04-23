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

        public MultiWorldController() : this(null) { }

        public MultiWorldController(RandoController rc)
        {
            randoController = rc;
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
                MenuHolder.MenuInstance.ShowStartGameFailure();
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

        private bool hooked;
        internal void SetupMultiSession()
        {
            ItemManager.SubscribeEvents();

            MultiWorldMod.Connection.FlushReceivedMessagesQueue();
            MultiWorldMod.Connection.JoinRando(MultiWorldMod.MWS.MWRandoId, MultiWorldMod.MWS.PlayerId);
            
            if (MultiWorldMod.RecentItemsInstalled)
                RemoteItemUIDef.RegisterRecentItemsCallback();

            ForfeitButton.Enable();
            MultiWorldMod.VoteEjectMenuInstance.LoadNames(MultiWorldMod.MWS.GetNicknames().ToList());

            hooked = true;
        }

        internal void UnloadMultiSetup()
        {
            if (!hooked) return;

            ItemManager.UnloadCache();
            ItemManager.UnsubscribeEvents();

            MultiWorldMod.Connection.Disconnect();

            if (MultiWorldMod.RecentItemsInstalled)
                RemoteItemUIDef.UnregisterRecentItemsCallback();

            ForfeitButton.Disable();
            MultiWorldMod.VoteEjectMenuInstance.Reset();

            hooked = false;
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
            MenuHolder.MenuInstance.SetGeneratedHash(generationHash);
        }
    }
}
