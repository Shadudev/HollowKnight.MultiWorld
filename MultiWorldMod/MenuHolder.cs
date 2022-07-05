﻿using MenuChanger;
using MenuChanger.MenuElements;
using RandomizerMod.RC;
using MenuChanger.Extensions;
using MultiWorldLib;
using MultiWorldMod.MenuExtensions;
using MultiWorldMod.Randomizer;

namespace MultiWorldMod
{
    internal class MenuHolder
    {
        internal static MenuHolder MenuInstance { get; private set; }

        private MenuPage menuPage;
        private BigButton openMenuButton, startButton, joinGameButton;
        private DynamicToggleButton connectButton, readyButton;
        private EntryField<string> urlInput;
        private LockableEntryField<string> nicknameInput, roomInput;
        private CounterLabel readyPlayersCounter;
        private DynamicLabel readyPlayersBox;
        private Thread connectThread;

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

            readyButton.SetValue(false);
            readyButton.SetText("Ready");
            readyButton.Unlock();
            readyButton.Hide();

            readyPlayersBox.SetText("");
            readyPlayersBox.Hide();
            readyPlayersCounter.Set(0);
            readyPlayersCounter.Hide();

            startButton.Hide();
            joinGameButton.Hide();

            MultiWorldMod.Connection.Disconnect();
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
            }
            catch
            {
                LogHelper.Log("Failed to connect!");
                connectButton.SetValue(false);
                connectButton.SetText("Failed to Connect, Try Again");
                return;
            }

            ThreadSupport.BeginInvoke(() =>
            {
                connectButton.SetText("Disconnect");
                urlInput.Hide();

                nicknameInput.Show();
                nicknameInput.Unlock();

                roomInput.Show();
                roomInput.Unlock();

                readyButton.Show();
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
            }
            else
            {
                if (MultiWorldMod.Connection.IsConnected()) MultiWorldMod.Connection.Unready();
                startButton.Hide();
                
                readyButton.SetText("Ready");
                readyPlayersBox.SetText("");
                readyPlayersBox.Hide();
                readyPlayersCounter.Set(0);
                readyPlayersCounter.Hide();

                nicknameInput.Unlock();
                roomInput.Unlock();
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

            MultiWorldMod.Controller.InitiateGame(RandomizationAlgorithm.Default /* TODO randomizationAlgorithmEntry.Value*/);
        }

        private void StartNewGame() => MultiWorldMod.Controller.StartGame();

        internal void ShowJoinGameButton()
        {
            connectButton.Hide();
            readyButton.Hide();
            startButton.Hide();
            joinGameButton.Show();
        }
    }
}
