using ItemSyncMod.Extras.Roars;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras
{
    public class RoarSync : IAdditionalFeatureModule
    {
        public readonly List<Roar> Roars = new()
        {
            new CollectorRoar(),
            new DungDefenderRoar(),
            new FlukemarmRoar(),
            new GorbRoar(),
            new LurkerRoar(),
            new MarmuRoar()
        };

        public List<(string, string)> GetPreloadNames()
        {
#if DEBUG
            return Roars.Select(roar => roar.GetPreloadName()).ToList();
#else
            return new();
#endif
        }

        public void SavePreloads(Dictionary<string, Dictionary<string, GameObject>> objectsByScene)
        {
            foreach (Roar roar in Roars)
            {
#if DEBUG
                roar.SavePreload(objectsByScene[roar.Scene][roar.FSM_Name].gameObject);
                UnityEngine.Object.DontDestroyOnLoad(roar.Audio);
#else
                roar.LoadAudioFromResources();
#endif
            }
        }

        public void Hook()
        {
            On.PlayMakerFSM.OnEnable += PrepareByFSMEnabled;
            ItemManager.OnGiveItem += OnItemGive;
        }

        public void Unhook()
        {
            On.PlayMakerFSM.OnEnable -= PrepareByFSMEnabled;
            ItemManager.OnGiveItem -= OnItemGive;
        }

        internal void PrepareByFSMEnabled(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            foreach (Roar roar in Roars)
            {
                if (roar.ShouldPrepare(self.gameObject.name, self.FsmName))
                {
                    roar.Prepare(self);
                }
            }
        }

        internal void OnItemGive(string itemId)
        {
            foreach (Roar roar in Roars)
                if (roar.ID == itemId)
                    AudioPlayer.PlayAudio(roar.Audio);
        }
    }
}
