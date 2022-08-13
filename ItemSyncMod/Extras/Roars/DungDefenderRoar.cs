using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class DungDefenderRoar : Roar
    {
        public override string ID => "Dung_Defender_Scream";
        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Dung Defender" && fsmName == "Dung Defender" && new System.Random().Next(9) == 6;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("First?").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Should we send dung defender's scream?");
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
