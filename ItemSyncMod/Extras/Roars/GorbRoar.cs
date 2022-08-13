using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class GorbRoar : Roar
    {
        public override string ID => "Gorb_Scream";

        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Ghost Warrior Slug" && fsmName == "Attacking" && new System.Random().Next(9) == 7;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Init").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Should we send gorb's scream?");
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
