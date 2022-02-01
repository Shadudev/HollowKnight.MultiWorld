using ItemChanger.Extensions;
using UnityEngine;

namespace ItemSyncMod.Extras
{
    internal class DivinePoop : Roar
    {
        public override string ID => "Divine_poop";
        private readonly AudioClip[] audios = new AudioClip[2];
        public override AudioClip Audio => audios[new System.Random().Next() % 2];

        public override void LoadAudioFromResources()
        {
            for (int i = 0; i< audios.Length; i++)
                audios[i] = ItemChanger.Internal.SoundManager.FromStream(typeof(ItemSyncMod).
                    Assembly.GetManifestResourceStream($"ItemSyncMod.Resources.Roars.{ID}{i}.wav"), ID);
        }

        public override bool ShouldPrepare(string gameObjectName, string fsmName)
        {
            return gameObjectName == "Divine NPC" && fsmName == "Conversation Control" && new System.Random().Next(27) == 15;
        }

        public override void Prepare(PlayMakerFSM fsm)
        {
            fsm.GetState("Poo Charm").AddLastAction(new AdditionalFeatureAction(() =>
            {
                LogHelper.LogDebug("These sounds are DIVINE!");
                ItemSyncMod.Connection.SendItemToAll(ID);
            }));
        }
    }
}