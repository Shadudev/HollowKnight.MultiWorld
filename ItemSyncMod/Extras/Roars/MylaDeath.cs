using ItemChanger.Extensions;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class MylaDeath : Roar
    {
        public override string ID => "Myla_Death";

        private AudioClip audio;
        public override AudioClip Audio => audio;
        
        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Zombie Myla" && fsmName == "Death Cry" && new System.Random().Next(5) < 3;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Cry").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Heartless... hkGlod");
                ItemSyncMod.Connection.SendItemToAll(ID);
            }));
        }

        public override void LoadAudioFromResources()
        {
            audio = ItemChanger.Internal.SoundManager.FromStream(
                typeof(ItemSyncMod).Assembly.GetManifestResourceStream($"ItemSyncMod.Resources.Roars.{ID}.wav"), ID);
        }
    }
}
