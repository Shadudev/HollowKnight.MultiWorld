using ItemChanger.Modules;
using RandomizerMod.IC;

namespace ItemSyncMod.Randomizer
{
    internal class TransitionsFoundSyncer : Module
    {
        public override void Initialize()
        {
            TrackerUpdate.OnTransitionVisited += SendTransitionFound;
        }

        public override void Unload()
        {
            TrackerUpdate.OnTransitionVisited -= SendTransitionFound;
        }
        
        private static void SendTransitionFound(string source, string target)
        {
            ItemSyncMod.ISSettings.AddTransitionFound(source, target);
            ItemSyncMod.Connection.SendTransitionFound(source, target);
        }
    }
}
