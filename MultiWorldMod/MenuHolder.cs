using MenuChanger;
using MenuChanger.MenuElements;
using RandomizerMod.RC;
using MenuChanger.Extensions;
using MultiWorldMod.MenuExtensions;
using MultiWorldMod.Randomizer;
using MenuChanger.MenuPanels;

namespace MultiWorldMod
{
    public class MenuHolder
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
        private DynamicToggleButton connectButton, readyButton;
        private EntryField<string> urlInput;
        private LockableEntryField<string> nicknameInput, roomInput;
        private CounterLabel readyPlayersCounter;
        private DynamicLabel readyPlayersBox;
        private Thread connectThread;

        private MenuLabel additionalSettingsLabel;

        #region Split Groups
        private MenuPage splitGroupsPage;
        SmallButton jumpToSplitGroups;
        MultiGridItemPanel splitGroupsPanel;
        MenuLabel splitGroupsDescLabel;
        #endregion

        internal static void ConstructMenu(MenuPage connectionsPage)
        {
            MenuInstance ??= new();
            MenuInstance.OnMenuConstruction(connectionsPage);
        }

        internal static bool GetMultiWorldMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button)
            => MenuInstance.GetMenuButton(rc, landingPage, out button);

        internal void ShowStartGameFailure()
        {
            ThreadSupport.BeginInvoke(() =>
            {
                connectButton.Unlock();
                connectButton.Show();

                readyPlayersBox.SetText("Failed to start game.\nPlease check ModLog.txt for more info.");
            });
        }

        private void OnMenuConstruction(MenuPage finalPage)
        {
            CreateMenuElements(finalPage);
            AddEvents();
            Arrange();

            CreateAdditionalMenus();

            OnMenuConstructed?.Invoke();
            RevertToInitialState();
        }

        private void CreateMenuElements(MenuPage finalPage)
        {
            menuPage = new("Continue", finalPage);
            openMenuButton = new(finalPage, "MultiWorld");

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

            additionalSettingsLabel = new(menuPage, "Additional Settings");

            startButton = new(menuPage, "Start MultiWorld");

            joinGameButton = new(menuPage, "Join Game");
            joinGameButton.AddSetResumeKeyEvent("Randomizer");

            // Load last values from settings
            urlInput.SetValue(MultiWorldMod.GS.URL);
            nicknameInput.SetValue(MultiWorldMod.GS.UserName);
        }

        private void AddEvents()
        {
            openMenuButton.AddHideAndShowEvent(menuPage);
            connectButton.OnClick += () => ThreadSupport.BeginInvoke(ConnectClicked);
            nicknameInput.ValueChanged += UpdateNickname;
            nicknameInput.InputField.onValidateInput += (text, index, c) => c == ',' ? '.' : c; // ICU
            readyButton.OnClick += () => ThreadSupport.BeginInvoke(ReadyClicked);
            startButton.OnClick += () => ThreadSupport.BeginInvoke(InitiateGame);
            MultiWorldMod.Connection.OnReadyConfirm = (num, players) => ThreadSupport.BeginInvoke(() => UpdateReadyPlayersLabel(num, players));
            MultiWorldMod.Connection.OnReadyConfirm += (_, _) => ThreadSupport.BeginInvoke(EnsureStartButtonShown);
            MultiWorldMod.Connection.OnReadyDeny = (msg) => ThreadSupport.BeginInvoke(() => ShowReadyDeny(msg));

            menuPage.backButton.OnClick += () => ThreadSupport.BeginInvoke(RevertToInitialState);
            joinGameButton.OnClick += () => ThreadSupport.BeginInvoke(StartNewGame);
            joinGameButton.OnClick += () => OnGameJoined?.Invoke();
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

            additionalSettingsLabel.MoveTo(new(-600, 470));

            startButton.MoveTo(new(0, -130));

            urlInput.SymSetNeighbor(Neighbor.Down, connectButton);
            nicknameInput.SymSetNeighbor(Neighbor.Down, roomInput);
            roomInput.SymSetNeighbor(Neighbor.Down, readyButton);
            readyButton.SymSetNeighbor(Neighbor.Down, startButton);
        }

        private void RevertToInitialState()
        {
            // Set menu objects (in)active
            urlInput.Show();
            connectButton.Show();
            connectButton.SetValue(false);
            connectButton.SetText("Connect");
            readyButton.Unlock();

            nicknameInput.Hide();
            roomInput.Hide();

            readyButton.Hide();
            readyButton.SetValue(false);
            readyButton.SetText("Ready");
            readyButton.Unlock();

            readyPlayersBox.Hide();
            readyPlayersBox.SetText("");
            readyPlayersCounter.Hide();
            readyPlayersCounter.Set(0);

            startButton.Hide();
            joinGameButton.Hide();

            MultiWorldMod.Connection.Disconnect();

            OnMenuRevert?.Invoke();
            OnDisconnected?.Invoke();
        }

        private void CreateAdditionalMenus()
        {
            InitializeSplitGroupsMenu();
        }

        private void InitializeSplitGroupsMenu()
        {
            splitGroupsPage = new("Include Split Groups In MultiWorld", menuPage);
            splitGroupsPanel = new(splitGroupsPage, 8, 3, 60f, 650f, new(0, 300), Array.Empty<SmallButton>());

            jumpToSplitGroups = new(menuPage, "Split Groups");
            jumpToSplitGroups.OnClick += ShowSplitGroups;
            OnReady += jumpToSplitGroups.Lock;
            OnUnready += jumpToSplitGroups.Unlock;
            jumpToSplitGroups.AddHideAndShowEvent(menuPage, splitGroupsPage);
            jumpToSplitGroups.MoveTo(new(-600, 400));

            splitGroupsDescLabel = new(splitGroupsPage, "Included groups names must match with other players" +
                " to have an effect", MenuLabel.Style.Title);
            splitGroupsDescLabel.MoveTo(new(0, 400));
        }

        private void ShowSplitGroups()
        {
            if (splitGroupsPanel.Items.Count == 0)
                BuildSplitGroupsPanel();
        }

        private void BuildSplitGroupsPanel()
        {
            HashSet<string> groupLabels = new();
            foreach (var stage in MultiWorldMod.Controller.randoController.randomizer.stages)
                foreach (var group in stage.groups)
                    groupLabels.Add(group.Label);

            ToggleButton button = new(splitGroupsPage, RBConsts.MainItemGroup);
            button.SetValue(true);
            splitGroupsPanel.Add(button);

            groupLabels.Remove(RBConsts.MainItemGroup); // Slight efficiency improvement
            foreach (string groupLabel in groupLabels)
            {
                button = new(splitGroupsPage, groupLabel);
                button.SetValue(false);
                button.ValueChanged += val =>
                {
                    if (val) MultiWorldMod.Controller.IncludedGroupsLabels.Add(groupLabel);
                    else MultiWorldMod.Controller.IncludedGroupsLabels.Remove(groupLabel);
                };

                splitGroupsPanel.Add(button);
            }
        }

        private bool GetMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            button = openMenuButton;
            MultiWorldMod.Controller = new MultiWorldController(rc, this);
            MultiWorldMod.Connection.GameStarted = () => ThreadSupport.BeginInvoke(ShowJoinGameButton);
            return true;
        }

        private void UpdateNickname(string newNickname)
        {
            MultiWorldMod.GS.UserName = newNickname;
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
                ThreadSupport.BeginInvoke(RevertToInitialState);
            }
        }

        private void Connect()
        {
            try
            {
                string url = urlInput.Value;
                MultiWorldMod.Connection.Connect(url);
                LogHelper.Log($"Connected to `{url}` successfully");
                MultiWorldMod.GS.URL = url;
                connectButton.SetText("Disconnect");
            }
            catch
            {
                LogHelper.Log("Failed to connect!");
                connectButton.SetValue(false);
                OnDisconnected?.Invoke();
                return;
            }

            ThreadSupport.BeginInvoke(() =>
            {
                urlInput.Hide();

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
                MultiWorldMod.Connection.ReadyUp(roomInput.Value);
                readyPlayersBox.Show();
                readyPlayersCounter.Show();
                OnReady?.Invoke();
            }
            else
            {
                if (MultiWorldMod.Connection.IsConnected()) MultiWorldMod.Connection.Unready();
                readyButton.SetText("Ready");
                startButton.Hide();
                readyPlayersBox.Hide();
                readyPlayersBox.SetText("");
                readyPlayersCounter.Hide();
                readyPlayersCounter.Set(0);

                nicknameInput.Unlock();
                roomInput.Unlock();
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
            MultiWorldMod.Controller.InitiateGame();
        }

        private void StartNewGame() => MultiWorldMod.Controller.StartGame();

        internal void ShowJoinGameButton()
        {
            readyButton.Hide();
            startButton.Hide();
            connectButton.Hide();
            joinGameButton.Show();
        }
    }
}
