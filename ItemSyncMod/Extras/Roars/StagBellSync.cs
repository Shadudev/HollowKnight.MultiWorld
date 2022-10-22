using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class StagBellSync : Roar
    {
        public override string ID => "bell_hit";

        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Bell" && fsmName == "Bell Control" && new System.Random().Next(100) == 69;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Detect").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Ring ring...");
                ItemManager.SendItemToAll(ID);
            }));
        }

        public override void LoadAudioFromResources()
        {
            audio = ItemChanger.Internal.SoundManager.FromStream(
                typeof(ItemSyncMod).Assembly.GetManifestResourceStream($"ItemSyncMod.Resources.Roars.{ID}.wav"), ID);
        }
    }
}
