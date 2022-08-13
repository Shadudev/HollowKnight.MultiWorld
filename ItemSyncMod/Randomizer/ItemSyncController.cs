using ItemSyncMod.Items;
using ItemSyncMod.SyncFeatures;
using ItemSyncMod.SyncFeatures.SimpleKeysUsages;
using ItemSyncMod.SyncFeatures.TransitionsFoundSync;
using ItemSyncMod.SyncFeatures.VisitStateChangesSync;
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
            if (ItemSyncMod.ISSettings.SyncVanillaItems)
                VanillaItems.AddVanillaItemsToICPlacements(rc.ctx.Vanilla);

            HashSet<string> existingItemIds = new();
            ItemManager.AddSyncedTags(existingItemIds, ItemSyncMod.ISSettings.SyncVanillaItems);

            if (ItemSyncMod.ISSettings.SyncSimpleKeysUsages)
                SimpleKeysUsages.AddDoorsUnlockPlacements(existingItemIds);

            TransitionsManager.Setup();
        }

        public static void SessionSyncSetup()
        {
            ItemManager.SubscribeEvents();
            VisitStateUpdater.SubscribeEvents();

            if (ItemSyncMod.ISSettings.AdditionalFeaturesEnabled)
                ItemSyncMod.AdditionalFeatures.Hook();
        }

        internal void SessionSyncUnload()
        {
            VisitStateUpdater.UnsubscribeEvents();
            
            if (ItemSyncMod.ISSettings.AdditionalFeaturesEnabled)
                ItemSyncMod.AdditionalFeatures.Unhook();
        }

        internal int GetRandoHash()
        {
            return rc.Hash();
        }
    }
}
