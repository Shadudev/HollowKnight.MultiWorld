using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class MenderBugStartle : Roar
    {
        public override string ID => "MenderBug_Startle";
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
            fsm.GetState("Startle").AddFirstAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("You scared menderbug away :O");
                ItemManager.SendItemToAll(ID);
            }));
        }
    }
}
