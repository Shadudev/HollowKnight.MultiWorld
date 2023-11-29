using ItemSyncMod.Menu;
using ItemSyncMod.SyncFeatures;
using RandomizerMod.RC;

namespace ItemSyncMod.Randomizer
{
    internal class ItemSyncRandoController : BaseController
    {
        private readonly RandoController rc;

        public ItemSyncRandoController(RandoController rc, ItemSyncMenu menu) : base(menu)
        {
            this.rc = rc;
        }

        internal override int GetHash() => rc.Hash();

        internal override void OnStartGame()
        {
            rc.Save();

            if (ItemSyncMod.ISSettings.SyncVanillaItems)
                VanillaItems.AddVanillaItemsToICPlacements(rc.ctx.Vanilla);
        }
    }
}
