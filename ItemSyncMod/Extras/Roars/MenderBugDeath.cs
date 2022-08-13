using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class MenderBugDeath : Roar
    {
        public override string ID => "MenderBug_Death";
        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override void LoadAudioFromResources()
        {
            audio = ItemChanger.Internal.SoundManager.FromStream(
                typeof(ItemSyncMod).Assembly.GetManifestResourceStream($"ItemSyncMod.Resources.Roars.{ID}.wav"), ID);
        }

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Mender Bug" && fsmName == "Mender Bug Ctrl";
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Killed").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("YOU KILLED MENDERBUG?!");
                ItemManager.SendItemToAll(ID);
            }));
        }
    }
}
