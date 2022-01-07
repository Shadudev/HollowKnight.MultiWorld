using MenuChanger;
using MultiWorldMod.Randomizer;
using RandomizerMod.RC;

namespace MultiWorldMod
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

        public void StartGame()
        {
            // Based off of RandomizerMod.RandomizerMenu.StartRandomizerGame
            try
            {
                rc.Save();
                MenuChangerMod.HideAllMenuPages();
                UIManager.instance.StartNewGame();
            }
            catch (Exception e)
            {
                LogHelper.LogError("Start Game terminated due to error:\n" + e);
                menu.ShowGameStartFailure();
            }
        }

        public (int, string, string)[] GetShuffleableItems()
        {
            return OrderedItemPlacements.Get(rc);
        }

        public void InitiateGame()
        {
            MultiWorldMod.Connection.InitiateGame(rc.gs.Seed);
        }
    }
}
