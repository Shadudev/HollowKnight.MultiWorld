using ItemSyncMod.Items;
using ItemSyncMod.Menu;
using ItemSyncMod.SyncFeatures.SimpleKeysUsages;
using ItemSyncMod.SyncFeatures.TransitionsFoundSync;
using ItemSyncMod.SyncFeatures.VisitStateChangesSync;
using MenuChanger;

namespace ItemSyncMod
{
    internal abstract class BaseController
    {
        private readonly ItemSyncMenu menu;

        public BaseController(ItemSyncMenu menu)
        {
            this.menu = menu;
        }

        internal abstract int GetHash();

        internal abstract void OnStartGame();

        // Based on RandomizerMod.Menu.RandomizerMenu.StartRandomizerGame
        public void StartGame()
        {
            try
            {
                OnStartGame();

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
            HashSet<string> existingItemIds = new();
            ItemManager.AddSyncedTags(existingItemIds, ItemSyncMod.ISSettings.SyncVanillaItems);

            if (ItemSyncMod.ISSettings.SyncSimpleKeysUsages)
                SimpleKeysUsages.AddDoorsUnlockPlacements(existingItemIds);

            TransitionsManager.Setup();
        }

        private bool hooked;
        public void SessionSyncSetup()
        {
            ItemManager.SubscribeEvents();
            VisitStateUpdater.SubscribeEvents();

            ItemSyncMod.Connection.FlushReceivedMessagesQueue();

            ItemSyncMod.Connection.JoinRando(ItemSyncMod.ISSettings.MWRandoId, ItemSyncMod.ISSettings.MWPlayerId);
            hooked = true;
        }

        internal void SessionSyncUnload()
        {
            if (!hooked) return;

            ItemManager.UnsubscribeEvents();
            VisitStateUpdater.UnsubscribeEvents();

            ItemSyncMod.Connection.Disconnect();

            hooked = false;
        }
    }
}
