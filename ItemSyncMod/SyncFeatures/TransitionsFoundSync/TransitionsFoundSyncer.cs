using ItemChanger.Modules;
using Newtonsoft.Json;
using RandomizerMod.IC;

namespace ItemSyncMod.SyncFeatures.TransitionsFoundSync
{
    public class TransitionFound
    {
        public string source, target;
    }

    public class TransitionsFoundSyncer : Module
    {
        public static readonly string TRANSITION_MESSAGE_LABEL = "ItemSync-Transition";

        public override void Initialize()
        {
            TrackerUpdate.OnTransitionVisited += SendTransitionFound;
            ItemSyncMod.Connection.OnDataReceived += HandleTransitionFound;
        }

        private void HandleTransitionFound(ClientConnection.DataReceivedEvent dataReceivedEvent)
        {
            if (dataReceivedEvent.Label != TRANSITION_MESSAGE_LABEL) return;

            TransitionFound transitionFound = JsonConvert.DeserializeObject<TransitionFound>(dataReceivedEvent.Data);
            TransitionsManager.MarkTransitionFound(transitionFound.source, transitionFound.target);
            dataReceivedEvent.Handled = true;
        }

        public override void Unload()
        {
            TrackerUpdate.OnTransitionVisited -= SendTransitionFound;
        }
        
        private static void SendTransitionFound(string source, string target)
        {
            ItemSyncMod.Connection.SendDataToAll(TRANSITION_MESSAGE_LABEL,
                JsonConvert.SerializeObject(new TransitionFound() { source = source, target = target }));
        }
    }
}
