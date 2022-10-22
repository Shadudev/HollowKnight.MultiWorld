using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class LurkerRoar : Roar
    {
        public override string ID => "Lurker_Scream";
        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Lurker Intro" && fsmName == "Intro" && new System.Random().Next(12) == 1;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Roar").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("That's loud...");
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
