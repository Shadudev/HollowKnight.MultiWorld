using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class MarmuRoar : Roar
    {
        public override string ID => "Marmu_Scream";
        private AudioClip[] audioClips = new AudioClip[3];
        public override AudioClip Audio => audioClips[new System.Random().Next() % audioClips.Length];

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Ghost Warrior Marmu" && fsmName == "Control" && new System.Random().Next(9) == 8;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Start Pause").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("It do be ballin'!");
                ItemManager.SendItemToAll(ID);
            }));
        }

        public override void LoadAudioFromResources()
        {
            for (int i = 0; i < audioClips.Length; i++)
                audioClips[i] = ItemChanger.Internal.SoundManager.FromStream(
                    typeof(ItemSyncMod).Assembly.GetManifestResourceStream($"ItemSyncMod.Resources.Roars.{ID}{i}.wav"), ID);
        }
    }
}
