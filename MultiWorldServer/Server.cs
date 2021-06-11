using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiWorldLib.Messaging;
using MultiWorldLib.Binary;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using MultiWorldLib.Messaging.Definitions.Messages;

using System.IO;
using MultiWorldLib;

namespace MultiWorldServer
{
    class Server
    {
        const int PingInterval = 10000; //In milliseconds

        private ulong nextUID = 1;
        private readonly MWMessagePacker Packer = new MWMessagePacker(new BinaryMWMessageEncoder());
        private readonly List<Client> Unidentified = new List<Client>();

        private readonly Timer PingTimer;

        private readonly object _clientLock = new object();
        private readonly Dictionary<ulong, Client> Clients = new Dictionary<ulong, Client>();
        private readonly Dictionary<int, GameSession> GameSessions = new Dictionary<int, GameSession>();
        private readonly Dictionary<string, Dictionary<ulong, int>> ready = new Dictionary<string, Dictionary<ulong, int>>();
        private readonly Dictionary<string, Dictionary<ulong, (int, RandoResult)>> gameGeneratingRooms = new Dictionary<string, Dictionary<ulong, (int, RandoResult)>>(); // TODO replace RandoResult with ItemsPool
        private readonly Dictionary<int, RandoResult> unsavedResults = new Dictionary<int, RandoResult>();
        private TcpListener _server;
        private readonly Timer ResendTimer;

        private static StreamWriter LogWriter;

        public bool Running { get; private set; }

        public Server(int port)
        {
            //Listen on any ip
            _server = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
            _server.Start();

            //_readThread = new Thread(ReadWorker);
            //_readThread.Start();
            _server.BeginAcceptTcpClient(AcceptClient, _server);
            PingTimer = new Timer(DoPing, Clients, 1000, PingInterval);
            ResendTimer = new Timer(DoResends, Clients, 500, 1000);
            Running = true;
            Log($"Server started on port {port}!");
        }

        internal static void OpenLogger(string filename)
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }
            filename = "Logs/" + filename + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
            FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            LogWriter = new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true };
        }

        internal static void Log(string message, int? session = null)
        {
            if (session == null)
            {
                LogWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {message}");
            }
            else
            {
                LogWriter.WriteLine($"[{DateTime.Now.ToLongTimeString()}] [{session}] {message}");
            }
        }

        internal static void LogToConsole(string message)
        {
            Console.WriteLine(message);
            Log(message);
        }

        public void GiveItem(string item, int session, int player)
        {
            LogToConsole($"Giving item {item} to player {player + 1} in session {session}");
            string suffix;
            (item, suffix) = LanguageStringManager.ExtractSuffix(item);

            if (item == null)
            {
                LogToConsole($"Invalid item: {item}");
                return;
            }

            if (!GameSessions.ContainsKey(session))
            {
                LogToConsole($"Session {session} does not exist");
                return;
            }

            GameSessions[session].SendItemTo(player, item + suffix, "Magic", "Server");
        }

        public void ListSessions()
        {
            LogToConsole($"{GameSessions.Count} current sessions");
            foreach (var kvp in GameSessions)
            {
                LogToConsole($"ID: {kvp.Key} players: {kvp.Value.getPlayerString()}");
            }
        }

        public void ListReady()
        {
            LogToConsole($"{ready.Count} current lobbies");
            foreach (var kvp in ready)
            {
                string playerString = string.Join(", ", kvp.Value.Keys.Select((uid) => Clients[uid].Nickname).ToArray());
                LogToConsole($"Room: {kvp.Key} players: {playerString}");
            }
        }

        private void DoPing(object clients)
        {
            lock (_clientLock)
            {
                var Now = DateTime.Now;
                List<Client> clientList = Clients.Values.ToList();
                for (int i = clientList.Count - 1; i >= 0; i--)
                {
                    Client client = clientList[i];
                    //If a client has missed 3 pings we disconnect them
                    if (Now - client.lastPing > TimeSpan.FromMilliseconds(PingInterval * 3.5))
                    {
                        Log(string.Format("Client {0} timed out. ({1})", client.UID, client.Session?.Name));
                        DisconnectClient(client);
                    }
                    else
                        SendMessage(new MWPingMessage(), client);
                }
            }
        }

        private void DoResends(object clients)
        {
            try
            {
                lock (_clientLock)
                {
                    var ClientList = Clients.Values.ToList();
                    for (int i = ClientList.Count - 1; i >= 0; i--)
                    {
                        var client = ClientList[i];
                        if (client.Session != null)
                        {
                            lock (client.Session.MessagesToConfirm)
                            {
                                var now = DateTime.Now;
                                for (int j = client.Session.MessagesToConfirm.Count - 1; j >= 0; j--)
                                {
                                    var entry = client.Session.MessagesToConfirm[j];
                                    if (now - entry.LastSent > TimeSpan.FromSeconds(5))
                                    {
                                        var msg = entry.Message;
                                        SendMessage(msg, client);
                                        entry.LastSent = now;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // I don't really like doing this but I was occasionally getting NullRefeneceExceptions here
                Log($"Error resending items: {e.Message}");
            }
        }

        private void StartReadThread(Client c)
        {
            //Check that we aren't already reading
            if (c.ReadWorker != null)
                return;
            var start = new ParameterizedThreadStart(ReadWorker);
            c.ReadWorker = new Thread(start);
            c.ReadWorker.Start(c);
        }

        private void ReadWorker(object boxedClient)
        {
            Client client = boxedClient as Client;
            NetworkStream stream = client.TcpClient.GetStream();
            try
            {
                while (client.TcpClient.Connected)
                {
                    MWPackedMessage message = new MWPackedMessage(stream);
                    ReadFromClient(client, message);
                }
            }
            catch (Exception)
            {
                DisconnectClient(client);
            }
        }

        private void AcceptClient(IAsyncResult res)
        {
            try
            {
                Client client = new Client
                {
                    TcpClient = _server.EndAcceptTcpClient(res)
                };

                _server.BeginAcceptTcpClient(AcceptClient, _server);

                if (!client.TcpClient.Connected)
                {
                    return;
                }

                client.TcpClient.ReceiveTimeout = 2000;
                client.TcpClient.SendTimeout = 2000;
                client.lastPing = DateTime.Now;

                lock (_clientLock)
                {
                    Unidentified.Add(client);
                }

                StartReadThread(client);
            } catch (Exception e) // Not sure what could throw here, but have been seeing random rare exceptions in the servers
            {
                Log("Error when accepting client: " + e.Message);
            }
        }

        private bool SendMessage(MWMessage message, Client client)
        {
            if (client?.TcpClient == null || !client.TcpClient.Connected)
            {
                //Log("Returning early due to client not connected");
                return false;
            }

            try
            {
                byte[] bytes = Packer.Pack(message).Buffer;
                lock (client.TcpClient)
                {
                    NetworkStream stream = client.TcpClient.GetStream();
                    stream.WriteTimeout = 2000;
                    stream.Write(bytes, 0, bytes.Length);
                }
                return true;
            }
            catch (Exception e)
            {
                Log($"Failed to send message to '{client.Session?.Name}':\n{e}\nDisconnecting");
                DisconnectClient(client);
                return false;
            }
        }

        private static void WriteToClient(IAsyncResult res)
        {
            Client c = res.AsyncState as Client;
            if (c == null)
                throw new InvalidOperationException("How the fuck was this ever called with a null state object?");
            try
            {
                NetworkStream stream = (NetworkStream)c.TcpClient.GetStream();
                stream.EndWrite(res);
            }
            catch (Exception)
            {
            }
            finally
            {
                Monitor.Exit(c.SendLock);
            }
        }

        private void DisconnectClient(Client client)
        {
            Log(string.Format("Disconnecting UID {0}", client.UID));
            try
            {
                //Remove first from lists so if we get a network exception at least on the server side stuff should be clean
                Unready(client.UID);
                lock (_clientLock)
                {
                    Clients.Remove(client.UID);
                    Unidentified.Remove(client);
                    RemovePlayerFromSession(client);
                }
                SendMessage(new MWDisconnectMessage(), client);
                //Wait a bit to give the message a chance to be sent at least before closing the client
                Thread.Sleep(10);
                client.TcpClient.Close();
            }
            catch (Exception e)
            {
                //Do nothing, we're already disconnecting
                Log("Exception disconnecting client: " + e);
            }
        }

        private void RemovePlayerFromSession(Client client)
        {
            lock (_clientLock)
            {
                if (client.Session != null)
                {
                    GameSessions[client.Session.randoId].RemovePlayer(client);

                    // TODO: Leaving this out for now, meaning fully async multiworlds should be possible
                    // Maybe put a timeout on sessions so they don't last forever, but for now this is ok

                    /*if (GameSessions[client.Session.randoId].isEmpty())
                    {
                        Log($"Removing session for rando id: {client.Session.randoId}");
                        GameSessions.Remove(client.Session.randoId);
                    }*/
                    client.Session = null;
                }
            }
        }

        private void ReadFromClient(Client sender, MWPackedMessage packed)
        {
            MWMessage message;
            try
            {
                message = Packer.Unpack(packed);
            }
            catch (Exception e)
            {
                Log(e.ToString());
                return;
            }

            switch (message.MessageType)
            {
                case MWMessageType.SharedCore:
                    break;
                case MWMessageType.ConnectMessage:
                    HandleConnect(sender, (MWConnectMessage)message);
                    break;
                case MWMessageType.ReconnectMessage:
                    break;
                case MWMessageType.DisconnectMessage:
                    HandleDisconnect(sender, (MWDisconnectMessage)message);
                    break;
                case MWMessageType.JoinMessage:
                    HandleJoin(sender, (MWJoinMessage)message);
                    break;
                case MWMessageType.JoinConfirmMessage:
                    break;
                case MWMessageType.LeaveMessage:
                    HandleLeaveMessage(sender, (MWLeaveMessage)message);
                    break;
                case MWMessageType.ItemReceiveMessage:
                    break;
                case MWMessageType.ItemReceiveConfirmMessage:
                    HandleItemReceiveConfirm(sender, (MWItemReceiveConfirmMessage)message);
                    break;
                case MWMessageType.ItemSendMessage:
                    HandleItemSend(sender, (MWItemSendMessage)message);
                    break;
                case MWMessageType.ItemSendConfirmMessage:
                    break;
                case MWMessageType.NotifyMessage:
                    HandleNotify(sender, (MWNotifyMessage)message);
                    break;
                case MWMessageType.PingMessage:
                    HandlePing(sender, (MWPingMessage)message);
                    break;
                case MWMessageType.ReadyMessage:
                    HandleReadyMessage(sender, (MWReadyMessage)message);
                    break;
                case MWMessageType.RejoinMessage:
                    HandleRejoinMessage(sender, (MWRejoinMessage)message);
                    break;
                case MWMessageType.UnreadyMessage:
                    HandleUnreadyMessage(sender, (MWUnreadyMessage)message);
                    break;
                case MWMessageType.InitiateGameMessage:
                    HandleInitiateGameMessage(sender, (MWInitiateGameMessage)message);
                    break;
                case MWMessageType.RandoGeneratedMessage:
                    HandleRandoGeneratedMessage(sender, (MWRandoGenerated)message);
                    break;
                case MWMessageType.SaveMessage:
                    HandleSaveMessage(sender, (MWSaveMessage)message);
                    break;
                case MWMessageType.InvalidMessage:
                default:
                    throw new InvalidOperationException("Received Invalid Message Type");
            }
        }

        private void HandleConnect(Client sender, MWConnectMessage message)
        {
            //Log(string.Format("Seeing connection with UID={0}", message.SenderUid));
            lock (_clientLock)
            {
                if (Unidentified.Contains(sender))
                {
                    if (message.SenderUid == 0)
                    {
                        sender.UID = nextUID++;
                        Log(string.Format("Assigned UID={0}", sender.UID));
                        SendMessage(new MWConnectMessage { SenderUid = sender.UID }, sender);
                        Log("Connect sent!");
                        Clients.Add(sender.UID, sender);
                        Unidentified.Remove(sender);
                    }
                    else
                    {
                        Unidentified.Remove(sender);
                        sender.TcpClient.Close();
                    }
                }
            }
        }

        private void HandleDisconnect(Client sender, MWDisconnectMessage message)
        {
            DisconnectClient(sender);
        }

        private void HandlePing(Client sender, MWPingMessage message)
        {
            sender.lastPing = DateTime.Now;
        }

        private void HandleJoin(Client sender, MWJoinMessage message)
        {
            lock (_clientLock)
            {
                if (!Clients.ContainsKey(sender.UID))
                {
                    return;
                }

                if (!GameSessions.ContainsKey(message.RandoId))
                {
                    Log($"Starting session for rando id: {message.RandoId}");
                    GameSessions[message.RandoId] = new GameSession(message.RandoId);
                }

                GameSessions[message.RandoId].AddPlayer(sender, message);
                SendMessage(new MWJoinConfirmMessage(), sender);
            }
        }

        private void HandleLeaveMessage(Client sender, MWLeaveMessage message)
        {
            RemovePlayerFromSession(sender);
        }

        private void HandleReadyMessage(Client sender, MWReadyMessage message)
        {
            sender.Nickname = message.Nickname;
            sender.Room = message.Room;
            lock (_clientLock)
            {
                if (!ready.ContainsKey(sender.Room))
                {
                    ready[sender.Room] = new Dictionary<ulong, int>();
                }

                int readyId = (new Random()).Next();
                ready[sender.Room][sender.UID] = readyId;

                string roomText = string.IsNullOrEmpty(sender.Room) ? "default room" : $"room \"{sender.Room}\"";
                Log($"{sender.Nickname} (UID {sender.UID}) readied up in {roomText} ({ready[sender.Room].Count} readied)");

                string names = string.Join(", ", ready[sender.Room].Keys.Select((uid) => Clients[uid].Nickname).ToArray());

                foreach (ulong uid in ready[sender.Room].Keys)
                {
                    SendMessage(new MWReadyConfirmMessage { Ready = ready[sender.Room].Count, Names = names, ReadyID = readyId }, Clients[uid]);
                }
            }
        }

        private void Unready(ulong uid)
        {
            if (!Clients.TryGetValue(uid, out Client c)) return;

            lock (_clientLock)
            {
                if (c.Room == null || !ready.ContainsKey(c.Room) || !ready[c.Room].ContainsKey(uid)) return;
                string roomText = string.IsNullOrEmpty(c.Room) ? "default room" : $"room \"{c.Room}\"";
                Log($"{c.Nickname} (UID {c.UID}) unreadied from {roomText} ({ready[c.Room].Count - 1} readied)");

                ready[c.Room].Remove(c.UID);
                if (ready[c.Room].Count == 0)
                {
                    ready.Remove(c.Room);
                    return;
                }

                string names = "";
                foreach (ulong uid2 in ready[c.Room].Keys)
                {
                    names += Clients[uid2].Nickname;
                    names += ", ";
                }

                if (names.Length >= 2)
                {
                    names = names.Substring(0, names.Length - 2);
                }

                foreach (var kvp in ready[c.Room])
                {
                    if (!Clients.ContainsKey(kvp.Key)) continue;
                    SendMessage(new MWReadyConfirmMessage { Ready = ready[c.Room].Count, Names = names, ReadyID = kvp.Value }, Clients[kvp.Key]);
                }
            }
        }

        private void HandleUnreadyMessage(Client sender, MWUnreadyMessage message)
        {
            Unready(sender.UID);
        }

        private void HandleSaveMessage(Client sender, MWSaveMessage message)
        {
            if (unsavedResults.ContainsKey(message.ReadyID))
            {
                unsavedResults.Remove(message.ReadyID);
            }

            if (sender.Session == null) return;

            GameSessions[sender.Session.randoId].Save(sender.Session.playerId);
        }

        private void HandleRejoinMessage(Client sender, MWRejoinMessage message)
        {
            if (!unsavedResults.ContainsKey(message.ReadyID))
            {
                Log($"Bad rejoin attempt (readyId = {message.ReadyID}, UID = {sender.UID})");
            }
            else
            {
                SendMessage(new MWResultMessage { Result = unsavedResults[message.ReadyID] }, sender);
            }
        }

        private void HandleInitiateGameMessage(Client sender, MWInitiateGameMessage message)
        {
            string room = sender.Room;

            lock (_clientLock)
            {
                if (room == null || !ready.ContainsKey(room) || !ready[room].ContainsKey(sender.UID)) return;
                if (gameGeneratingRooms.ContainsKey(room)) return;

                gameGeneratingRooms[room] = new Dictionary<ulong, (int, RandoResult)>();
            }
            
            foreach (var kvp in ready[room])
            {
                Client client = Clients[kvp.Key];
                SendMessage(new MWRequestRandoMessage(), client);
            }
        }

        private void HandleRandoGeneratedMessage(Client sender, MWRandoGenerated message)
        {
            string room = sender.Room;

            lock (_clientLock)
            {
                if (room == null || !ready.ContainsKey(room) || !ready[room].ContainsKey(sender.UID)) return;
                if (!gameGeneratingRooms.ContainsKey(room) || gameGeneratingRooms[room].ContainsKey(sender.UID)) return;
            }

            // set sender's rando in the list
            gameGeneratingRooms[room][sender.UID] = (ready[room][sender.UID], null); // TODO replace null with message.ItemsPool
            // If list contains as many randos as needed, proceed. 
            // Make sure to mark anything to prevent leftover randos from same room sent after this continued flow

            // Most is relevant once everyone provided generated randos

            List<Client> clients = new List<Client>();
            List<int> readyIds = new List<int>();
            List<string> nicknames = new List<string>();

            string roomText = string.IsNullOrEmpty(sender.Room) ? "default room" : $"room \"{sender.Room}\"";
            Log($"Starting MW for {roomText} at request of {sender.Nickname}");

            lock (_clientLock)
            {
                if (!ready[room].ContainsKey(sender.UID)) return;

                foreach (var kvp in ready[room])
                {
                    clients.Add(Clients[kvp.Key]);
                    readyIds.Add(kvp.Value);
                    nicknames.Add(Clients[kvp.Key].Nickname);
                }
            }

            Log("Starting rando with players:");
            foreach (string nickname in nicknames)
            {
                Log(nickname);
            }

            Log("Randomizing world...");
            List<(int, string, string)[]> playersItems = new List<(int, string, string)[]>();

            ItemsRandomizer itemsRandomizer = new ItemsRandomizer(playersItems, nicknames);
            List<RandoResult> results = itemsRandomizer.RandomizeItems();
            Log("Done randomization");

            Dictionary<string, RandoResult> clientsResults = new Dictionary<string, RandoResult>();
            for (int i=0; i < results.Count; i++)
            {
                clientsResults.Add(clients[i].Nickname, results[i]);
            }
            
            /* TODO
             * general items spoiler log
            string spoilerLocalPath = $"Spoilers/{results[0].randoId}.txt";
            string itemsSpoiler = SpoilerLogger.GetItemSpoiler(results[0]);
            SaveItemSpoilerFile(results[0], spoilerLocalPath, itemsSpoiler);
            Log($"Done generating spoiler log");
            */

            Log("Sending to players...");
            for (int i = 0; i < results.Count; i++)
            {
                unsavedResults[readyIds[i]] = results[i];
                Log($"Sending to player {i + 1}");
                SendMessage(new MWResultMessage { Result = results[i] }, clients[i]);
            }
            Log($"Done sending to players!");
        }

        private void SaveItemSpoilerFile(RandoResult result, string path, string itemsSpoiler)
        {
            if (!Directory.Exists("Spoilers"))
            {
                Directory.CreateDirectory("Spoilers");
            }
            /* TODO uncomment once implemented
             * SpoilerLogger spoilerLogger = new SpoilerLogger(path);
            spoilerLogger.LogSpoiler(itemsSpoiler);*/
        }

        private void HandleNotify(Client sender, MWNotifyMessage message)
        {
            Log($"[{sender.Session?.Name}]: {message.Message}");
        }

        private void HandleItemReceiveConfirm(Client sender, MWItemReceiveConfirmMessage message)
        {
            List<MWMessage> confirmed = sender.Session.ConfirmMessage(message);

            foreach (MWMessage msg in confirmed)
            {
                switch (msg.MessageType)
                {
                    case MWMessageType.ItemReceiveMessage:
                        MWItemReceiveMessage itemMsg = msg as MWItemReceiveMessage;
                        GameSessions[sender.Session.randoId].ConfirmItem(sender.Session.playerId, itemMsg);
                        break;
                    default:
                        break;
                }
            }
        }

        private void HandleItemSend(Client sender, MWItemSendMessage message)
        {
            if (sender.Session == null) return;  // Throw error?

            //Confirm sending the item to the sender
            SendMessage(new MWItemSendConfirmMessage { Location = message.Location, Item = message.Item, To = message.To}, sender);
            GameSessions[sender.Session.randoId].SendItemTo(message.To, message.Item, message.Location, sender.Session.Name);
        }

        private Client GetClient(ulong uuid)
        {
            lock (_clientLock)
            {
                return Clients.TryGetValue(uuid, out Client client) ? client : null;
            }
        }
    }
}
