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

        // Based on RandomizerMod.RandomizerMenu.StartRandomizerGame
        public void StartGame()
        {
            try
            {
                rc.Save();

                InitialMultiSetup();

                MenuChangerMod.HideAllMenuPages();
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
