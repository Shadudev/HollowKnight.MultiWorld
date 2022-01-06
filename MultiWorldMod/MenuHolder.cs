using MenuChanger;
using UnityEngine.SceneManagement;
using MenuChanger.MenuElements;
using RandomizerMod.RC;
using MenuChanger.Extensions;
using MultiWorldMod.MenuExtensions;

namespace MultiWorldMod
{
    internal class MenuHolder
    {
        internal static MenuHolder MenuInstance { get; private set; }

        private MenuPage MenuPage;
        private BigButton OpenMenuButton, StartButton;
        private DynamicToggleButton ConnectButton, ReadyButton;
        private EntryField<string> URLInput;
        private LockableEntryField<string> NicknameInput, RoomInput;
        private DynamicLabel ReadyPlayersBox;
        private Thread ConnectThread;

        internal static void ConstructMenu(MenuPage connectionsPage)
        {
            MenuInstance ??= new();
            MenuInstance.OnMenuConstruction(connectionsPage);
        }

        internal static void OnMainMenu(Scene from, Scene to)
        {
            if (from.name == "Menu_Title") MenuInstance = null;
        }

        internal static bool GetMultiWorldMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button) 
            => MenuInstance.GetMenuButton(rc, landingPage, out button);

        internal void ShowGameStartFailure()
        {
            ReadyButton.Unlock();
            ReadyPlayersBox.SetText("Failed to start game.\nPlease check ModLog.txt for more info.");
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
            MenuPage = new("Continue", finalPage);
            OpenMenuButton = new(finalPage, "MultiWorld");

            URLInput = new(MenuPage, "URL: ");
            URLInput.InputField.characterLimit = 120;
            ConnectButton = new(MenuPage, "Connect");

            NicknameInput = new(MenuPage, "Nickname: ");
            NicknameInput.InputField.characterLimit = 30;

            RoomInput = new(MenuPage, "Room: ");
            RoomInput.InputField.characterLimit = 60;
            ReadyButton = new(MenuPage, "Ready");
            ReadyPlayersBox = new(MenuPage, "", MenuLabel.Style.Body);

            StartButton = new(MenuPage, "Start MultiWorld");

            // Load last values from settings
            URLInput.SetValue(MultiWorldMod.GS.URL);
            NicknameInput.SetValue(MultiWorldMod.GS.UserName);
        }

        private void AddEvents()
        {
            OpenMenuButton.AddHideAndShowEvent(MenuPage);
            ConnectButton.ValueChanged += ConnectClicked;
            NicknameInput.ValueChanged += UpdateNickname;
            NicknameInput.InputField.onValidateInput += (text, index, c) => c == ',' ? '.' : c; // ICU
            ReadyButton.OnClick += ReadyClicked;
            StartButton.OnClick += InitiateGame;
            StartButton.OnClick += MultiWorldMod.Controller.InitiateGame;
            MultiWorldMod.Connection.OnReadyConfirm = UpdateReadyPlayersLabel;
            MultiWorldMod.Connection.OnReadyConfirm += (a, b) => EnsureStartButtonShown();
            MultiWorldMod.Connection.OnReadyDeny = ShowReadyDeny;
        }

        private void Arrange()
        {
            URLInput.MoveTo(new(0, 300));
            ConnectButton.MoveTo(new(0, 250));

            NicknameInput.MoveTo(new(0, 140));
            RoomInput.MoveTo(new(0, 60));
            ReadyButton.MoveTo(new(0, -40));
            ReadyPlayersBox.MoveTo(new(600, 500));

            StartButton.MoveTo(new(0, -130));
        }

        private void RevertToInitialState()
        {
            // Set menu objects (in)active
            URLInput.Show();
            ConnectButton.Show();
            ConnectButton.SetText("Connect");

            NicknameInput.Hide();
            RoomInput.Hide();

            ReadyButton.Hide();
            ReadyButton.SetValue(false);

            ReadyPlayersBox.Hide();
            ReadyPlayersBox.SetText("");
            
            StartButton.Hide();

            MultiWorldMod.Connection.Disconnect();
        }

        private bool GetMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            button = OpenMenuButton;
            MultiWorldMod.Controller = new MultiWorldController(rc, this);
            return true;
        }

        private void UpdateNickname(string newNickname)
        {
            MultiWorldMod.GS.UserName = newNickname;
        }

        private void ConnectClicked(bool newValue)
        {
            if (newValue)
            {
                if (ConnectThread is not null && ConnectThread.IsAlive) ConnectThread.Abort();
                ConnectButton.SetText("Connecting");
                ConnectThread = new Thread(Connect);
                ConnectThread.Start();
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
                string url = URLInput.Value;
                MultiWorldMod.Connection.Connect(url);
                LogHelper.Log($"Connected to `{url}` successfully");
                MultiWorldMod.GS.URL = url;
                ConnectButton.SetText("Disconnect");
            }
            catch
            {
                LogHelper.Log("Failed to connect!");
                ConnectButton.SetValue(false);
                return;
            }

            URLInput.Hide();

            NicknameInput.Show();
            NicknameInput.Unlock();

            RoomInput.Show();
            RoomInput.Unlock();

            ReadyButton.Show();
        }

        private void ReadyClicked()
        {
            if (ReadyButton.Value)
            {
                NicknameInput.Lock();
                RoomInput.Lock();
                MultiWorldMod.Connection.ReadyUp(RoomInput.Value);
                ReadyPlayersBox.Show();
            }
            else
            {
                if (MultiWorldMod.Connection.IsConnected()) MultiWorldMod.Connection.Unready();
                StartButton.Hide();
                ReadyPlayersBox.Hide();
                ReadyPlayersBox.SetText("");
                NicknameInput.Unlock();
                RoomInput.Unlock();
            }
        }

        private void UpdateReadyPlayersLabel(int num, string players)
        {
            ReadyPlayersBox.SetText(players);
        }

        private void EnsureStartButtonShown()
        {
            if (StartButton.Hidden)
            {
                StartButton.Show();
                ReadyButton.SetText("Unready");
            }
        }

        private void ShowReadyDeny(string description)
        {
            ReadyButton.SetText("Ready");   
            ReadyButton.SetValue(false);
            ReadyPlayersBox.SetText(description);
            NicknameInput.Unlock();
            RoomInput.Unlock();

            RoomInput.InputField.Select();
        }

        private void InitiateGame()
        {
            ReadyButton.Lock();
            StartButton.Hide();
        }
    }
}
