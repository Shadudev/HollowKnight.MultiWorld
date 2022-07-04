using ItemSyncMod.Items;
using ItemSyncMod.SyncFeatures;
using ItemSyncMod.SyncFeatures.SimpleKeysUsages;
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

        // Based on RandomizerMod.Menu.RandomizerMenu.StartRandomizerGame
        public void StartGame()
        {
            try
            {
                rc.Save();
                
                InitialSyncSetup();
                SessionSyncSetup();

                MenuChangerMod.HideAllMenuPages();
                ItemSyncMod.Connection.JoinRando(ItemSyncMod.ISSettings.MWRandoId, ItemSyncMod.ISSettings.MWPlayerId);
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
            int globalItemID = 0;
            if (ItemSyncMod.ISSettings.SyncVanillaItems)
                VanillaItems.AddVanillaItemsToICPlacements(rc.ctx.Vanilla);

            ItemManager.AddSyncedTags(ItemSyncMod.ISSettings.SyncVanillaItems, ref globalItemID);

            if (ItemSyncMod.ISSettings.SyncSimpleKeysUsages)
                SimpleKeysUsages.AddDoorsUnlockPlacements(ref globalItemID);

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

        internal int GetRandoHash()
        {
            return rc.Hash();
        }
    }
}
