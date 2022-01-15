using ItemChanger;

namespace ItemSyncMod.Items
{
    internal class SyncedVisitStateTag : Tag
    {
        public VisitState Change { get; internal set; }

        public override void Load(object parent) { }
        public override void Unload(object parent) { }
    }
}
