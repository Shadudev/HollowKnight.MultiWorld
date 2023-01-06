namespace MultiWorldLib.ExportedAPI
{
    /// <summary>
    /// For internal use only.
    /// </summary>
    public abstract class ExportedClientConnectionAPI
    {
        protected Dictionary<int, string> m_connectedPlayersMap = new();
        /// <summary>
        /// An event for when the a player connects or disconnects.
        /// The passed dictionary is the new list of online players.
        /// </summary>
        public Action<Dictionary<int, string>> OnConnectedPlayersChanged;

        /// <summary>
        /// Invoked when data received. Register to this callback once.
        /// </summary>
        public Action<DataReceivedEvent> OnDataReceived;

        protected abstract void SendAndQueueData(string label, string data, int to, int ttl = Consts.DEFAULT_TTL, bool isOnJoin = false);
        protected abstract int GetPlayerID(string playerName);


        /// <summary>
        /// Returns a collection with the connected players. Keys are player IDs, values are player names.
        /// </summary>
        public Dictionary<int, string> GetConnectedPlayers()
        {
            return new(m_connectedPlayersMap);
        }

        /// <summary>
        /// Sends message to a player by their ID
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        /// <param name="to">Receiver player ID</param>
        /// <param name="ttl">Max times the message will be sent to the receiver till they confirm it</param>
        public void SendData(string label, string data, int to, int ttl)
        {
            SendAndQueueData(label, data, to, ttl);
        }

        /// <summary>
        /// Sends message to a player by their ID
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        /// <param name="to">Receiver player ID</param>
        public void SendData(string label, string data, int to)
        {
            SendData(label, data, to, Consts.DEFAULT_TTL);
        }

        /// <summary>
        /// Sends message to a player by their name
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        /// <param name="receieverName">Receiver player name</param>
        /// <param name="ttl">Max times the message will be sent to the receiver till they confirm it</param>
        /// <returns>Whether the player name exists in the names collection</returns>
        public bool SendData(string label, string data, string receieverName, int ttl)
        {
            int playerId = GetPlayerID(receieverName);
            if (playerId == -1) return false;

            SendData(label, data, playerId, ttl);
            return true;
        }

        /// <summary>
        /// Sends message to a player by their name
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        /// <param name="receieverName">Receiver player name</param>
        /// <returns>Whether the player name exists in the names collection</returns>
        public bool SendData(string label, string data, string receieverName)
        {
            return SendData(label, data, receieverName, Consts.DEFAULT_TTL);
        }

        /// <summary>
        /// Sends message to a collection of players by their IDs
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        /// <param name="to">Receiver players IDs</param>
        /// <param name="ttl">Max times the message will be sent to the receiver till they confirm it</param>
        public void SendData(string label, string data, IEnumerable<int> to, int ttl = Consts.DEFAULT_TTL)
        {
            foreach (int _to in to)
                SendData(label, data, _to, ttl);
        }

        /// <summary>
        /// Sends message to a collection of players by their names
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        /// <param name="receiversNames">Receiver players names</param>
        /// <param name="ttl">Max times the message will be sent to the receiver till they confirm it</param>
        public void SendData(string label, string data, IEnumerable<string> receiversNames, int ttl = Consts.DEFAULT_TTL)
        {
            foreach (string name in receiversNames)
                SendData(label, data, name, ttl);
        }

        /// <summary>
        /// Sends message to all the players
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        /// <param name="ttl">Max times the message will be sent to the receiver till they confirm it</param>
        public void SendDataToAll(string label, string data, int ttl)
        {
            SendData(label, data, Consts.TO_ALL_MAGIC, ttl);
        }

        /// <summary>
        /// Sends message to all the players
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        public void SendDataToAll(string label, string data)
        {
            SendDataToAll(label, data, Consts.DEFAULT_TTL);
        }

        /// <summary>
        /// Sends message to a player by their ID, only if they are currently connected
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        /// <param name="to">Receiver players ID</param>
        public void SendDataIfConnected(string label, string data, int to, int ttl = Consts.DEFAULT_TTL)
        {
            if (!GetConnectedPlayers().ContainsKey(to)) return;

            SendData(label, data, to, ttl);
        }

        /// <summary>
        /// Sends message to a player by their name, only if they are currently connected
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        /// <param name="to">Receiver players name</param>
        public void SendDataIfConnected(string label, string data, string to, int ttl = Consts.DEFAULT_TTL)
        {
            if (!GetConnectedPlayers().ContainsValue(to)) return;

            SendData(label, data, to, ttl);
        }

        /// <summary>
        /// Sends message to all the currently connected players
        /// </summary>
        /// <param name="label">Message Label to filter by</param>
        /// <param name="data">Message content</param>
        /// <returns>Whether the player name exists in the names collection</returns>
        public void SendDataToAllConnected(string label, string data, int ttl = Consts.DEFAULT_TTL)
        {
            SendData(label, data, GetConnectedPlayers().Keys, ttl);
        }
    }
}
