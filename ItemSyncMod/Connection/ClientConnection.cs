using MultiWorldLib.Binary;
using MultiWorldLib.Messaging;
using MultiWorldLib.Messaging.Definitions.Messages;
using ItemSyncMod.Connection;
using System.Net.Sockets;
using Modding;
using static ItemSyncMod.LogHelper;

using System.Net;
using MultiWorldLib;
using MultiWorldLib.Messaging.Definitions;
using MultiWorldLib.ExportedAPI;
using ItemSyncMod.Menu;

namespace ItemSyncMod
{
    public class ClientConnection : ExportedClientConnectionAPI
    {
        private const int PING_INTERVAL = 10000;

        private readonly MWMessagePacker Packer = new(new BinaryMWMessageEncoder());
        private TcpClient _client;
        private string currentUrl;
        private Timer PingTimer;
        private readonly ConnectionState State;
        private readonly List<MWConfirmableMessage> ConfirmableMessagesQueue = new();
        private Thread ReadThread;

        internal Action<int, string[]> OnReadyConfirm;
        internal Action<string> OnReadyDeny;
        internal Action<ulong, string> OnConnect;
        internal Action GameStarted;

        private readonly List<MWMessage> messageEventQueue = new();

        internal ClientConnection()
        {
            State = new ConnectionState();

            ModHooks.ApplicationQuitHook += Disconnect;
        }

        internal void Connect(string url)
        {
            if (_client != null && _client.Connected)
            {
                Disconnect();
            }

            currentUrl = url;
            Reconnect();
        }

        private void Reconnect()
        {
            if (_client != null && _client.Connected)
            {
                return;
            }

            Log($"Trying to connect to `{currentUrl}`");

            State.Uid = 0;
            State.LastPing = DateTime.Now;

            _client = new TcpClient
            {
                ReceiveTimeout = 2000,
                SendTimeout = 2000
            };

            if (!TryConnect())
            {
                throw new Exception($"Could not connect to {currentUrl}");
            }

            if (ReadThread != null && ReadThread.IsAlive)
            {
                ReadThread.Abort();
            }

            // Make sure we never have more than one ping timer
            PingTimer?.Dispose();
            PingTimer = new Timer(DoPing, State, 1000, PING_INTERVAL);

            ReadThread = new Thread(ReadWorker);
            ReadThread.Start();

            SendMessage(new MWConnectMessage());
        }

        internal void FlushReceivedMessagesQueue()
        {
            messageEventQueue.Clear();
        }

        private bool TryConnect()
        {
            List<Tuple<string, int>> resoledUrls = ResolveURL();
            foreach (Tuple<string, int> resolvedUrl in resoledUrls)
            {
                try
                {
                    string ip = resolvedUrl.Item1;
                    int port = resolvedUrl.Item2;
                    Log($"Attemping to connect to {ip}:{port}");
                    _client.Connect(ip, port);
                }
                catch { } // Ignored exception as we may connect to another IP successfully
            }
            return _client.Connected;
        }

        private List<Tuple<string, int>> ResolveURL()
        {
            List<Tuple<string, int>> urls = new();
            int port = GetPortFromURL(currentUrl);

            string ipString = currentUrl;
            int index = currentUrl.IndexOf(':');
            if (index != -1)
                ipString = currentUrl.Substring(0, index);
            if (IPAddress.TryParse(ipString, out IPAddress ip))
            {
                urls.Add(new Tuple<string, int>(ip.ToString(), port));
            }
            else
            {
                // ipString is probably domain
                IPHostEntry hostEntry = Dns.GetHostEntry(ipString);
                Array.ForEach(hostEntry.AddressList, ipAddress => urls.Add(
                    new Tuple<string, int>(ipAddress.ToString(), port)));
            }

            return urls;
        }

        private int GetPortFromURL(string url)
        {
            int index = url.IndexOf(":");
            if (index != -1 && int.TryParse(url.Substring(index + 1), out int port))
            {
                return port;
            }
            return Consts.DEFAULT_PORT;
        }

        internal void JoinRando(int randoId, int playerId)
        {
            Log($"Joining rando session {randoId} as \"{ItemSyncMod.ISSettings.UserName}\" - ({playerId})");

            State.SessionId = randoId;
            State.PlayerId = playerId;

            SendMessage(new MWJoinMessage
            {
                DisplayName = ItemSyncMod.ISSettings.UserName,
                RandoId = randoId,
                PlayerId = playerId,
                Mode = Mode.ItemSync
            });

            ModHooks.HeroUpdateHook += SynchronizeEvents;
        }

        internal void Rejoin()
        {
            if (State.SessionId == -1 || State.PlayerId == -1) return;
            JoinRando(State.SessionId, State.PlayerId);
        }

        internal void Disconnect()
        {
            if (!State.Connected) return;

            Log("Disconnecting from server");
            PingTimer?.Dispose();
            PingTimer = null;

            try
            {
                ReadThread?.Abort();
                ReadThread = null;
                Log($"Disconnecting (UID = {State.Uid})");
                byte[] buf = Packer.Pack(new MWDisconnectMessage { SenderUid = State.Uid }).Buffer;
                _client?.GetStream().Write(buf, 0, buf.Length);
                _client?.Close();
            }
            catch (InvalidOperationException) { } // Socket is not connected, already disconnected
            catch (Exception e)
            {
                if (State.Connected)
                    LogError("Error during disconnect:\n" + e);
            }
            finally
            {
                State.Connected = false;
                State.Joined = false;
                _client = null;
                messageEventQueue.Clear();

                ModHooks.HeroUpdateHook -= SynchronizeEvents;
            }
        }

        private void InvokeDataReceived(DataReceivedEvent dataReceivedEvent)
        {
            try
            {
                OnDataReceived?.Invoke(dataReceivedEvent);
            }
            catch (Exception e)
            {
                LogError($"OnDataReceived failed for `{dataReceivedEvent.Label}: {dataReceivedEvent.Content}`: {e.Message}\n{e.StackTrace}");
            }
        }

        private void SynchronizeEvents()
        {
            MWMessage message = null;

            lock (messageEventQueue)
            {
                if (messageEventQueue.Count > 0)
                {
                    message = messageEventQueue[0];
                    messageEventQueue.RemoveAt(0);
                }
            }

            if (message == null)
            {
                return;
            }

            switch (message)
            {
                case MWDataReceiveMessage msg:
                    DataReceivedEvent dataReceivedEvent = new(msg.Label, msg.Content, msg.From);
                    InvokeDataReceived(dataReceivedEvent);

                    if (dataReceivedEvent.Handled)
                        SendMessage(new MWDataReceiveConfirmMessage { Label = msg.Label, Data = msg.Content, From = msg.From });
                    break;
                default:
                    Log("Unknown type in message queue: " + message.MessageType);
                    break;
            }
        }

        private void DoPing(object state)
        {
            if (_client == null || !_client.Connected)
            {
                if (State.Connected)
                {
                    State.Connected = false;
                    State.Joined = false;

                    Log("Disconnected from server");
                }

                Disconnect();
                Reconnect();
                Rejoin();
            }

            if (State.Connected)
            {
                if (DateTime.Now - State.LastPing > TimeSpan.FromMilliseconds(PING_INTERVAL * 3.5))
                {
                    LogWarn("Connection timed out");

                    Disconnect();
                    Reconnect();
                    Rejoin();
                }
                else
                {
                    SendMessage(new MWPingMessage());

                    if (State.Joined)
                    {
                        ResendMessagesQueue();
                    }
                }
            }
        }

        private void SendMessage(MWMessage msg)
        {
            try
            {
                //Always set Uid in here, if uninitialized will be 0 as required.
                //Otherwise less work resuming session etc.
                msg.SenderUid = State.Uid;
                byte[] bytes = Packer.Pack(msg).Buffer;

                NetworkStream stream = _client.GetStream();
                lock (stream)
                {
                    stream.BeginWrite(bytes, 0, bytes.Length, WriteToServer, stream);
                }
            }
            catch (Exception e)
            {
                LogWarn($"Failed to send message '{msg}' to server:\n{e}");
            }
        }

        private void ReadWorker()
        {
            NetworkStream stream = _client.GetStream();
            try
            {
                while (true)
                {
                    var message = new MWPackedMessage(stream);
                    ReadFromServer(message);
                }
            } 
            catch (ThreadAbortException) { } 
            catch (Exception e)
            {
                LogError($"Something failed in connection listening thread: {e}");
            }
        }

        private void WriteToServer(IAsyncResult res)
        {
            NetworkStream stream = (NetworkStream)res.AsyncState;
            stream.EndWrite(res);
        }

        private void ReadFromServer(MWPackedMessage packed)
        {
            MWMessage message = Packer.Unpack(packed);
            switch (message.MessageType)
            {
                case MWMessageType.ConnectMessage:
                    HandleConnect((MWConnectMessage)message);
                    break;
                case MWMessageType.DisconnectMessage:
                    HandleDisconnectMessage((MWDisconnectMessage)message);
                    break;
                case MWMessageType.JoinConfirmMessage:
                    HandleJoinConfirm((MWJoinConfirmMessage)message);
                    break;
                case MWMessageType.DataReceiveMessage:
                    HandleDataReceive((MWDataReceiveMessage)message);
                    break;
                case MWMessageType.DataSendConfirmMessage:
                    HandleDataSendConfirm((MWDataSendConfirmMessage)message);
                    break;
                case MWMessageType.PingMessage:
                    State.LastPing = DateTime.Now;
                    break;
                case MWMessageType.ReadyConfirmMessage:
                    HandleReadyConfirm((MWReadyConfirmMessage)message);
                    break;
                case MWMessageType.ReadyDenyMessage:
                    HandleReadyDeny((MWReadyDenyMessage)message);
                    break;
                case MWMessageType.RequestSettingsMessage:
                    HandleRequestSettings((MWRequestSettingsMessage)message);
                    break;
                case MWMessageType.ApplySettingsMessage:
                    HandleApplySettings((MWApplySettingsMessage)message);
                    break;
                case MWMessageType.InitiateSyncGameMessage:
                    HandleInitiateGame((MWInitiateSyncGameMessage)message);
                    break;
                case MWMessageType.ResultMessage:
                    HandleResult((MWResultMessage)message);
                    break;
                case MWMessageType.ConnectedPlayersChangedMessage:
                    HandleConnectedPlayersChanged((MWConnectedPlayersChangedMessage)message);
                    break;
                case MWMessageType.InvalidMessage:
                default:
                    throw new InvalidOperationException("Received Invalid Message Type");
            }
        }

        private void ResendMessagesQueue()
        {
            lock (ConfirmableMessagesQueue)
                ConfirmableMessagesQueue.ForEach(SendMessage);
        }

        private void ClearFromSendQueue(IConfirmMessage message)
        {
            lock (ConfirmableMessagesQueue)
            {
                for (int i = 0; i < ConfirmableMessagesQueue.Count; i++)
                {
                    MWConfirmableMessage queueMessage = ConfirmableMessagesQueue[i];
                    if (message.Confirms(queueMessage))
                    {
                        ConfirmableMessagesQueue.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private void HandleConnect(MWConnectMessage message)
        {
            State.Uid = message.SenderUid;
            State.Connected = true;
            Log($"Connected! (UID = {State.Uid})");
            OnConnect?.Invoke(State.Uid, message.ServerName);
        }

        private void HandleJoinConfirm(MWJoinConfirmMessage message)
        {
            State.Joined = true;

            foreach ((string label, string data, int to) in ItemSyncMod.ISSettings.GetUnconfirmedDatas())
                SendAndQueueData(label, data, to, isOnJoin: true);
        }

        private void HandleDisconnectMessage(MWDisconnectMessage message)
        {
            State.Connected = false;
            State.Joined = false;
        }

        private void HandleReadyConfirm(MWReadyConfirmMessage message)
        {
            OnReadyConfirm?.Invoke(message.Ready, message.Names);
            SendMessage(new MWRequestSettingsMessage { });
        }

        private void HandleReadyDeny(MWReadyDenyMessage message)
        {
            OnReadyDeny?.Invoke(message.Description);
        }

        private void HandleDataReceive(MWDataReceiveMessage message)
        {
            LogDebug($"Data received, label: " + message.Label);
            lock (messageEventQueue)
            {
                messageEventQueue.Add(message);
            }
        }

        private void HandleDataSendConfirm(MWDataSendConfirmMessage message)
        {
            // Mark the data confirmed here, so if we send an data but disconnect we can be sure it will be resent when we open again
            ItemSyncMod.ISSettings.MarkDataConfirmed((message.Label, message.Content, message.To));
            ClearFromSendQueue(message);
        }

        internal void ReadyUp(string room, int hash)
        {
            SendMessage(new ISReadyMessage { Room = room, Nickname = ItemSyncMod.GS.UserName, Hash = hash });
        }

        internal void Unready()
        {
            SendMessage(new MWUnreadyMessage());
        }

        internal void InitiateGame(string settings)
        {
            SendMessage(new MWInitiateSyncGameMessage { Settings = settings });
        }

        internal void HandleRequestSettings(MWRequestSettingsMessage message)
        {
            SendSettings(MenuHolder.MenuInstance.SettingsSharer.GetSerializedSettings());
        }

        internal void SendSettings(string settingsJson)
        {
            SendMessage(new MWApplySettingsMessage() { Settings = settingsJson });
        }

        internal void HandleApplySettings(MWApplySettingsMessage message)
        {
            MenuHolder.MenuInstance.SettingsSharer.BroadcastReceivedSettings(message.Settings);
        }

        private void HandleInitiateGame(MWInitiateSyncGameMessage message)
        {
            MenuHolder.MenuInstance.SettingsSharer.BroadcastReceivedSettings(message.Settings);
            MenuHolder.MenuInstance.LockSettingsButtons();
        }

        private void HandleResult(MWResultMessage message)
        {
            ItemSyncMod.ISSettings.URL = ItemSyncMod.GS.URL;
            ItemSyncMod.ISSettings.MWPlayerId = message.PlayerId;
            ItemSyncMod.ISSettings.MWRandoId = message.RandoId;
            ItemSyncMod.ISSettings.UserName = ItemSyncMod.GS.UserName;
            ItemSyncMod.ISSettings.IsItemSync = true;
            ItemSyncMod.ISSettings.SetNicknames(message.Nicknames);

            ItemSyncMod.ISSettings.SyncVanillaItems = ItemSyncMod.GS.SyncVanillaItems;
            ItemSyncMod.ISSettings.SyncSimpleKeysUsages = ItemSyncMod.GS.SyncSimpleKeysUsages;
            ItemSyncMod.ISSettings.AdditionalFeaturesEnabled = ItemSyncMod.GS.AdditionalFeaturesEnabled;

            GameStarted?.Invoke();
        }

        internal void NotifySave()
        {
            SendMessage(new MWSaveMessage { });
        }

        protected override void SendAndQueueData(string label, string data, int to, int ttl = Consts.DEFAULT_TTL, bool isOnJoin = false)
        {
            if (!isOnJoin)
                ItemSyncMod.ISSettings.AddSentData((label, data, to));

            MWDataSendMessage msg = new() { Label = label, Content = data, To = to, TTL = ttl };
            lock (ConfirmableMessagesQueue)
                ConfirmableMessagesQueue.Add(msg);
            SendMessage(msg);
        }

        protected override int GetPlayerID(string playerName)
        {
            return Array.IndexOf(ItemSyncMod.ISSettings.GetNicknames(), playerName);
        }

        private void HandleConnectedPlayersChanged(MWConnectedPlayersChangedMessage message)
        {
            m_connectedPlayersMap = message.Players;
            OnConnectedPlayersChanged?.Invoke(message.Players);
        }

        public bool IsConnected()
        {
            return State.Connected;
        }

        ~ClientConnection()
        {
            try
            {
                Disconnect();
            }
            catch (Exception) {}
        }
    }
}
