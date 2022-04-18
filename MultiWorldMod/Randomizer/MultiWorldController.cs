using MenuChanger;
using MultiWorldMod.Items;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer
{
    internal class MultiWorldController
    {
        private readonly RandoController rc;
        private readonly MenuHolder menu;

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
                LogHelper.LogDebug($"StartGame called");
                //RandomizerMenuAPI.Menu.StartRandomizerGame();
                rc.Save();
                LogHelper.LogDebug($"rc.Save finished");

                InitialMultiSetup();
                LogHelper.LogDebug($"InitialMultiSetup finished");

                MenuChangerMod.HideAllMenuPages();
                LogHelper.LogDebug($"HideAllMenuPages finished");
                MultiWorldMod.Connection.JoinRando(MultiWorldMod.MWS.MWRandoId, MultiWorldMod.MWS.PlayerId);

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
            LogHelper.LogDebug($"AddRemoteNotchCostUI finished");

            ItemManager.SetupPlacements();
        }

        public void InitiateGame()
        {
            MultiWorldMod.Connection.InitiateGame(rc.gs.Seed);
        }

        internal void UnloadMultiSetup()
        {
            ItemManager.UnloadCache();
            MultiWorldMod.Connection.Disconnect();
        }

        internal (string, string)[] GetShuffledItemsPlacementsInOrder()
        {
            ItemManager.LoadShuffledItemsPlacementsInOrder(rc);
            return ItemManager.GetShuffledItemsPlacementsInOrder();
        }
    }
}
