using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    public class FlukemarmRoar : Roar
    {
        public override string ID => "Flukemarm_Scream";
        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Fluke Mother" && fsmName == "Fluke Mother" && new System.Random().Next(12) == 5;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Roar Start").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Do I send " + ID);
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
