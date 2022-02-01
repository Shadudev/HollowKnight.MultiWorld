using ItemSyncMod.Extras.Roars;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras
{
    public class RoarSync : IAdditionalFeatureModule
    {
        public readonly List<Roar> Roars = new()
        {
            new BrettaRescue(),
            new CollectorRoar(),
            new DivinePoop(),
            new DungDefenderRoar(),
            new FlukemarmRoar(),
            new GorbRoar(),
            new LurkerRoar(),
            new MarmuRoar(),
            new MenderBugDeath(),
            new MenderBugStartle(),
            new MidwifeChomp(),
            new MylaDeath(),
            new StagBellSync()
        };

        public List<(string, string)> GetPreloadNames()
        {
            // Uncomment this and below to save sound files
            // return Roars.Select(roar => roar.GetPreloadName()).ToList();
            return new();
        }

        public void SavePreloads(Dictionary<string, Dictionary<string, GameObject>> objectsByScene)
        {
            foreach (Roar roar in Roars)
            {
                // roar.SavePreload(objectsByScene[roar.Scene][roar.FSM_Name].gameObject);
                // UnityEngine.Object.DontDestroyOnLoad(roar.Audio);
                // Or rather find it here https://www.reddit.com/r/HollowKnight/comments/9lfg10/raw_audio_files_2_electric_boogaloo/
                roar.LoadAudioFromResources();
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
