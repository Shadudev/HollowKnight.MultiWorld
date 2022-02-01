using ItemSyncMod.Extras.HardFallSync;
using UnityEngine;

namespace ItemSyncMod.Extras
{
    class AdditionalFeatures
    {
        public readonly List<IAdditionalFeatureModule> modules;

        private bool initialized = false;
        internal AdditionalFeatures()
        {
            modules = new List<IAdditionalFeatureModule>()
            {
                new RoarSync(),
                new HardFallSoundSync()
            };
        }

        public List<(string, string)> GetPreloadNames() =>
            modules.SelectMany(module => module.GetPreloadNames()).ToList();

        internal void SavePreloads(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            initialized = true;
            modules.ForEach(module => module.SavePreloads(preloadedObjects));
        }

        internal void Hook()
        {
            if (initialized)
                modules.ForEach(module => module.Hook());
        }

        internal void Unhook()
        {
            if (initialized)
                modules.ForEach(module => module.Unhook());
        }
    }
}
