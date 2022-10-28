using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.Extensions;
using ItemSyncMod.MenuExtensions;
using RandomizerMod.RC;

namespace ItemSyncMod
{
    internal class MenuHolder
    {
        public static MenuHolder MenuInstance { get; private set; }
        
        public delegate void MenuConstructed();
        public delegate void MenuReverted();
        public delegate void Connected();
        public delegate void Disconnected();
        public delegate void Ready();
        public delegate void Unready();
        public delegate void GameJoined();

        public event MenuConstructed OnMenuConstructed;
        public event MenuReverted OnMenuRevert;
        public event Connected OnConnected;
        public event Disconnected OnDisconnected;
        public event Ready OnReady;
        public event Unready OnUnready;
        public event GameJoined OnGameJoined;

        private MenuPage menuPage;
        private BigButton openMenuButton, startButton, joinGameButton;
        private DynamicToggleButton connectButton, readyButton, additionalFeaturesToggleButton;
        private EntryField<string> urlInput;
        private MenuLabel serverNameLabel;
        private LockableEntryField<string> nicknameInput, roomInput;
        private CounterLabel readyPlayersCounter;
        private DynamicLabel readyPlayersBox;
        private Thread connectThread;

        private MenuLabel additionalSettingsLabel, localPreferencesLabel;
        private ToggleButton syncVanillaItemsButton, syncSimpleKeysUsagesButton;

        internal static void ConstructMenu(MenuPage connectionsPage)
        {
            MenuInstance ??= new();
            MenuInstance.OnMenuConstruction(connectionsPage);
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
            CreateMenuElements(finalPage);
            AddEvents();
            Arrange();

            OnMenuConstructed?.Invoke();
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
            additionalFeaturesToggleButton = new(menuPage, "Additional Features");

            joinGameButton = new(menuPage, "Join Game");
            joinGameButton.AddSetResumeKeyEvent("Randomizer");
            joinGameButton.OnClick += () => OnGameJoined?.Invoke();
            joinGameButton.Hide(); // Always hidden for obvious reasons

            // Load last values from settings
            urlInput.SetValue(ItemSyncMod.GS.URL);
            nicknameInput.SetValue(ItemSyncMod.GS.UserName);
            syncVanillaItemsButton.SetValue(ItemSyncMod.GS.SyncVanillaItems);
            syncSimpleKeysUsagesButton.SetValue(ItemSyncMod.GS.SyncSimpleKeysUsages);
            additionalFeaturesToggleButton.SetValue(ItemSyncMod.GS.AdditionalFeaturesEnabled && !ItemSyncMod.GS.ReducePreload);
        }

        private void AddEvents()
        {
            openMenuButton.AddHideAndShowEvent(menuPage);
            connectButton.OnClick += () => ThreadSupport.BeginInvoke(ConnectClicked);
            nicknameInput.ValueChanged += (value) => ThreadSupport.BeginInvoke(() => UpdateNickname(value));
            nicknameInput.InputField.onValidateInput += (text, index, c) => c == ',' ? '.' : c; // ICU
            readyButton.OnClick += () => ThreadSupport.BeginInvoke(ReadyClicked);
            startButton.OnClick += () => ThreadSupport.BeginInvoke(InitiateGame);
            ItemSyncMod.Connection.OnReadyConfirm = (num, players) => ThreadSupport.BeginInvoke(() => UpdateReadyPlayersLabel(num, players));
            ItemSyncMod.Connection.OnReadyConfirm += (_, _) => ThreadSupport.BeginInvoke(EnsureStartButtonShown);
            ItemSyncMod.Connection.OnReadyDeny = (msg) => ThreadSupport.BeginInvoke(() => ShowReadyDeny(msg));
            ItemSyncMod.Connection.OnConnect = ConnectAcknowledged;

            syncVanillaItemsButton.OnClick += () => ThreadSupport.BeginInvoke(SyncVanillaItems_OnClick);
            syncVanillaItemsButton.ValueChanged += value => 
                ItemSyncMod.GS.SyncVanillaItems = value;

            syncSimpleKeysUsagesButton.OnClick += () => ThreadSupport.BeginInvoke(SyncSimpleKeysUsagesButton_OnClick);
            syncSimpleKeysUsagesButton.ValueChanged += value =>
                ItemSyncMod.GS.SyncSimpleKeysUsages = value;

            additionalFeaturesToggleButton.InterceptChanged += AdditionalFeaturesToggleButton_InterceptChanged;
            
            menuPage.backButton.OnClick += () => ThreadSupport.BeginInvoke(RevertToInitialState);
            joinGameButton.OnClick += () => ThreadSupport.BeginInvoke(StartNewGame);
        }

        private void AdditionalFeaturesToggleButton_InterceptChanged(MenuItem self, ref object newValue, ref bool cancelChange)
        {
            if (!ItemSyncMod.GS.ReducePreload)
                ItemSyncMod.GS.AdditionalFeaturesEnabled = (bool)newValue;
            else
            {
                cancelChange = true;
                additionalFeaturesToggleButton.SetText("Mod Options & restart game\n\nDisable Reduce Preloads in");
            }
        }

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

            localPreferencesLabel.MoveTo(new(-600, -300));
            additionalFeaturesToggleButton.MoveTo(new(-600, -370));

            urlInput.SymSetNeighbor(Neighbor.Down, connectButton);
            nicknameInput.SymSetNeighbor(Neighbor.Down, roomInput);
            roomInput.SymSetNeighbor(Neighbor.Down, readyButton);
            readyButton.SymSetNeighbor(Neighbor.Down, startButton);
            
            syncVanillaItemsButton.SymSetNeighbor(Neighbor.Down, syncSimpleKeysUsagesButton);
            syncSimpleKeysUsagesButton.SymSetNeighbor(Neighbor.Down, additionalFeaturesToggleButton);
            
            syncVanillaItemsButton.SymSetNeighbor(Neighbor.Right, readyButton);
            syncSimpleKeysUsagesButton.SetNeighbor(Neighbor.Right, readyButton);
            additionalFeaturesToggleButton.SetNeighbor(Neighbor.Right, readyButton);
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

            UnlockSettingsButton();

            ItemSyncMod.Connection.Disconnect();
            OnMenuRevert?.Invoke();
        }

        private bool GetMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            button = openMenuButton;
            ItemSyncMod.Controller = new(rc, this);
            ItemSyncMod.Connection.GameStarted = () => ThreadSupport.BeginInvoke(ShowJoinGameButton);
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
                OnDisconnected?.Invoke();
                RevertToInitialState();
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
                ItemSyncMod.Connection.ReadyUp(roomInput.Value, ItemSyncMod.Controller.GetRandoHash());
                readyPlayersBox.Show();
                readyPlayersCounter.Show();
                OnReady?.Invoke();
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

        private void UpdateReadyPlayersLabel(int num, string players)
        {
            readyPlayersCounter.Set(num);
            readyPlayersBox.SetText(players);
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
            ItemSyncMod.Connection.InitiateGame(ItemSyncMod.SettingsSyncer.GetSerializedSettings());
        }

        private void SyncVanillaItems_OnClick()
        {
            if (readyButton.Value)
                ItemSyncMod.SettingsSyncer.SyncSetting(SettingsSyncer.SettingKey.SyncVanillaItems,
                    syncVanillaItemsButton.Value);
        }

        private void SyncSimpleKeysUsagesButton_OnClick()
        {
            if (readyButton.Value)
                ItemSyncMod.SettingsSyncer.SyncSetting(SettingsSyncer.SettingKey.SyncSimpleKeysUsages,
                    syncSimpleKeysUsagesButton.Value);
        }

        public void SetSyncVanillaItems(bool value)
        {
            ThreadSupport.BeginInvoke(() =>
            {
                if (!syncVanillaItemsButton.Locked)
                    syncVanillaItemsButton.SetValue(value);
            });
        }

        public void SetSyncSimpleKeysUsages(bool value)
        {
            ThreadSupport.BeginInvoke(() =>
            {
                if (!syncSimpleKeysUsagesButton.Locked)
                    syncSimpleKeysUsagesButton.SetValue(value);
            });
        }

        public void LockSettingsButtons()
        {
            syncVanillaItemsButton.Lock();
            syncSimpleKeysUsagesButton.Lock();
        }

        public void UnlockSettingsButton()
        {
            syncVanillaItemsButton.Unlock();
            syncSimpleKeysUsagesButton.Unlock();
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
