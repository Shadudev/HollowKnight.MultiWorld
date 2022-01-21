using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class DungDefenderRoar : Roar
    {
        public override string ID => "Dung_Defender_Scream";

        public override string Scene => "Waterways_05_boss";

        public override string FSM_Name => "Dung Defender";

        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override void SavePreload(GameObject gameObject)
        {
            audio = (AudioClip) gameObject.LocateMyFSM("Dung Defender").GetState("Rage Roar")
                .GetActionsOfType<AudioPlayerOneShotSingle>()[0].audioClip.Value;
        }

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Dung Defender" && fsmName == FSM_Name && new System.Random().Next(9) == 6;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("First?").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Should we send dung defender's scream?");
                ItemSyncMod.Connection.SendItemToAll(ID);
            }));
        }
    }
}
