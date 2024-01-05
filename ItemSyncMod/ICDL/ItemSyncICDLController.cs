using ItemChangerDataLoader;
using ItemSyncMod.Menu;
using ItemSyncMod.SyncFeatures;

namespace ItemSyncMod.ICDL
{
    internal class ItemSyncICDLController : BaseController
    {
        private readonly ICDLMenu.StartData data;

        public ItemSyncICDLController(ICDLMenu.StartData data, ItemSyncMenu menu) : base(menu)
        {
            this.data = data;
        }

        internal override int GetHash() => data.Hash();

        internal override void OnStartGame()
        {
            data.ApplySettings();

            if (ItemSyncMod.ISSettings.SyncVanillaItems)
                VanillaItems.AddVanillaItemsToICPlacements(data.CTX.Vanilla);

            ICDLMod.LocalSettings.IsICDLSave = true;
        }
    }
}
