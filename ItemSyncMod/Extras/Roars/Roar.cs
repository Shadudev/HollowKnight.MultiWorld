using UnityEngine;

namespace ItemSyncMod.Extras
{
    public abstract class Roar
    {
        public abstract string ID { get; }
        public abstract string Scene { get; }
        public abstract string FSM_Name { get; }
        public abstract AudioClip Audio { get; }

        public (string, string) GetPreloadName() => (Scene, FSM_Name);
        public abstract void SavePreload(GameObject gameObject);
        public abstract bool ShouldPrepare(string gameObjectName, string fsmName);
        public abstract void Prepare(PlayMakerFSM fsm);
        public abstract void LoadAudioFromResources();
    }
}
