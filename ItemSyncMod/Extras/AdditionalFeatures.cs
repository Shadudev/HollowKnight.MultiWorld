using UnityEngine;

namespace ItemSyncMod.Extras
{
    class AdditionalFeatures
    {
        public readonly List<IAdditionalFeatureModule> modules;
        
        internal AdditionalFeatures()
        {
            modules = new List<IAdditionalFeatureModule>()
            {
                new RoarSync()
            };
        }

        public List<(string, string)> GetPreloadNames() =>
            modules.SelectMany(module => module.GetPreloadNames()).ToList();

        internal void SavePreloads(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) =>
            modules.ForEach(module => module.SavePreloads(preloadedObjects));

        internal void Hook() => modules.ForEach(module => module.Hook());

        internal void Unhook() => modules.ForEach(module => module.Unhook());
    }
}
