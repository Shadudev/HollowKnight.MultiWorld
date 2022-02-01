using MultiWorldLib.Binary;
using MultiWorldLib.Messaging;
using MultiWorldLib.Messaging.Definitions.Messages;
using ItemSyncMod.Connection;
using System.Net.Sockets;
using Modding;
using static ItemSyncMod.LogHelper;

using System.Net;
using MultiWorldLib;
using ItemSyncMod.Items;
using ItemChanger;
using MultiWorldLib.Messaging.Definitions;
using ItemSyncMod.Transitions;

namespace ItemSyncMod
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

        public ClientConnection()
        {
            State = new ConnectionState();
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
            return ItemSyncMod.GS.DefaultPort;
        }

        public void JoinRando(int randoId, int playerId)
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
            SendMessage(new MWLeaveMessage());
            OnLeave?.Invoke();
        }
        public void Disconnect()
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
            catch (Exception e)
            {
                Log("Error disconnection:\n" + e);
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
                case MWItemReceiveMessage item:
                    ItemManager.GiveItem(item.Item);
                    break;
                case MWVisitStateChangedMessage placementVisitChanged:
                    ItemManager.PlacementVisitChanged(placementVisitChanged);
                    break;
                case MWTransitionFoundMessage transitionFound:
                    TransitionsManager.MarkTransitionFound(transitionFound.Source, transitionFound.Target);
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
                    Log("Connection timed out");

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
                Log($"Failed to send message '{msg}' to server:\n{e}");
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
                LogError(e.ToString());
                return;
            }

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
                case MWMessageType.LeaveMessage:
                    HandleLeaveMessage((MWLeaveMessage)message);
                    break;
                case MWMessageType.ItemReceiveMessage:
                    HandleItemReceive((MWItemReceiveMessage)message);
                    break;
                case MWMessageType.ItemSendConfirmMessage:
                    HandleItemSendConfirm((MWItemSendConfirmMessage)message);
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
                case MWMessageType.VisitStateChangedMessage:
                    HandleVisitStateChanged((MWVisitStateChangedMessage)message);
                    break;
                case MWMessageType.VisitStateChangedConfirmMessage:
                    HandleVisitStateChangedConfirm((MWVisitStateChangedConfirmMessage)message);
                    break;
                case MWMessageType.TransitionFoundMessage:
                    HandleTransitionFound((MWTransitionFoundMessage)message);
                    break;
                case MWMessageType.TransitionFoundConfirmMessage:
                    HandleTransitionFoundConfirm((MWTransitionFoundConfirmMessage)message);
                    break;
                case MWMessageType.InvalidMessage:
                default:
                    throw new InvalidOperationException("Received Invalid Message Type");
            }
        }

        private void ResendMessagesQueue()
        {
            lock (ConfirmableMessagesQueue)
            {
                foreach (MWMessage message in ConfirmableMessagesQueue)
                {
                    SendMessage(message);
                }
            }
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

            ItemSyncMod.ISSettings.GetUnconfirmedItems().ForEach(SendItemToAll);
            ItemSyncMod.ISSettings.GetUnconfirmedStateChanges().ForEach(v =>
                SendVisitStateChanged(v.Item1, v.Item2, v.Item3, v.Item4));
            ItemSyncMod.ISSettings.GetUnconfirmedTransitionsFound().ForEach(transition =>
                SendTransitionFound(transition.Item1, transition.Item2));
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
            SendMessage(new MWRequestSettingsMessage { });
        }

        private void HandleItemReceive(MWItemReceiveMessage message)
        {
            LogDebug("Queueing received item: " + message.Item);
            lock (messageEventQueue)
            {
                messageEventQueue.Add(message);
            }

            //Do whatever we want to do when we get an item here, then confirm
            SendMessage(new MWItemReceiveConfirmMessage { Item = message.Item, From = message.From });
        }

        private void HandleVisitStateChanged(MWVisitStateChangedMessage message)
        {
            lock (messageEventQueue)
            {
                messageEventQueue.Add(message);
            }
            LogDebug($"Received visit state changed, name: {message.Name}, previews: {string.Join(", ", message.PreviewTexts)}, isMulti: {message.PreviewRecordTagType}, newFlags: {message.NewVisitFlags}");

            SendMessage(new MWVisitStateChangedConfirmMessage { Name = message.Name,
                PreviewRecordTagType = message.PreviewRecordTagType,
                NewVisitFlags = message.NewVisitFlags
            });
        }

        private void HandleTransitionFound(MWTransitionFoundMessage message)
        {
            lock (messageEventQueue)
            {
                messageEventQueue.Add(message);
            }
            LogDebug($"Received transition found: {message.Source}->{message.Target}");
            SendMessage(new MWTransitionFoundConfirmMessage { 
                Source = message.Source, Target = message.Target });
        }

        private void HandleItemSendConfirm(MWItemSendConfirmMessage message)
        {
            // Mark the item confirmed here, so if we send an item but disconnect we can be sure it will be resent when we open again
            ItemSyncMod.ISSettings.MarkItemConfirmed(message.Item);
            ClearFromSendQueue(message);
        }

        private void HandleVisitStateChangedConfirm(MWVisitStateChangedConfirmMessage message)
        {
            ItemSyncMod.ISSettings.MarkVisitChangeConfirmed(message.Name, message.PreviewRecordTagType, message.NewVisitFlags);
            ClearFromSendQueue(message);
        }

        private void HandleTransitionFoundConfirm(MWTransitionFoundConfirmMessage message)
        {
            ItemSyncMod.ISSettings.MarkTransitionFoundConfirmed(message.Source, message.Target);
            ClearFromSendQueue(message);
        }

        public void ReadyUp(string room)
        {
            SendMessage(new MWReadyMessage { Room = room, Nickname = ItemSyncMod.GS.UserName, ReadyMode=Mode.ItemSync });
        }

        public void Unready()
        {
            SendMessage(new MWUnreadyMessage());
        }

        public void InitiateGame(string settings)
        {
            SendMessage(new MWInitiateSyncGameMessage { Settings = settings });
        }

        public void HandleRequestSettings(MWRequestSettingsMessage message)
        {
            SendSettings(ItemSyncMod.SettingsSyncer.GetSerializedSettings());
        }

        public void SendSettings(string settingsJson)
        {
            SendMessage(new MWApplySettingsMessage() { Settings = settingsJson });
        }

        public void HandleApplySettings(MWApplySettingsMessage message)
        {
            ItemSyncMod.SettingsSyncer.SetSettings(message.Settings);
        }

        private void HandleInitiateGame(MWInitiateSyncGameMessage message)
        {
            ItemSyncMod.SettingsSyncer.SetSettings(message.Settings);
            MenuHolder.MenuInstance.LockSettingsButtons();
        }

        private void HandleResult(MWResultMessage message)
        {
            ItemSyncMod.ISSettings.URL = ItemSyncMod.GS.URL;
            ItemSyncMod.ISSettings.MWPlayerId = message.ResultData.playerId;
            ItemSyncMod.ISSettings.MWRandoId = message.ResultData.randoId;
            ItemSyncMod.ISSettings.UserName = ItemSyncMod.GS.UserName;
            ItemSyncMod.ISSettings.IsItemSync = true;

            ItemSyncMod.ISSettings.SyncVanillaItems = ItemSyncMod.GS.SyncVanillaItems;
            ItemSyncMod.ISSettings.AdditionalFeaturesEnabled = ItemSyncMod.GS.AdditionalFeaturesEnabled;

            JoinRando(ItemSyncMod.ISSettings.MWRandoId, ItemSyncMod.ISSettings.MWPlayerId);
            GameStarted?.Invoke();
        }

        public void NotifySave()
        {
            SendMessage(new MWSaveMessage { });
        }

        public void SendItemToAll(string item)
        {
            LogDebug("Sending " + item);
            MWItemSendMessage msg = new() {  Location = "", Item = item, To = -2 }; // -2 is an ItemSync magic
            lock (ConfirmableMessagesQueue) {
                ConfirmableMessagesQueue.Add(msg);
            }
            SendMessage(msg);
        }

        public void SendVisitStateChanged(string name, string[] previewTexts, PreviewRecordTagType previewRecordTag, VisitState newVisitFlags)
        {
            LogDebug($"Sending visit state changed name: {name}, previews: {string.Join(", ", previewTexts)}, isMulti: {previewRecordTag}, newFlags: {newVisitFlags}");
            SendMessage(new MWVisitStateChangedMessage
            {
                Name = name,
                PreviewTexts = previewTexts,
                NewVisitFlags = newVisitFlags,
                PreviewRecordTagType = previewRecordTag
            });
        }

        public void SendTransitionFound(string source, string target)
        {
            LogDebug($"Sending transition found ${source}->{target}");
            SendMessage(new MWTransitionFoundMessage() { Source = source, Target = target });
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
