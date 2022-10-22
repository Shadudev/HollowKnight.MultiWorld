using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras.Roars
{
    internal class BrettaRescue : Roar
    {
        public override string ID => "Bretta_rescue";
        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Bretta Dazed" && fsmName == "Conversation Control" && new System.Random().Next(6) == 2;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Meet 3").AddLastAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("So kind, too bad only Zote's on that mind...");
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
