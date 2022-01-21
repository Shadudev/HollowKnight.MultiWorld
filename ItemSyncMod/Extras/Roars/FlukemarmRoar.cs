using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    public class FlukemarmRoar : Roar
    {
        public override string ID => "Flukemarm_Scream";

        public override string Scene => "Waterways_12_boss";

        public override string FSM_Name => "Fluke Mother";

        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override void SavePreload(GameObject gameObject)
        {
            audio = (AudioClip) gameObject.LocateMyFSM("Fluke Mother").GetState("Roar Start")
                .GetActionsOfType<AudioPlayerOneShotSingle>()[0].audioClip.Value;
        }

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Fluke Mother" && fsmName == FSM_Name && new System.Random().Next(12) == 5;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Roar Start").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Do I send " + ID);
                ItemSyncMod.Connection.SendItemToAll(ID);
            }));
        }
    }
}
