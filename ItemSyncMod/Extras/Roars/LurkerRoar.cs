using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class LurkerRoar : Roar
    {
        public override string ID => "Lurker_Scream";

        public override string Scene => "GG_Lurker";

        public override string FSM_Name => "Lurker Control";

        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override void SavePreload(GameObject gameObject)
        {
            audio = (AudioClip) gameObject.transform.Find("Lurker Intro").gameObject.LocateMyFSM("Intro").GetState("Roar")
                .GetActionsOfType<AudioPlayerOneShotSingle>()[0].audioClip.Value;
        }

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Lurker Intro" && fsmName == "Intro" && new System.Random().Next(9) == 1;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Roar").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("That's loud...");
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
