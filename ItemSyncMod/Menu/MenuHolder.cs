using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.Extensions;
using ItemSyncMod.MenuExtensions;
using RandomizerMod.RC;
using MenuChanger.MenuPanels;

namespace ItemSyncMod.Menu
{
    internal class MenuHolder
    {
        internal static MenuHolder MenuInstance { get; private set; }
        
        internal delegate void MenuReverted();
        internal delegate void Connected();
        internal delegate void Disconnected();
        internal delegate void Ready(Dictionary<string, string> metadata);
        internal delegate void Unready();
        internal delegate void RoomStateUpdated(int playersCount, string[] playersNames);
        internal delegate void LockSettings();
        internal delegate void GameStarted();
        internal delegate void GameJoined();

        internal event MenuReverted OnMenuRevert;
        internal event Connected OnConnected;
        internal event Disconnected OnDisconnected;
        internal event Ready OnReady;
        internal event Unready OnUnready;
        internal event RoomStateUpdated OnRoomStateUpdated;
        internal event LockSettings OnLockSettings;
        internal event GameStarted OnGameStarted;
        internal event GameJoined OnGameJoined;

        internal SettingsSharer SettingsSharer;
        internal bool IsReadied => readyButton.Value;

        private MenuPage menuPage;
        private BigButton openMenuButton, startButton, joinGameButton;
        private DynamicToggleButton connectButton, readyButton;
        private EntryField<string> urlInput;
        private MenuLabel serverNameLabel;
        private LockableEntryField<string> nicknameInput, roomInput;
        private CounterLabel readyPlayersCounter;
        private DynamicLabel readyPlayersBox;
        private Thread connectThread;

        private MenuLabel additionalSettingsLabel, localPreferencesLabel;
        private ToggleButton syncVanillaItemsButton, syncSimpleKeysUsagesButton;

        #region Extensions Menu
        private MenuPage extensionsMenuPage;
        private SmallButton openExtensionsMenuButton;
        private MultiGridItemPanel extensionsGrid;
        #endregion

        internal static void ConstructMenu(MenuPage connectionsPage)
        {
            MenuInstance = new();
            MenuInstance.OnMenuConstruction(connectionsPage);
        }
        
        internal static void DisposeMenu()
        {
            MenuInstance = null;
        }

        internal static bool GetItemSyncMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button)
            => MenuInstance.GetMenuButton(rc, landingPage, out button);

        internal void ShowStartGameFailure()
        {
            connectButton.Show();
            readyPlayersBox.SetText("Failed to start game.\nPlease check ModLog.txt for more info.");
        }

        private void OnMenuConstruction(MenuPage finalPage)
        {
            SettingsSharer = new();

            CreateMenuElements(finalPage);
            AddEvents();
            Arrange();

            RevertToInitialState();
        }

        private void CreateMenuElements(MenuPage finalPage)
        {
            menuPage = new("Continue", finalPage);
            openMenuButton = new(finalPage, "ItemSync");

            urlInput = new(menuPage, "URL: ");
            urlInput.InputField.characterLimit = 120;
            connectButton = new(menuPage, "Connect");
            serverNameLabel = new(menuPage, "Server Name: ");

            nicknameInput = new(menuPage, "Nickname: ");
            nicknameInput.InputField.characterLimit = 30;

            roomInput = new(menuPage, "Room: ");
            roomInput.InputField.characterLimit = 60;
            readyButton = new(menuPage, "Ready");
            readyPlayersBox = new(menuPage, "", MenuLabel.Style.Body);
            readyPlayersCounter = new(menuPage, "Ready Players: ");

            startButton = new(menuPage, "Start ItemSync");

            additionalSettingsLabel = new(menuPage, "Additional Settings");
            syncVanillaItemsButton = new(menuPage, "Sync Vanilla Items");
            syncSimpleKeysUsagesButton = new(menuPage, "Sync Simple Keys Usages");

            localPreferencesLabel = new(menuPage, "Local Preferences");
            
            joinGameButton = new(menuPage, "Join Game");
            joinGameButton.AddSetResumeKeyEvent("Randomizer");
            joinGameButton.Hide(); // Always hidden for obvious reasons

            // Extensions Menu
            extensionsMenuPage = new("ItemSyncExtensionsMenu", menuPage);
            openExtensionsMenuButton = new(menuPage, "Extensions");
            ExtensionsMenuAPI.ResetMenuEvents();

            extensionsGrid = new(extensionsMenuPage, 8, 3, 60f, 650f, new(0, 300), Array.Empty<SmallButton>());
            ExtensionsMenuAPI.ConstructExtensionsMenus(extensionsMenuPage).ForEach(
                button => extensionsGrid.Add(button));

            // Load last values from settings
            urlInput.SetValue(ItemSyncMod.GS.URL);
            nicknameInput.SetValue(ItemSyncMod.GS.UserName);
            syncVanillaItemsButton.SetValue(ItemSyncMod.GS.SyncVanillaItems);
            syncSimpleKeysUsagesButton.SetValue(ItemSyncMod.GS.SyncSimpleKeysUsages);
        }

        internal void ConnectionOnConnect(ulong uid, string serverName) => ConnectAcknowledged(uid, serverName);

        internal void ConnectionOnReadyConfirm(int ready, string[] names) => InvokeOnRoomStateUpdated(ready, names);

        internal void ConnectionOnReadyDeny(string msg) => ThreadSupport.BeginInvoke(() => ShowReadyDeny(msg));

        internal void ConnectionOnGameStarted() => InvokeOnGameStarted();

        private void AddEvents()
        {
            openMenuButton.AddHideAndShowEvent(menuPage);
            connectButton.OnClick += () => ThreadSupport.BeginInvoke(ConnectClicked);

            nicknameInput.ValueChanged += (value) => ThreadSupport.BeginInvoke(() => UpdateNickname(value));
            nicknameInput.InputField.onValidateInput += (text, index, c) => c == ',' ? '.' : c; // ICU

            readyButton.OnClick += () => ThreadSupport.BeginInvoke(ReadyClicked);
            OnRoomStateUpdated += (num, players) => ThreadSupport.BeginInvoke(() => UpdateReadyPlayersLabel(num, players));
            OnRoomStateUpdated += (_, _) => ThreadSupport.BeginInvoke(EnsureStartButtonShown);

            #region Additional Sync Features
            SettingsSharer.OnValueChanged callback1 = SettingsSharer.RegisterSharedSetting(
                "ItemSync-SyncVanillaItems", GetSyncVanillaItems, SetSyncVanillaItems);
            syncVanillaItemsButton.OnClick += () => callback1();
            syncVanillaItemsButton.ValueChanged += value => 
                ItemSyncMod.GS.SyncVanillaItems = value;

            SettingsSharer.OnValueChanged callback2 = SettingsSharer.RegisterSharedSetting(
                "ItemSync-SyncSimpleKeysUsage", GetSyncSimpleKeysUsages, SetSyncSimpleKeysUsages);
            syncSimpleKeysUsagesButton.OnClick += () => callback2();
            syncSimpleKeysUsagesButton.ValueChanged += value =>
                ItemSyncMod.GS.SyncSimpleKeysUsages = value;

            #endregion

            startButton.OnClick += () => ThreadSupport.BeginInvoke(InitiateGame);
            OnGameStarted += () => ThreadSupport.BeginInvoke(ShowJoinGameButton);
            OnGameStarted += () => ThreadSupport.BeginInvoke(openExtensionsMenuButton.Lock);

            joinGameButton.OnClick += InvokeOnGameJoined;
            OnGameJoined += () => ThreadSupport.BeginInvoke(StartNewGame);

            #region Extensions Menu
            openExtensionsMenuButton.AddHideAndShowEvent(extensionsMenuPage);
            OnMenuRevert += ExtensionsMenuAPI.InvokeOnMenuReverted;
            OnConnected += ExtensionsMenuAPI.InvokeOnConnected;
            OnDisconnected += ExtensionsMenuAPI.InvokeOnDisconnected;
            OnReady += metadata =>
            {
                ExtensionsMenuAPI.InvokeOnAddReadyMetadata(metadata);
                ExtensionsMenuAPI.InvokeOnReady();
            };
            OnUnready += ExtensionsMenuAPI.InvokeOnUnready;
            OnRoomStateUpdated += ExtensionsMenuAPI.InvokeRoomStateUpdated;
            OnLockSettings += ExtensionsMenuAPI.InvokeOnLockSettings;
            OnGameStarted += ExtensionsMenuAPI.InvokeOnGameStarted;
            OnGameJoined += ExtensionsMenuAPI.InvokeOnGameJoined;
            #endregion

            menuPage.backButton.OnClick += () => ThreadSupport.BeginInvoke(RevertToInitialState);
        }

        private void InvokeOnRoomStateUpdated(int count, string[] names) => OnRoomStateUpdated.Invoke(count, names);
        private void InvokeOnGameStarted() => OnGameStarted?.Invoke();
        private void InvokeOnGameJoined() => OnGameJoined?.Invoke();

        private void Arrange()
        {
            urlInput.MoveTo(new(0, 300));
            serverNameLabel.MoveTo(urlInput.Label.GameObject.transform.localPosition);
            connectButton.MoveTo(new(0, 250));

            nicknameInput.MoveTo(new(0, 140));
            roomInput.MoveTo(new(0, 60));
            readyButton.MoveTo(new(0, -40));
            readyPlayersBox.MoveTo(new(600, 430));
            readyPlayersCounter.MoveTo(new(600, 470));

            startButton.MoveTo(new(0, -130));

            additionalSettingsLabel.MoveTo(new(-600, 470));
            syncVanillaItemsButton.MoveTo(new(-600, 400));
            syncSimpleKeysUsagesButton.MoveTo(new(-600, 350));
            openExtensionsMenuButton.MoveTo(new(-600, 280));

            localPreferencesLabel.MoveTo(new(-600, -300));
            
            urlInput.SymSetNeighbor(Neighbor.Down, connectButton);
            nicknameInput.SymSetNeighbor(Neighbor.Down, roomInput);
            roomInput.SymSetNeighbor(Neighbor.Down, readyButton);
            readyButton.SymSetNeighbor(Neighbor.Down, startButton);
            
            syncVanillaItemsButton.SymSetNeighbor(Neighbor.Down, syncSimpleKeysUsagesButton);
            
            syncVanillaItemsButton.SymSetNeighbor(Neighbor.Right, readyButton);
            syncSimpleKeysUsagesButton.SetNeighbor(Neighbor.Right, readyButton);
        }

        private void RevertToInitialState()
        {
            // Set menu objects (in)active
            urlInput.Show();
            serverNameLabel.Hide();
            connectButton.Show();
            connectButton.SetText("Connect");
            connectButton.SetValue(false);

            nicknameInput.Hide();
            roomInput.Hide();

            readyButton.Unlock();
            readyButton.SetText("Ready");
            readyButton.SetValue(false);
            readyButton.Hide();

            readyPlayersBox.SetText("");
            readyPlayersBox.Hide();
            readyPlayersCounter.Set(0);
            readyPlayersCounter.Hide();

            startButton.Hide();
            joinGameButton.Hide();

            openExtensionsMenuButton.Show();
            UnlockSettingsButton();
            
            ItemSyncMod.Connection.Disconnect();
            try
            {
                OnMenuRevert?.Invoke();
            }
            catch (Exception e)
            {
                LogHelper.LogError($"OnMenuRevert error: {e.Message}");
                LogHelper.LogError(e.StackTrace);
            }
        }

        private bool GetMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            button = openMenuButton;
            ItemSyncMod.Controller = new(rc, this);
            return true;
        }

        private void UpdateNickname(string newNickname)
        {
            ItemSyncMod.GS.UserName = newNickname;
        }

        private void ConnectClicked()
        {
            bool newValue = connectButton.Value;
            if (newValue)
            {
                if (connectThread is not null && connectThread.IsAlive) connectThread.Abort();
                connectButton.SetText("Connecting");
                connectThread = new Thread(Connect);
                connectThread.Start();
            }
            else
            {
                RevertToInitialState();
                OnDisconnected?.Invoke();
            }
        }

        private void Connect()
        {
            try
            {
                string url = urlInput.Value;
                ItemSyncMod.Connection.Connect(url);
                ItemSyncMod.GS.URL = url;
            }
            catch
            {
                LogHelper.Log("Failed to connect!");
                connectButton.SetValue(false);
                return;
            }

        }

        private void ConnectAcknowledged(ulong uid, string serverName)
        {
            ThreadSupport.BeginInvoke(() =>
            {
                connectButton.SetText("Disconnect");
                urlInput.Hide();
                serverNameLabel.Text.text = $"Server Name: {serverName}";
                serverNameLabel.Show();

                nicknameInput.Show();
                nicknameInput.Unlock();

                roomInput.Show();
                roomInput.Unlock();

                readyButton.Show();
                OnConnected?.Invoke();
            });
        }

        private void ReadyClicked()
        {
            if (readyButton.Value)
            {
                nicknameInput.Lock();
                roomInput.Lock();
                Dictionary<string, string> metadataDict = new();
                OnReady?.Invoke(metadataDict);
                ItemSyncMod.Connection.ReadyUp(roomInput.Value, ItemSyncMod.Controller.GetRandoHash(), metadataDict.Select(e => (e.Key, e.Value)).ToArray());
                readyPlayersBox.Show();
                readyPlayersCounter.Show();
            }
            else
            {
                if (ItemSyncMod.Connection.IsConnected()) ItemSyncMod.Connection.Unready();
                startButton.Hide();

                readyButton.SetText("Ready");
                readyPlayersBox.SetText("");
                readyPlayersBox.Hide();
                readyPlayersCounter.Set(0);
                readyPlayersCounter.Hide();

                nicknameInput.Unlock();
                roomInput.Unlock();

                UnlockSettingsButton();
                OnUnready?.Invoke();
            }
        }

        private void UpdateReadyPlayersLabel(int num, string[] playersNames)
        {
            readyPlayersCounter.Set(num);
            readyPlayersBox.SetText(playersNames);
        }

        private void EnsureStartButtonShown()
        {
            if (startButton.Hidden)
            {
                startButton.Show();
                readyButton.SetText("Unready");
            }
        }

        private void ShowReadyDeny(string description)
        {
            readyButton.SetText("Ready");
            readyButton.SetValue(false);
            readyPlayersBox.SetText(description);
            nicknameInput.Unlock();
            roomInput.Unlock();

            roomInput.InputField.Select();
            OnUnready?.Invoke();
        }

        private void InitiateGame()
        {
            readyButton.Lock();
            startButton.Hide();

            LockSettingsButtons();
            ItemSyncMod.Connection.InitiateGame(SettingsSharer.GetSerializedSettings());
        }

        internal string GetSyncSimpleKeysUsages() => syncSimpleKeysUsagesButton.Value.ToString();

        internal void SetSyncSimpleKeysUsages(string literalValue)
        {
            ThreadSupport.BeginInvoke(() =>
            {
                if (!syncSimpleKeysUsagesButton.Locked)
                    syncSimpleKeysUsagesButton.SetValue(bool.Parse(literalValue));
            });
        }

        internal string GetSyncVanillaItems() => syncVanillaItemsButton.Value.ToString();

        internal void SetSyncVanillaItems(string literalValue)
        {
            ThreadSupport.BeginInvoke(() =>
            {
                if (!syncVanillaItemsButton.Locked)
                    syncVanillaItemsButton.SetValue(bool.Parse(literalValue));
            });
        }

        internal void LockSettingsButtons()
        {
            ThreadSupport.BeginInvoke(() => {
                syncVanillaItemsButton.Lock();
                syncSimpleKeysUsagesButton.Lock();
                openExtensionsMenuButton.Lock();
            });

            OnLockSettings?.Invoke();
        }

        internal void UnlockSettingsButton()
        {
            ThreadSupport.BeginInvoke(() =>
            {
                syncVanillaItemsButton.Unlock();
                syncSimpleKeysUsagesButton.Unlock();
                openExtensionsMenuButton.Unlock();
            });
        }

        // Workaround for unity main thread crashes
        private void StartNewGame() => ItemSyncMod.Controller.StartGame();

        internal void ShowJoinGameButton()
        {
            connectButton.Hide();
            readyButton.Hide();
            startButton.Hide();
            joinGameButton.Show();
        }
    }
}
