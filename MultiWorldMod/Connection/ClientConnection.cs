using MultiWorldLib.Binary;
using MultiWorldLib.Messaging;
using MultiWorldLib.Messaging.Definitions.Messages;
using MultiWorldMod.Connection;
using System.Net.Sockets;
using static MultiWorldMod.LogHelper;

using System.Net;
using MultiWorldLib;
using Modding;
using MultiWorldLib.Messaging.Definitions;
using MultiWorldMod.Items;

namespace MultiWorldMod
{
    public class ClientConnection
    {
        private const int PING_INTERVAL = 10000;

        private readonly MWMessagePacker Packer = new(new BinaryMWMessageEncoder());
        private TcpClient _client;
        private string currentUrl;
        private Timer PingTimer;
        private readonly ConnectionState State;
        private readonly List<MWConfirmableMessage> ConfirmableMessagesQueue = new();
        private Thread ReadThread;

        public delegate void DisconnectEvent();
        public delegate void ConnectEvent(ulong uid);
        public delegate void JoinEvent();
        public delegate void LeaveEvent();

        public Action<int, string> OnReadyConfirm;
        public Action<string> OnReadyDeny;
        public event DisconnectEvent OnDisconnect;
        public event ConnectEvent OnConnect;
        public event JoinEvent OnJoin;
        public event LeaveEvent OnLeave;
        public Action GameStarted;

        private readonly List<MWMessage> messageEventQueue = new();

        private int readyID;

        public ClientConnection()
        {
            State = new ConnectionState();

            ModHooks.ApplicationQuitHook += Disconnect;
        }

        public void Connect(string url)
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
                throw new Exception($"Could not connect to `{currentUrl}`");
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
            if (index != -1 && int.TryParse(url.Substring(index + 1), out int port)) {
                return port;
            }
            return MultiWorldMod.GS.DefaultPort;
        }

        public void JoinRando(int randoId, int playerId)
        {
            Log($"Joining rando session {randoId} as \"{MultiWorldMod.GS.UserName}\" - ({playerId})");

            State.SessionId = randoId;
            State.PlayerId = playerId;

            SendMessage(new MWJoinMessage
            {
                DisplayName = MultiWorldMod.GS.UserName,
                RandoId = randoId,
                PlayerId = playerId,
                Mode = Mode.MultiWorld
            });

            ModHooks.HeroUpdateHook += SynchronizeEvents;
        }

        public void Rejoin()
        {
            if (State.SessionId == -1 || State.PlayerId == -1) return;
            JoinRando(State.SessionId, State.PlayerId);
        }

        public void Leave()
        {
            State.SessionId = -1;
            State.PlayerId = -1;

            State.Joined = false;
            if (State.Connected) SendMessage(new MWLeaveMessage());
            OnLeave?.Invoke();
        }

        public void Disconnect()
        {
            if (!State.Connected) return;

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
            catch (Exception e)
            {
                if (State.Connected)
                    LogError("Error disconnecting:\n" + e);
            }
            finally
            {
                State.Connected = false;
                State.Joined = false;
                _client = null;
                messageEventQueue.Clear();

                OnDisconnect?.Invoke();
                ModHooks.HeroUpdateHook -= SynchronizeEvents;
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
                case MWItemReceiveMessage itemMsg:
                    if (ItemManager.TryGiveItem(itemMsg.Item, itemMsg.From))
                        SendMessage(new MWItemReceiveConfirmMessage { Item = itemMsg.Item, From = itemMsg.From });
                    break;
                case MWItemsReceiveMessage itemsMsg:
                    try
                    {
                        if (itemsMsg.Items.All(itemId => ItemManager.TryGiveItem(itemId, itemsMsg.From)))
                            SendMessage(new MWItemsReceiveConfirmMessage { Count = itemsMsg.Items.Count, From = itemsMsg.From });
                    }
                    catch (Exception) { } // Failed to give all sent items, don't respond to server and try to reprocess it soon
                    break;
                default:
                    LogError("Unknown type in message queue: " + message.MessageType);
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
                    //If there are items in the queue that the server hasn't confirmed yet
                    lock (ConfirmableMessagesQueue)
                    {
                        if (ConfirmableMessagesQueue.Count > 0 && State.Joined)
                        {
                            ResendItemQueue();
                        }
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
                    stream.BeginWrite(bytes, 0, bytes.Length, WriteToServer, stream);
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
            MWMessage message;
            try
            {
                message = Packer.Unpack(packed);
            }
            catch (Exception e)
            {
                Log(e);
                return;
            }

            switch (message.MessageType)
            {
                case MWMessageType.ConnectMessage:
                    HandleConnect((MWConnectMessage)message);
                    break;
                case MWMessageType.ReconnectMessage:
                    break;
                case MWMessageType.DisconnectMessage:
                    HandleDisconnectMessage((MWDisconnectMessage)message);
                    break;
                case MWMessageType.JoinConfirmMessage:
                    HandleJoinConfirm((MWJoinConfirmMessage)message);
                    break;
                case MWMessageType.LeaveMessage:
                    HandleLeaveMessage((MWLeaveMessage)message);
                    break;
                case MWMessageType.ItemReceiveMessage:
                    HandleItemReceive((MWItemReceiveMessage)message);
                    break;
                case MWMessageType.ItemSendConfirmMessage:
                    HandleItemSendConfirm((MWItemSendConfirmMessage)message);
                    break;
                case MWMessageType.ItemsSendConfirmMessage:
                    HandleItemsSendConfirm((MWItemsSendConfirmMessage)message);
                    break;
                case MWMessageType.ItemsReceiveMessage:
                    HandleItemsReceive((MWItemsReceiveMessage)message);
                    break;
                case MWMessageType.NotifyMessage:
                    HandleNotify((MWNotifyMessage)message);
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
                case MWMessageType.RequestRandoMessage:
                    HandleRequestRando((MWRequestRandoMessage)message);
                    break;
                case MWMessageType.ResultMessage:
                    HandleResult((MWResultMessage)message);
                    break;
                case MWMessageType.RequestCharmNotchCostsMessage:
                    HandleRequestCharmNotchCosts((MWRequestCharmNotchCostsMessage)message);
                    break;
                case MWMessageType.AnnounceCharmNotchCostsMessage:
                    HandleAnnounceCharmNotchCosts((MWAnnounceCharmNotchCostsMessage)message);
                    break;
                case MWMessageType.InvalidMessage:
                default:
                    throw new InvalidOperationException("Received Invalid Message Type");
            }
        }

        private void ResendItemQueue()
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
            OnConnect?.Invoke(State.Uid);
        }

        private void HandleJoinConfirm(MWJoinConfirmMessage message)
        {
            State.Joined = true;
            OnJoin?.Invoke();

            foreach (MWItem item in MultiWorldMod.MWS.UnconfirmedItems)
                SendItem(item);
        }

        private void HandleLeaveMessage(MWLeaveMessage message)
        {
            State.Joined = false;
            OnLeave?.Invoke();
        }

        private void HandleDisconnectMessage(MWDisconnectMessage message)
        {
            State.Connected = false;
            State.Joined = false;
        }

        private void HandleNotify(MWNotifyMessage message)
        {
            lock (messageEventQueue)
            {
                messageEventQueue.Add(message);
            }
        }

        private void HandleReadyConfirm(MWReadyConfirmMessage message)
        {
            OnReadyConfirm?.Invoke(message.Ready, message.Names);
            readyID = message.ReadyID;
        }

        private void HandleReadyDeny(MWReadyDenyMessage message)
        {
            OnReadyDeny?.Invoke(message.Description);
        }

        private void HandleItemReceive(MWItemReceiveMessage message)
        {
            LogDebug("Queueing received item: " + message.Item);
            lock (messageEventQueue)
            {
                messageEventQueue.Add(message);
            }
        }

        private void HandleItemsReceive(MWItemsReceiveMessage message)
        {
            LogDebug($"Queueing {message.Items.Count} received items from {message.From}");
            lock  (messageEventQueue)
            {
                messageEventQueue.Add(message);
            }
        }

        private void HandleItemSendConfirm(MWItemSendConfirmMessage message)
        {
            // Mark the item confirmed here, so if we send an item but disconnect we can be sure it will be resent when we connect again
            LogDebug($"Confirming item: {message.Item} to {message.To}");
            MultiWorldMod.MWS.MarkItemConfirmed(new MWItem(message.To, message.Item));
            ClearFromSendQueue(message);
        }

        private void HandleItemsSendConfirm(MWItemsSendConfirmMessage message)
        {
            EjectMenuHandler.UpdateButton(message.ItemsCount);
        }

        public void ReadyUp(string room)
        {
            SendMessage(new MWReadyMessage { Room = room, Nickname = MultiWorldMod.GS.UserName, ReadyMode = Mode.MultiWorld });
        }

        public void Unready()
        {
            SendMessage(new MWUnreadyMessage());
        }

        public void InitiateGame(int seed)
        {
            SendMessage(new MWInitiateGameMessage { Seed = seed, ReadyID = readyID });
        }

        private void HandleRequestRando(MWRequestRandoMessage message)
        {
            (string, string)[] placements = MultiWorldMod.Controller.GetShuffledItemsPlacementsInOrder();
            ExchangePlacementsWithServer(placements);
            Log("Exchanged items with server successfully!");
        }

        private void ExchangePlacementsWithServer((string, string)[] placements)
        {
            SendMessage(new MWRandoGeneratedMessage { Items = placements });
        }

        private void HandleResult(MWResultMessage message)
        {
            LogDebug($"Result received");
            MultiWorldMod.MWS.PlayerId = message.ResultData.playerId;
            LogDebug($"PlayerId set");
            MultiWorldMod.MWS.MWRandoId = message.ResultData.randoId;
            LogDebug($"MWRandoId set");
            MultiWorldMod.MWS.SetPlayersNames(message.ResultData.nicknames);
            LogDebug($"SetPlayersNames called");
            MultiWorldMod.MWS.IsMW = true;
            LogDebug($"IsMW set");
            MultiWorldMod.MWS.URL = currentUrl;
            LogDebug($"URL set");

            ItemManager.StorePlacements(message.Placements);
            LogDebug($"StorePlacements called");

            GameStarted?.Invoke();
        }

        public void SendItem(MWItem item)
        {
            MWItemSendMessage msg = new() { Item = item.Item, To = item.PlayerId };
            MultiWorldMod.MWS.AddSentItem(item);
            lock (ConfirmableMessagesQueue)
                ConfirmableMessagesQueue.Add(msg);
            SendMessage(msg);
        }

        public void SendItem(string item, int playerId)
        {
            SendItem(new MWItem(playerId, item));
        }

        public void SendItems(List<(int, string)> items)
        {
            SendMessage(new MWItemsSendMessage { Items = items });
        }

        public void NotifySave()
        {
            SendMessage(new MWSaveMessage {});
        }

        private void HandleRequestCharmNotchCosts(MWRequestCharmNotchCostsMessage message)
        {
            LogDebug("Sending costs");
            SendMessage(new MWAnnounceCharmNotchCostsMessage {
                PlayerID = MultiWorldMod.MWS.PlayerId,
                Costs = CharmNotchCosts.Get()
            });
        }

        private void HandleAnnounceCharmNotchCosts(MWAnnounceCharmNotchCostsMessage message)
        {
            LogDebug($"Received {message.PlayerID}'s charm notch costs");
            ItemManager.UpdateOthersCharmNotchCosts(message.PlayerID, message.Costs);
            SendMessage(new MWConfirmCharmNotchCostsReceivedMessage { PlayerID = message.PlayerID });
        }

        public bool IsConnected()
        {
            return State.Connected;
        }

        public ConnectionStatus GetStatus()
        {
            if (!State.Connected)
            {
                return ConnectionStatus.NotConnected;
            }

            if (!State.Joined)
            {
                return ConnectionStatus.Connected;
            }

            return ConnectionStatus.Joined;
        }

        public enum ConnectionStatus
        {
            NotConnected,
            Connected,
            Joined
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
