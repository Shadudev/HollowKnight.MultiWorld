using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class CollectorRoar : Roar
    {
        public override string ID => "Collector_Scream";
        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Jar Collector" && fsmName == "Control" && new System.Random().Next(9) == 3;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Roar").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Should we send the collector's scream?");
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
