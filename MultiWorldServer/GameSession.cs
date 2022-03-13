using System;
using System.Collections.Generic;
using ItemChanger;
using MultiWorldLib.Messaging.Definitions.Messages;

namespace MultiWorldServer
{
    class GameSession
    {
        private readonly int randoId;
        private readonly Dictionary<int, PlayerSession> players;
        private readonly Dictionary<int, string> nicknames;
        private readonly Dictionary<int, int[]> playersCharmsNotchCosts;

        // These are to try to prevent items being lost. When items are sent, they go to unconfirmed. Once the confirmation message is received,
        // they are moved to unsaved items. When we receive a message letting us know that 
        private readonly Dictionary<int, HashSet<MWItemReceiveMessage>> unconfirmedItems;
        private readonly Dictionary<int, HashSet<MWItemReceiveMessage>> unsavedItems;

        private readonly Dictionary<int, HashSet<MWVisitStateChangedMessage>> unconfirmedVisitStateChanges;
        private readonly Dictionary<int, HashSet<MWVisitStateChangedMessage>> unsavedVisitStateChanges;

        private readonly Dictionary<int, HashSet<MWTransitionFoundMessage>> unconfirmedTransitionsFound;
        private readonly Dictionary<int, HashSet<MWTransitionFoundMessage>> unsavedTransitionsFound;

        public GameSession(int id, bool isItemSync)
        {
            randoId = id;
            players = new Dictionary<int, PlayerSession>();
            nicknames = new Dictionary<int, string>();
            unconfirmedItems = new Dictionary<int, HashSet<MWItemReceiveMessage>>();
            unsavedItems = new Dictionary<int, HashSet<MWItemReceiveMessage>>();
            unconfirmedVisitStateChanges = new Dictionary<int, HashSet<MWVisitStateChangedMessage>>();
            unsavedVisitStateChanges = new Dictionary<int, HashSet<MWVisitStateChangedMessage>>();
            unconfirmedTransitionsFound = new Dictionary<int, HashSet<MWTransitionFoundMessage>>();
            unsavedTransitionsFound = new Dictionary<int, HashSet<MWTransitionFoundMessage>>();

            if (isItemSync)
                playersCharmsNotchCosts = null;
            else
                playersCharmsNotchCosts = new Dictionary<int, int[]>();
        }

        public GameSession(int id, List<int> playersIds, bool isItemSync) : this(id, isItemSync)
        {
            foreach (int playerId in playersIds)
                players[playerId] = null;
        }

        // We know that the client received the message, but until the game is saved we can't be sure it isn't lost in a crash
        public void ConfirmItem(int playerId, MWItemReceiveMessage msg)
        {
            unconfirmedItems.GetOrCreateDefault(playerId).Remove(msg);
            unsavedItems.GetOrCreateDefault(playerId).Add(msg);
            Server.Log($"Confirmed {msg.Item} received by '{players[playerId]?.Name}' ({playerId + 1}). Unconfirmed: {unconfirmedItems[playerId].Count} Unsaved: {unsavedItems[playerId].Count}", randoId);
        }

        public void ConfirmVisitStateChanged(int playerId, MWVisitStateChangedMessage msg)
        {
            unconfirmedVisitStateChanges.GetOrCreateDefault(playerId).Remove(msg);
            unsavedVisitStateChanges.GetOrCreateDefault(playerId).Add(msg);
            Server.LogDebug($"Confirmed {msg.Name} visit state change received by '{players[playerId]?.Name}' ({playerId + 1}). Unconfirmed: {unconfirmedVisitStateChanges[playerId].Count} Unsaved: {unsavedVisitStateChanges.GetOrCreateDefault(playerId).Count}", randoId);
        }

        public void ConfirmTransitionFound(int playerId, MWTransitionFoundMessage msg)
        {
            unconfirmedTransitionsFound.GetOrCreateDefault(playerId).Remove(msg);
            unsavedTransitionsFound.GetOrCreateDefault(playerId).Add(msg);
            Server.LogDebug($"Confirmed {msg.Target} transition found received by '{players[playerId]?.Name}' ({playerId + 1}). Unconfirmed: {unconfirmedTransitionsFound.GetOrCreateDefault(playerId).Count} Unsaved: {unsavedTransitionsFound.GetOrCreateDefault(playerId).Count}", randoId);
        }

        // If items have been both confirmed and the player saves and we STILL lose the item, they didn't deserve it anyway
        public void Save(int playerId)
        {
            Server.Log($"Player '{players[playerId]?.Name}' ({playerId + 1}) saved. Clearing {unsavedItems.GetOrCreateDefault(playerId).Count} items, {unsavedVisitStateChanges.GetOrCreateDefault(playerId).Count} visit state changes, {unsavedTransitionsFound.GetOrCreateDefault(playerId).Count} transitions found", randoId);
            unsavedItems[playerId].Clear();
            unsavedVisitStateChanges[playerId].Clear();
            unsavedTransitionsFound[playerId].Clear();
        }

        public void AddPlayer(Client c, MWJoinMessage join)
        {
            // If a player disconnects and rejoins before they can be removed from game session, you can have a weird order of events
            if (players.ContainsKey(join.PlayerId) && players[join.PlayerId] != null)
            {
                // In this case, make sure that their unsaved items from before are protected
                MoveUnsavedToUnconfirmed(join.PlayerId);
            }

            if (!nicknames.ContainsKey(join.PlayerId))
                nicknames[join.PlayerId] = join.DisplayName;

            PlayerSession session = new PlayerSession(join.DisplayName, join.RandoId, join.PlayerId, c.UID);
            players[join.PlayerId] = session;
            c.Session = session;

            Server.Log($"Player {join.PlayerId + 1} joined session {join.RandoId}", randoId);

            if (unconfirmedItems.ContainsKey(join.PlayerId))
            {
                foreach (var msg in unconfirmedItems.GetOrCreateDefault(join.PlayerId))
                {
                    Server.Log($"Resending {msg.Item} to '{join.DisplayName}' ({join.PlayerId + 1}) on join", randoId);
                    players[join.PlayerId].QueueConfirmableMessage(msg);
                }
            }
            if (unconfirmedVisitStateChanges.ContainsKey(join.PlayerId))
            {
                Server.Log($"Resending {unconfirmedVisitStateChanges.GetOrCreateDefault(join.PlayerId).Count} visit state changes to '{join.DisplayName}' ({join.PlayerId + 1}) on join", randoId);
                foreach (var msg in unconfirmedVisitStateChanges.GetOrCreateDefault(join.PlayerId))
                    players[join.PlayerId].QueueConfirmableMessage(msg);
            }
            if (unconfirmedTransitionsFound.ContainsKey(join.PlayerId))
            {
                Server.Log($"Resending {unconfirmedTransitionsFound.GetOrCreateDefault(join.PlayerId).Count} transitions found to '{join.DisplayName}' ({join.PlayerId + 1}) on join", randoId);
                foreach (var msg in unconfirmedTransitionsFound.GetOrCreateDefault(join.PlayerId))
                    players[join.PlayerId].QueueConfirmableMessage(msg);
            }

            if (playersCharmsNotchCosts != null)
            {
                if (!playersCharmsNotchCosts.ContainsKey(join.PlayerId))
                {
                    players[join.PlayerId].QueueConfirmableMessage(new MWRequestCharmNotchCostsMessage());
                }

                foreach (var kvp in playersCharmsNotchCosts)
                {
                    if (kvp.Key == join.PlayerId) continue;
                    session.QueueConfirmableMessage(new MWAnnounceCharmNotchCostsMessage { PlayerID = kvp.Key, Costs = kvp.Value });
                }
            }
        }

        public void RemovePlayer(Client c)
        {
            if (!players.ContainsKey(c.Session.playerId) || players[c.Session.playerId] == null) return;

            // See above in add player, if someone disconnects and rejoins before RemovePlayer is called, then their new session will get removed and they
            // will be in a weird limbo state. So, if the connection associated with this session doesn't match, then don't remove the player, since it
            // was on a new connection
            if (c.UID != players[c.Session.playerId].uid)
            {
                Server.Log($"Trying to remove player {c.Session.playerId + 1} but UIDs mismatch ({c.UID} != {players[c.Session.playerId].uid}). Stale connection?", randoId);
                return;
            }
            Server.Log($"Player {c.Session.playerId + 1} removed from session {c.Session.randoId}", randoId);
            players[c.Session.playerId] = null;

            // If there are unsaved items when player is leaving, copy them to unconfirmed to be resent later
            MoveUnsavedToUnconfirmed(c.Session.playerId);
        }

        private void MoveUnsavedToUnconfirmed(int playerId)
        {
            if (unsavedItems.ContainsKey(playerId))
            {
                unconfirmedItems.GetOrCreateDefault(playerId).UnionWith(unsavedItems[playerId]);
                unsavedItems[playerId].Clear();
            }
            if (unsavedVisitStateChanges.ContainsKey(playerId))
            {
                unconfirmedVisitStateChanges.GetOrCreateDefault(playerId).UnionWith(unsavedVisitStateChanges[playerId]);
                unsavedVisitStateChanges[playerId].Clear();
            }
            if (unsavedTransitionsFound.ContainsKey(playerId))
            {
                unconfirmedTransitionsFound.GetOrCreateDefault(playerId).UnionWith(unsavedTransitionsFound[playerId]);
                unsavedTransitionsFound[playerId].Clear();
            }
        }

        public void SendItemTo(int player, string item, string location, string from)
        {
            MWItemReceiveMessage msg = new MWItemReceiveMessage { Location = location, From = from, Item = item };
            if (players.ContainsKey(player) && players[player] != null)
            {
                Server.Log($"Sending item '{item}' from '{from}' to '{players[player].Name}'", randoId);
                Server.QueuePushMessage(players[player].uid, msg);
            }

            // Always add to unconfirmed, which doubles as holding items for offline players
            unconfirmedItems.GetOrCreateDefault(player).Add(msg);
        }

        // Strictly ItemSync functionality
        public void SendVisitStateChange(MWVisitStateChangedMessage message, int sender)
        {
            Server.LogDebug($"Sending '{message.Name}' visit state change with new flags: {message.NewVisitFlags}", randoId);
            foreach (int player in players.Keys)             
                if (player != sender && players[player] != null && player != sender)
                    players[player].QueueConfirmableMessage(message);
        }

        public void SendTransitionFound(string source, string target, int sender)
        {
            Server.LogDebug($"Sending transition found '{source}->{target}'", randoId);
            MWTransitionFoundMessage msg = new MWTransitionFoundMessage { Source = source, Target = target };
            foreach (int player in players.Keys)
                if (player != sender && players[player] != null && player != sender)
                    players[player].QueueConfirmableMessage(msg);
        }

        public string GetPlayerString()
        {
            if (players.Count == 0) return "";

            List<string> playersStrings = new List<string>();
            foreach (var kvp in players)
                if (kvp.Value != null)
                    playersStrings.Add($"{kvp.Key + 1}: {kvp.Value.Name}");

            return string.Join(", ", playersStrings.ToArray());
        }

        internal void SendItemTo(int toId, string item, string location, int fromId)
        {
            SendItemTo(toId, item, location, nicknames[fromId]);
        }

        internal void AnnouncePlayerCharmNotchCosts(int playerId, MWAnnounceCharmNotchCostsMessage message)
        {
            playersCharmsNotchCosts[playerId] = message.Costs;
            foreach (var kvp in players)
            {
                if (kvp.Key == playerId || kvp.Value == null) continue;

                kvp.Value.QueueConfirmableMessage(message);
            }
        }

        internal void SendItemToAll(string item, string location, int playerId)
        {
            foreach (var kvp in players)
            {
                if (kvp.Key == playerId) continue;

                SendItemTo(kvp.Key, item, location, playerId);
            }
        }

        // Todo better implementation for multiworld eject to avoid stressing push mechanism
        internal void SendItemsTo(int toId, List<(string, string)> items, int fromId)
        {
            /*
            MWItemsReceiveMessage msg = new MWItemsReceiveMessage { Items = items, from = nicknames[fromId] };
            if (players.TryGetValue(toId, out var playerSession) && playerSession != null)
                playerSession.QueueConfirmableMessage(msg);
            */
    }

}
}
