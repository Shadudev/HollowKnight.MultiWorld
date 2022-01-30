using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class GorbRoar : Roar
    {
        public override string ID => "Gorb_Scream";

        public override string Scene => "Cliffs_02_boss";

        public override string FSM_Name => "Warrior";

        private AudioClip audio;
        public override AudioClip Audio => audio;


        public override void SavePreload(GameObject gameObject)
        {
            audio = (AudioClip)gameObject.transform.Find("Ghost Warrior Slug").gameObject
                .LocateMyFSM("Attacking").GetState("Init").
                GetActionsOfType<AudioPlayerOneShotSingle>()[0].audioClip.Value;
        }

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Ghost Warrior Slug" && fsmName == "Attacking" && new System.Random().Next(9) == 7;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Init").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Should we send gorb's scream?");
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
