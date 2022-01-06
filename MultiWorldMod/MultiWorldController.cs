using MenuChanger;
using MultiWorldMod.Randomizer;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            int i = 0;
            (int, string, string)[] items = OrderedItemPlacements.Get(rc);
            return items;
        }

        public void InitiateGame()
        {
            MultiWorldMod.Connection.InitiateGame(rc.gs.Seed);
        }
    }
}
