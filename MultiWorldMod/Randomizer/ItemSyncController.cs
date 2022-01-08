using ItemChanger;
using ItemSyncMod.Items;
using MenuChanger;
using RandomizerMod.IC;
using RandomizerMod.RC;

namespace ItemSyncMod.Randomizer
{
    internal class ItemSyncController
    {
        private readonly RandoController rc;
        private readonly MenuHolder menu;

        public ItemSyncController(RandoController rc, MenuHolder menu)
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
                AddSyncedItemTags();
                MenuChangerMod.HideAllMenuPages();
                UIManager.instance.StartNewGame();
            }
            catch (Exception e)
            {
                LogHelper.LogError("Start Game terminated due to error:\n" + e);
                menu.ShowStartGameFailure();
            }
        }

        private static void AddSyncedItemTags()
        {
            foreach (AbstractPlacement placement in ItemChanger.Internal.Ref.Settings.GetPlacements())
            {
                foreach (AbstractItem randoItem in placement.Items.Where(item => item.HasTag<RandoItemTag>()))
                {
                    randoItem.AddTag<SyncedItemTag>().ItemID = ItemManager.GenerateItemId(placement, randoItem);
                }
            }
        }
    }
}
