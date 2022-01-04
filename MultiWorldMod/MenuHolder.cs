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
        private ToggleButton ConnectButton, ReadyButton;
        private SettableFormatter ConnectButtonFormatter;
        private EntryField<string> URLInput;
        private LockableEntryField<string> NicknameInput, RoomInput;
        private DynamicLabel ReadyPlayersBox;
        private Thread ConnectThread;
        // private SmallButton rejoinButton;

        internal static void ConstructMenu(MenuPage connectionsPage)
        {
            MenuInstance ??= new();
            MenuInstance.OnMenuConstruction(connectionsPage);
        }

        internal static void OnMainMenu(Scene from, Scene to)
        {
            if (from.name == "Menu_Title") MenuInstance = null;
        }

        internal static bool GetMultiWorldMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button) =>
            MenuInstance.GetMenuButton(rc, landingPage, out button);

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
            URLInput.InputField.characterLimit = 50;
            ConnectButton = new(MenuPage, "Connect");
            ConnectButtonFormatter = new SettableFormatter("Connect");
            ConnectButton.Formatter = ConnectButtonFormatter;

            NicknameInput = new(MenuPage, "Nickname: ");
            RoomInput = new(MenuPage, "Room: ");
            ReadyButton = new(MenuPage, "Ready");
            ReadyButton.Formatter = new SimpleToggleButtonFormatter("Unready", "Ready");
            ReadyPlayersBox = new(MenuPage, "", MenuLabel.Style.Body);

            StartButton = new(MenuPage, "Start MultiWorld");
            // TODO RejoinButton = new(MenuPage, "Rejoin");

            // Load last values from settings
            URLInput.SetValue(MultiWorldMod.GS.URL);
            NicknameInput.SetValue(MultiWorldMod.GS.UserName);
        }

        private void AddEvents()
        {
            OpenMenuButton.AddHideAndShowEvent(MenuPage);
            ConnectButton.InterceptChanged += ConnectClicked;
            NicknameInput.ValueChanged += UpdateNickname;
            ReadyButton.ValueChanged += ReadyClicked;
            StartButton.OnClick += InitiateGame;
            // RejoinButton.OnClick += MultiWorldMod.Connection.RejoinGame();
            MultiWorldMod.Connection.OnReadyConfirm = UpdateReadyPlayersLabel;
            MultiWorldMod.Connection.OnReadyDeny = ShowReadyDeny;
        }

        private void Arrange()
        {
            URLInput.MoveTo(new(0, 300));
            ConnectButton.MoveTo(new(0, 250));

            NicknameInput.MoveTo(new(0, 140));
            RoomInput.MoveTo(new(0, 60));
            ReadyButton.MoveTo(new(0, -40));
            ReadyPlayersBox.MoveTo(new(400, -300));

            StartButton.MoveTo(new(0, -130));
        }

        private void RevertToInitialState()
        {
            // Set menu objects (in)active
            URLInput.Show();
            ConnectButton.Show();
            ConnectButtonFormatter.Text = "Connect";

            NicknameInput.Hide();
            RoomInput.Hide();

            ReadyButton.Hide();
            ReadyButton.SetValue(false);

            ReadyPlayersBox.Hide();
            ReadyPlayersBox.SetText("");
            
            StartButton.Hide();
            // RejoinButton.Hide();

            MultiWorldMod.Connection.Disconnect();
        }

        private bool GetMenuButton(RandoController rc, MenuPage landingPage, out BaseButton button)
        {
            button = OpenMenuButton;
            return true;
        }

        private static void UpdateNickname(string newNickname)
        {
            MultiWorldMod.GS.UserName = newNickname;
        }

        private void ConnectClicked(MenuItem self, ref object newValue, ref bool cancelChange)
        {
            // Do not interfere if someone stops this change already
            if (cancelChange) return;

            if (self != ConnectButton) LogHelper.LogError("WTF, who added this callback?");

            if ((bool) newValue)
            {
                if (ConnectButtonFormatter.Text != "Disconnect")
                {
                    if (ConnectThread is not null && ConnectThread.IsAlive) ConnectThread.Abort();
                    ConnectButtonFormatter.Text = "Connecting";
                    ConnectThread = new Thread(Connect);
                    ConnectThread.Start();
                }
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
                MultiWorldMod.Connection.Connect(URLInput.Value);
                MultiWorldMod.GS.URL = URLInput.Value;
                ConnectButtonFormatter.Text = "Disconnect";
                ConnectButton.SetValue(true); // Basically to call RefreshText
            }
            catch
            {
                LogHelper.Log("Failed to connect!");
                ConnectButtonFormatter.Text = "Failed to connect, try again";
                ConnectButton.SetValue(false);
                return;
            }

            URLInput.Hide();

            NicknameInput.Show();
            NicknameInput.Unlock();

            RoomInput.Show();
            RoomInput.Unlock();

            ReadyButton.Show();
            StartButton.Hide();
            // RejoinButton.Show();
        }

        private void ReadyClicked(bool isReady)
        {
            if (isReady)
            {
                MultiWorldMod.Connection.ReadyUp(RoomInput.Value);
                NicknameInput.Lock();
                RoomInput.Lock();
                // RejoinButton.Hide();
            }
            else
            {
                if (MultiWorldMod.Connection.IsConnected()) MultiWorldMod.Connection.Unready();
                StartButton.Hide();
                ReadyPlayersBox.Hide();
                ReadyPlayersBox.SetText("");
                NicknameInput.Unlock();
                RoomInput.Unlock();
                // RejoinButton.Show();
            }
        }

        private void UpdateReadyPlayersLabel(int num, string players)
        {
            if (ReadyButton.Value)
            {
                ReadyPlayersBox.SetText(players);
                StartButton.Show();
            }
        }

        private void ShowReadyDeny(string description)
        {
            ReadyButton.SetValue(false);
            ReadyPlayersBox.SetText(description);
            // TODO Format text nicely within ReadyPlayersBox (characters per line, element placement)
        }

        private static void InitiateGame()
        {
            MultiWorldMod.Connection.InitiateGame();
        }
    }
}
