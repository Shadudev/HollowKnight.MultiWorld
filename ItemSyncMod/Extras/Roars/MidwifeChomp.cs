using ItemChanger.Extensions;
using ItemSyncMod.Items;
using UnityEngine;

namespace ItemSyncMod.Extras
{
    internal class MidwifeChomp : Roar
    {
        public override string ID => "Midwife_Chomp";
        private AudioClip audio;
        public override AudioClip Audio => audio;

        public override void LoadAudioFromResources()
        {
            audio = ItemChanger.Internal.SoundManager.FromStream(
                typeof(ItemSyncMod).Assembly.GetManifestResourceStream($"ItemSyncMod.Resources.Roars.{ID}.wav"), ID);
        }

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Happy Spider NPC" && fsmName == "Control" && new System.Random().Next(20) == 1;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Attack").AddLastAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("Midwaifu is hangry");
                ItemManager.SendItemToAll(ID);
            }));
        }
    }
}