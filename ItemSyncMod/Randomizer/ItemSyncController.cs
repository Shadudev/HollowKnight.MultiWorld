using ItemSyncMod.Items;
using ItemSyncMod.Transitions;
using MenuChanger;
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
                
                InitialSyncSetup();
                SessionSyncSetup();
                
                MenuChangerMod.HideAllMenuPages();
                UIManager.instance.StartNewGame();
            }
            catch (Exception e)
            {
                LogHelper.LogError("Start Game terminated due to error:\n" + e);
                menu.ShowStartGameFailure();
            }
        }

        public void InitialSyncSetup()
        {
            if (ItemSyncMod.ISSettings.SyncVanillaItems)
                ItemManager.AddVanillaItemsToICPlacements(rc.ctx.Vanilla);
            ItemManager.AddSyncedTags(ItemSyncMod.ISSettings.SyncVanillaItems);
            TransitionsManager.Setup();
        }

        public static void SessionSyncSetup()
        {
            ItemManager.SubscribeEvents();
            if (ItemSyncMod.ISSettings.AdditionalFeaturesEnabled)
                ItemSyncMod.AdditionalFeatures.Hook();
        }

        internal void SessionSyncUnload()
        {
            ItemManager.UnsubscribeEvents();
            if (ItemSyncMod.ISSettings.AdditionalFeaturesEnabled)
                ItemSyncMod.AdditionalFeatures.Unhook();
        }
    }
}
