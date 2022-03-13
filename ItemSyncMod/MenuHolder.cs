using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.Extensions;
using ItemSyncMod.MenuExtensions;
using RandomizerMod.RC;

namespace ItemSyncMod
{
    internal class MenuHolder
    {
        internal static MenuHolder MenuInstance { get; private set; }

        private MenuPage menuPage;
        private BigButton ppenMenuButton, startButton;
        private DynamicToggleButton connectButton, readyButton, additionalFeaturesToggleButton;
        private EntryField<string> urlInput;
        private LockableEntryField<string> nicknameInput, roomInput;
        private CounterLabel readyPlayersCounter;
        private DynamicLabel readyPlayersBox;
        private Thread connectThread;

        private MenuLabel additionalSettingsLabel, localPreferencesLabel;
        private ToggleButton syncVanillaItemsButton;

        internal static void ConstructMenu(MenuPage connectionsPage)
        {
            MenuInstance ??= new();
            MenuInstance.OnMenuConstruction(connectionsPage);
        }

        internal static bool GetItemSyncMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button)
            => MenuInstance.GetMenuButton(rc, landingPage, out button);

        internal void ShowStartGameFailure()
        {
            readyButton.Unlock();
            readyPlayersBox.SetText("Failed to start game.\nPlease check ModLog.txt for more info.");
        }

        private void OnMenuConstruction(MenuPage finalPage)
        {
            CreateMenuElements(finalPage);
            AddEvents();
            Arrange();
            RevertToInitialState();
        }

        private void CreateMenuElements(MenuPage finalPage)
        {
            menuPage = new("Continue", finalPage);
            ppenMenuButton = new(finalPage, "ItemSync");

            urlInput = new(menuPage, "URL: ");
            urlInput.InputField.characterLimit = 120;
            connectButton = new(menuPage, "Connect");

            nicknameInput = new(menuPage, "Nickname: ");
            nicknameInput.InputField.characterLimit = 30;

            roomInput = new(menuPage, "Room: ");
            roomInput.InputField.characterLimit = 60;
            readyButton = new(menuPage, "Ready");
            readyPlayersBox = new(menuPage, "", MenuLabel.Style.Body);
            readyPlayersCounter = new(menuPage, "Ready Players: ");

            startButton = new(menuPage, "Start ItemSync");
            startButton.AddSetResumeKeyEvent("Randomizer");

            additionalSettingsLabel = new(menuPage, "Additional Settings");
            syncVanillaItemsButton = new(menuPage, "Sync Vanilla Items");

            localPreferencesLabel = new(menuPage, "Local Preferences");
            additionalFeaturesToggleButton = new(menuPage, "Additional Features");

            // Load last values from settings
            urlInput.SetValue(ItemSyncMod.GS.URL);
            nicknameInput.SetValue(ItemSyncMod.GS.UserName);
            syncVanillaItemsButton.SetValue(ItemSyncMod.GS.SyncVanillaItems);
            additionalFeaturesToggleButton.SetValue(ItemSyncMod.GS.AdditionalFeaturesEnabled && !ItemSyncMod.GS.ReducePreload);
        }

        private void AddEvents()
        {
            ppenMenuButton.AddHideAndShowEvent(menuPage);
            connectButton.ValueChanged += ConnectClicked;
            nicknameInput.ValueChanged += UpdateNickname;
            nicknameInput.InputField.onValidateInput += (text, index, c) => c == ',' ? '.' : c; // ICU
            readyButton.OnClick += ReadyClicked;
            startButton.OnClick += InitiateGame;
            ItemSyncMod.Connection.OnReadyConfirm = UpdateReadyPlayersLabel;
            ItemSyncMod.Connection.OnReadyConfirm += (a, b) => EnsureStartButtonShown();
            ItemSyncMod.Connection.OnReadyDeny = ShowReadyDeny;

            syncVanillaItemsButton.OnClick += SyncVanillaItems_OnClick;
            syncVanillaItemsButton.ValueChanged += value => 
                ItemSyncMod.GS.SyncVanillaItems = value;

            additionalFeaturesToggleButton.InterceptChanged += AdditionalFeaturesToggleButton_InterceptChanged;
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
            connectButton.MoveTo(new(0, 250));

            nicknameInput.MoveTo(new(0, 140));
            roomInput.MoveTo(new(0, 60));
            readyButton.MoveTo(new(0, -40));
            readyPlayersBox.MoveTo(new(600, 430));
            readyPlayersCounter.MoveTo(new(600, 470));

            startButton.MoveTo(new(0, -130));

            additionalSettingsLabel.MoveTo(new(-600, 470));
            syncVanillaItemsButton.MoveTo(new(-600, 400));

            localPreferencesLabel.MoveTo(new(-600, -300));
            additionalFeaturesToggleButton.MoveTo(new(-600, -370));

            urlInput.SymSetNeighbor(Neighbor.Down, connectButton);
            nicknameInput.SymSetNeighbor(Neighbor.Down, roomInput);
            roomInput.SymSetNeighbor(Neighbor.Down, readyButton);
            readyButton.SymSetNeighbor(Neighbor.Down, startButton);
            
            syncVanillaItemsButton.SymSetNeighbor(Neighbor.Right, readyButton);
            syncVanillaItemsButton.SymSetNeighbor(Neighbor.Down, additionalFeaturesToggleButton);
            additionalFeaturesToggleButton.SetNeighbor(Neighbor.Right, readyButton);
        }

        private void RevertToInitialState()
        {
            // Set menu objects (in)active
            urlInput.Show();
            connectButton.Show();
            connectButton.SetText("Connect");

            nicknameInput.Hide();
            roomInput.Hide();

            readyButton.Hide();
            readyButton.SetValue(false);
            readyButton.SetText("Ready");

            readyPlayersBox.Hide();
            readyPlayersBox.SetText("");
            readyPlayersCounter.Hide();
            readyPlayersCounter.Set(0);

            startButton.Hide();

            syncVanillaItemsButton.SetValue(true);

            UnlockSettingsButton();

            ItemSyncMod.Connection.Disconnect();
        }

        private bool GetMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            button = ppenMenuButton;
            ItemSyncMod.Controller = new(rc, this);
            ItemSyncMod.Connection.GameStarted = ItemSyncMod.Controller.StartGame;
            return true;
        }

        private void UpdateNickname(string newNickname)
        {
            ItemSyncMod.GS.UserName = newNickname;
        }

        private void ConnectClicked(bool newValue)
        {
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
            }
        }

        private void Connect()
        {
            try
            {
                string url = urlInput.Value;
                ItemSyncMod.Connection.Connect(url);
                ItemSyncMod.GS.URL = url;
                connectButton.SetText("Disconnect");
            }
            catch
            {
                LogHelper.Log("Failed to connect!");
                connectButton.SetValue(false);
                return;
            }

            urlInput.Hide();

            nicknameInput.Show();
            nicknameInput.Unlock();

            roomInput.Show();
            roomInput.Unlock();

            readyButton.Show();
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
            }
            else
            {
                if (ItemSyncMod.Connection.IsConnected()) ItemSyncMod.Connection.Unready();
                startButton.Hide();

                readyButton.SetText("Ready");
                readyPlayersBox.Hide();
                readyPlayersBox.SetText("");
                readyPlayersCounter.Hide();
                readyPlayersCounter.Set(0);

                nicknameInput.Unlock();
                roomInput.Unlock();

                UnlockSettingsButton();
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

        public void SetSyncVanillaItems(bool value)
        {
            if (!syncVanillaItemsButton.Locked)
                syncVanillaItemsButton.SetValue(value);
        }

        public void LockSettingsButtons()
        {
            syncVanillaItemsButton.Lock();

        }

        public void UnlockSettingsButton()
        {
            syncVanillaItemsButton.Unlock();

        }
    }
}
