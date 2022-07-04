using System;
using System.Collections.Generic;
using MultiWorldLib;
using MultiWorldLib.Messaging;
using MultiWorldLib.Messaging.Definitions.Messages;

namespace MultiWorldServer
{
    class GameSession
    {
        private readonly int randoId;
        private readonly Dictionary<int, PlayerSession> players;
        private readonly Dictionary<int, string> nicknames;
        private readonly Dictionary<int, Dictionary<int, int>> playersCharmsNotchCosts;

        // These are to try to prevent items being lost. When items are sent, they go to unconfirmed. Once the confirmation message is received,
        // they are moved to unsaved items. When we receive a message letting us know that 
        private readonly Dictionary<int, HashSet<MWConfirmableMessage>> unconfirmedMessages;
        private readonly Dictionary<int, HashSet<MWConfirmableMessage>> unsavedMessages;
        private readonly bool isMultiWorld;

        public GameSession(int id, bool isItemSync)
        {
            randoId = id;
            players = new Dictionary<int, PlayerSession>();
            nicknames = new Dictionary<int, string>();
            unconfirmedMessages = new Dictionary<int, HashSet<MWConfirmableMessage>>();
            unsavedMessages = new Dictionary<int, HashSet<MWConfirmableMessage>>();
            isMultiWorld = !isItemSync;

            if (isItemSync)
                playersCharmsNotchCosts = null;
            else
                playersCharmsNotchCosts = new Dictionary<int, Dictionary<int, int>>();
        }

        public GameSession(int id, List<int> playersIds, bool isItemSync) : this(id, isItemSync)
        {
            foreach (int playerId in playersIds)
                players[playerId] = null;
        }

        // We know that the client received the message, but until the game is saved we can't be sure it isn't lost in a crash
        public void ConfirmItem(int playerId, MWItemReceiveMessage msg)
        {
            unconfirmedMessages.GetOrCreateDefault(playerId).Remove(msg);
            unsavedMessages.GetOrCreateDefault(playerId).Add(msg);
            Server.Log($"Confirmed {msg.Item} received by '{players[playerId]?.Name}' ({playerId})", randoId);
        }

        public void ConfirmVisitStateChanged(int playerId, MWVisitStateChangedMessage msg)
        {
            unconfirmedMessages.GetOrCreateDefault(playerId).Remove(msg);
            unsavedMessages.GetOrCreateDefault(playerId).Add(msg);
            Server.LogDebug($"Confirmed {msg.Name} visit state change received by '{players[playerId]?.Name}' ({playerId})", randoId);
        }

        public void ConfirmTransitionFound(int playerId, MWTransitionFoundMessage msg)
        {
            unconfirmedMessages.GetOrCreateDefault(playerId).Remove(msg);
            unsavedMessages.GetOrCreateDefault(playerId).Add(msg);
            Server.LogDebug($"Confirmed {msg.Target} transition found received by '{players[playerId]?.Name}' ({playerId})", randoId);
        }

        // If items have been both confirmed and the player saves and we STILL lose the item, they didn't deserve it anyway
        public void Save(int playerId)
        {
            Server.Log($"Player '{players[playerId]?.Name}' ({playerId}) saved. Clearing {unsavedMessages.GetOrCreateDefault(playerId).Count} messages", randoId);
            unsavedMessages[playerId].Clear();
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

            Server.Log($"Player {join.PlayerId} joined session {join.RandoId}", randoId);

            if (unconfirmedMessages.ContainsKey(join.PlayerId))
            {
                foreach (var msg in unconfirmedMessages.GetOrCreateDefault(join.PlayerId))
                {
                    players[join.PlayerId].QueueConfirmableMessage(msg);
                }
            }

            // false for ItemSync rooms
            if (playersCharmsNotchCosts != null)
            {
                lock (playersCharmsNotchCosts)
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
        }

        internal bool IsMultiWorld() => isMultiWorld;

        public void RemovePlayer(Client c)
        {
            if (!players.ContainsKey(c.Session.playerId) || players[c.Session.playerId] == null) return;

            // See above in add player, if someone disconnects and rejoins before RemovePlayer is called, then their new session will get removed and they
            // will be in a weird limbo state. So, if the connection associated with this session doesn't match, then don't remove the player, since it
            // was on a new connection
            if (c.UID != players[c.Session.playerId].uid)
            {
                Server.Log($"Trying to remove player {c.Session.playerId} but UIDs mismatch ({c.UID} != {players[c.Session.playerId].uid}). Stale connection?", randoId);
                return;
            }
            Server.Log($"Player {c.Session.playerId} removed from session {c.Session.randoId}", randoId);
            players[c.Session.playerId] = null;

            // If there are unsaved items when player is leaving, copy them to unconfirmed to be resent later
            MoveUnsavedToUnconfirmed(c.Session.playerId);
        }

        private void MoveUnsavedToUnconfirmed(int playerId)
        {
            if (unsavedMessages.ContainsKey(playerId))
            {
                unconfirmedMessages.GetOrCreateDefault(playerId).UnionWith(unsavedMessages[playerId]);
                unsavedMessages[playerId].Clear();
            }
        }

        public void SendItem(Item item, string fromNickname)
        {
            MWItemReceiveMessage msg = new MWItemReceiveMessage { From = fromNickname, Item = item };
            if (players.ContainsKey(item.OwnerID) && players[item.OwnerID] != null)
            {
                Server.Log($"Sending item '{item}' from '{fromNickname}' to '{players[item.OwnerID].Name}'", randoId);
                Server.QueuePushMessage(players[item.OwnerID].uid, msg);
                players[item.OwnerID].QueueConfirmableMessage(msg);
            }

            // Always add to unconfirmed, which doubles as holding items for offline players
            unconfirmedMessages.GetOrCreateDefault(item.OwnerID).Add(msg);
        }

        public void SendItem(Item item, int fromId)
        {
            SendItem(item, nicknames[fromId]);
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
                    playersStrings.Add($"{kvp.Key}: {kvp.Value.Name}");

            return string.Join(", ", playersStrings.ToArray());
        }

        internal void AnnouncePlayerCharmNotchCosts(int playerId, MWAnnounceCharmNotchCostsMessage message)
        {
            lock (playersCharmsNotchCosts)
            {
                playersCharmsNotchCosts[playerId] = message.Costs;
                foreach (var kvp in players)
                {
                    if (kvp.Key == playerId || kvp.Value == null) continue;
                    
                    kvp.Value.QueueConfirmableMessage(message);
                }
            }
        }

        internal void SendItemToAll(Item item, int playerId)
        {
            foreach (var kvp in players)
            {
                if (kvp.Key == playerId) continue;

                SendItem(item, playerId);
            }
        }

        internal void SendItemsTo(int toId, List<Item> items, int fromId)
        {
            MWItemsReceiveMessage msg = new MWItemsReceiveMessage { Items = items, From = nicknames[fromId] };
            if (players.TryGetValue(toId, out var playerSession) && playerSession != null)
                playerSession.QueueConfirmableMessage(msg);
            
            unconfirmedMessages.GetOrCreateDefault(toId).Add(msg);
        }
    }
}
