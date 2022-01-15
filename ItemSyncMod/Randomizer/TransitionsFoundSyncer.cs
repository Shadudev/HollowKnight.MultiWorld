using ItemChanger.Modules;
using RandomizerMod.IC;
using RandomizerMod.RC;

namespace ItemSyncMod.Randomizer
{
    internal class TransitionsFoundSyncer : Module
    {
        public override void Initialize()
        {
            if (RandomizerMod.RandomizerMod.RS.GenerationSettings.TransitionSettings.Mode != 
                    RandomizerMod.Settings.TransitionSettings.TransitionMode.None)
            {
                Unload();
                TrackerUpdate.OnTransitionVisited += SendTransitionFound;
            }
        }

        public override void Unload()
        {
            TrackerUpdate.OnTransitionVisited -= SendTransitionFound;
        }
        
        private static void SendTransitionFound(string source, string target)
        {
            ItemSyncMod.Connection.SendTransitionFound(source, target);
        }
    }
}
