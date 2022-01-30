using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class CollectorRoar : Roar
    {
        public override string ID => "Collector_Scream";

        public override string Scene => "Ruins2_11_boss";

        public override string FSM_Name => "Battle Scene";

        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override void SavePreload(GameObject gameObject)
        {
            audio = (AudioClip)gameObject.transform.Find("Jar Collector").gameObject
                .LocateMyFSM("Control").GetState("Roar")
                .GetActionsOfType<AudioPlaySimple>()[0].oneShotClip.Value;
        }

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Jar Collector" && fsmName == "Control" && new System.Random().Next(9) == 3;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Roar").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Should we send the collector's scream?");
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
