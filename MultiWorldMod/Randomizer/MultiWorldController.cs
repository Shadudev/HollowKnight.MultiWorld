using MenuChanger;
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

                // TODO if needed
                // InitialMultiSetup();
                // SessionSetup();

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
        }

        public static void SessionSetup()
        {
            ItemManager.SubscribeEvents();
        }

        internal void SessionUnload()
        {
            ItemManager.UnsubscribeEvents();
        }
    }
}
