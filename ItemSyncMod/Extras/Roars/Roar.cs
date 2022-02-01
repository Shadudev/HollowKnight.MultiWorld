using UnityEngine;

namespace ItemSyncMod.Extras
{
    public abstract class Roar
    {
        public abstract string ID { get; }
        public abstract AudioClip Audio { get; }

        public abstract bool ShouldPrepare(string gameObjectName, string fsmName);
        public abstract void Prepare(PlayMakerFSM fsm);
        public abstract void LoadAudioFromResources();
    }
}
