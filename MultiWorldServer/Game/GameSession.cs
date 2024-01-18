using System;
using System.Collections.Generic;
using MultiWorldLib;
using MultiWorldLib.Messaging;
using MultiWorldLib.Messaging.Definitions.Messages;
using MultiWorldServer.Loggers;

namespace MultiWorldServer.Game
{
    [Serializable]
    public class GameSession
    {
        private readonly int randoId;
        private readonly Dictionary<int, PlayerSession> players;
        private readonly Dictionary<int, string> nicknames;
        private readonly Dictionary<int, Dictionary<int, int>> playersCharmsNotchCosts;

        [NonSerialized]
        private LogWriter logWriter;

        // These are to try to prevent datas being lost. When datas are sent, they go to unconfirmed. Once the confirmation message is received,
        // they are moved to unsaved datas. When we receive a message letting us know that 
        private readonly Dictionary<int, HashSet<MWConfirmableMessage>> unconfirmedMessages;
        private readonly Dictionary<int, HashSet<MWConfirmableMessage>> unsavedMessages;
        private readonly Mode mode;

        [NonSerialized]
        public Action<int, Dictionary<int, string>> OnConnectedPlayersChanged;

        public GameSession(int id, Mode mode, LogWriter logWriter)
        {
            randoId = id;
            this.logWriter = logWriter;
            players = new Dictionary<int, PlayerSession>();
            nicknames = new Dictionary<int, string>();
            unconfirmedMessages = new Dictionary<int, HashSet<MWConfirmableMessage>>();
            unsavedMessages = new Dictionary<int, HashSet<MWConfirmableMessage>>();
            this.mode = mode;

            if (mode == Mode.ItemSync)
                playersCharmsNotchCosts = null;
            else
                playersCharmsNotchCosts = new Dictionary<int, Dictionary<int, int>>();

            OnConnectedPlayersChanged = null;
        }

        public GameSession(int id, List<int> playerIds, Mode mode, LogWriter logWriter) : this(id, mode, logWriter)
        {
            foreach (int playerId in playerIds)
                players[playerId] = null;
        }

        // We know that the client received the message, but until the game is saved we can't be sure it isn't lost in a crash
        public void ConfirmData(int playerId, MWDataReceiveMessage msg)
        {
            unconfirmedMessages.GetOrCreateDefault(playerId).Remove(msg);
            unsavedMessages.GetOrCreateDefault(playerId).Add(msg);
            logWriter.LogDebug($"Confirmed {msg.Label} received by '{players[playerId]?.Name}' ({playerId})", randoId);
        }

        internal void SetLogger(LogWriter logWriter)
        {
            this.logWriter = logWriter;
        }

        // If datas have been both confirmed and the player saves and we STILL lose the data, they didn't deserve it anyway
        public void Save(int playerId)
        {
            logWriter.Log($"Player '{players[playerId]?.Name}' ({playerId}) saved. Clearing {unsavedMessages.GetOrCreateDefault(playerId).Count} messages", randoId);
            unsavedMessages[playerId].Clear();
        }

        public void AddPlayer(Client c, MWJoinMessage join)
        {
            // If a player disconnects and rejoins before they can be removed from game session, you can have a weird order of events
            if (players.ContainsKey(join.PlayerId) && players[join.PlayerId] != null)
            {
                // In this case, make sure that their unsaved datas from before are protected
                MoveUnsavedToUnconfirmed(join.PlayerId);
            }

            if (!nicknames.ContainsKey(join.PlayerId))
                nicknames[join.PlayerId] = join.DisplayName;

            PlayerSession session = new PlayerSession(join.DisplayName, join.RandoId, join.PlayerId, c.UID);
            players[join.PlayerId] = session;
            c.Session = session;

            logWriter.Log($"Player {join.PlayerId} joined session {join.RandoId}", randoId);

            if (unconfirmedMessages.ContainsKey(join.PlayerId))
            {
                foreach (var msg in unconfirmedMessages.GetOrCreateDefault(join.PlayerId))
                {
                    players[join.PlayerId].QueueConfirmableMessage(msg);
                }
            }

            // Always false for ItemSync rooms
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

            InvokeConnectedPlayersChanged();
        }

        internal Mode GetMode() => mode;
        internal int GetRandoId() => randoId;

        public void RemovePlayer(Client c)
        {
            if (!players.ContainsKey(c.Session.playerId) || players[c.Session.playerId] == null) return;

            // See above in add player, if someone disconnects and rejoins before RemovePlayer is called, then their new session will get removed and they
            // will be in a weird limbo state. So, if the connection associated with this session doesn't match, then don't remove the player, since it
            // was on a new connection
            if (c.UID != players[c.Session.playerId].uid)
            {
                logWriter.Log($"Trying to remove player {c.Session.playerId} but UIDs mismatch ({c.UID} != {players[c.Session.playerId].uid}). Stale connection?", randoId);
                return;
            }
            logWriter.Log($"Player {c.Session.playerId} removed from session {c.Session.randoId}", randoId);
            players[c.Session.playerId] = null;

            // If there are unsaved datas when player is leaving, copy them to unconfirmed to be resent later
            MoveUnsavedToUnconfirmed(c.Session.playerId);
            
            InvokeConnectedPlayersChanged();
        }

        private void InvokeConnectedPlayersChanged()
        {
            Dictionary<int, string> connectedPlayers = new Dictionary<int, string>();
            foreach (var playerID in players.Keys)
            {
                if (players[playerID] != null)
                    connectedPlayers[playerID] = nicknames[playerID];
            }
            OnConnectedPlayersChanged?.Invoke(randoId, connectedPlayers);
        }

        private void MoveUnsavedToUnconfirmed(int playerId)
        {
            if (unsavedMessages.ContainsKey(playerId))
            {
                unconfirmedMessages.GetOrCreateDefault(playerId).UnionWith(unsavedMessages[playerId]);
                unsavedMessages[playerId].Clear();
            }
        }

        public void SendDataTo(string label, string data, int player, string from, int fromId, int ttl)
        {
            MWDataReceiveMessage msg = new MWDataReceiveMessage { Label = label, Content = data, From = from, FromID = fromId };
            if (players.ContainsKey(player) && players[player] != null)
            {
                logWriter.LogDebug($"Sending '{label}': '{data}' from '{from}' to '{players[player].Name}'", randoId);
                Server.QueuePushMessage(players[player].uid, msg);
                players[player].QueueConfirmableMessage(msg, ttl);
            }

            // Always add to unconfirmed, which doubles as holding datas for offline players
            unconfirmedMessages.GetOrCreateDefault(player).Add(msg);
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

        internal void SendDataTo(string label, string data, int toId, int fromId, int ttl = 30)
        {
            SendDataTo(label, data, toId, nicknames[fromId], fromId, ttl);
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

        internal void SendDataToAll(string label, string data, int playerId, int ttl)
        {
            foreach (var kvp in players)
            {
#if !DEBUG
                if (kvp.Key == playerId) continue;
#endif

                SendDataTo(label, data, kvp.Key, playerId, ttl);
            }
        }

        internal void SendDatasTo(int toId, List<(string, string)> datas, int fromId)
        {
            MWDatasReceiveMessage msg = new MWDatasReceiveMessage { Datas = datas, From = nicknames[fromId] };
            if (players.TryGetValue(toId, out var playerSession) && playerSession != null)
                playerSession.QueueConfirmableMessage(msg);
            
            unconfirmedMessages.GetOrCreateDefault(toId).Add(msg);
        }
    }
}
