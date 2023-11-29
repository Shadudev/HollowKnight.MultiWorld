﻿using ItemSyncMod.Items;
using ItemSyncMod.Menu;
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
        private readonly ItemSyncMenu menu;

        public ItemSyncController() : this(null, null) { }


        public ItemSyncController(RandoController rc, ItemSyncMenu menu)
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

        internal int GetRandoHash()
        {
            return rc.Hash();
        }
    }
}
