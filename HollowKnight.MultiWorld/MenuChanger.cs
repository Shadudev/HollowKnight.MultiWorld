using RandomizerMod.Extensions;
using UnityEngine.EventSystems;
using MultiWorldMenu = RandomizerMod.MultiWorld.MultiWorldMenu;
using static MultiWorld.LogHelper;
using UnityEngine.UI;
using System;

namespace MultiWorld
{
    internal static class MenuChanger
    {
        public static void AddMultiWorldMenu()
        {
            MultiWorldMenu multiWorldMenu = RandomizerMod.RandomizerMod.Instance.CreateMultiWorldMenu();

            // Load last values from settings
            multiWorldMenu.URLInput.text = MultiWorld.Instance.MultiWorldSettings.URL;
            multiWorldMenu.NicknameInput.text = MultiWorld.Instance.MultiWorldSettings.UserName;
            multiWorldMenu.NicknameInput.onEndEdit.AddListener(ChangeNickname);

            // toggle button event
            multiWorldMenu.MultiWorldBtn.Changed += item => MultiWorldChanged(multiWorldMenu, item);
            multiWorldMenu.MultiWorldReadyBtn.Changed += item => MultiWorldReadyChanged(multiWorldMenu, item);

            multiWorldMenu.StartMultiWorldBtn.AddEvent(EventTriggerType.Submit, garbage => InitiateGame());
            
            // Initiator Flow
            // Once StartGame is Pressed
            // Send Server a MWInitiateRequest
            // Proceed into the MWGetRandoMessage Handler Flow

            // MWGetRandoMessage Handler Flow
            // Create a RandoResult and Send To Server
            // Proceed into the MWStartGameMessage

            // Listen for MWStartGameMessage
            // StartGame with attached (modified by server) RandoResult

            // Optional: Server Sends RandomizingGame Message
            // Hide startGame Button - has to have a timeout to prevent deadlocks in lobby
            // Also undo-able by unreadying/toggling off MW/backing off menu

            multiWorldMenu.RejoinBtn.AddEvent(EventTriggerType.Submit, (data) =>
            {
                MultiWorld.Instance.Connection.RejoinGame();
            });
        }

        private static void ChangeNickname(string newNickname)
        {
            MultiWorld.Instance.MultiWorldSettings.UserName = newNickname;
        }

        private static void MultiWorldChanged(MultiWorldMenu multiWorldMenu, RandoMenuItem<string> item)
        {
            multiWorldMenu.StartMultiWorldBtn.gameObject.SetActive(false);
            multiWorldMenu.MultiWorldReadyBtn.SetSelection(false);
            multiWorldMenu.MultiWorldReadyBtn.SetName("Ready");
            multiWorldMenu.ReadyPlayersLabel.transform.Find("Text").GetComponent<Text>().text = "";

            if (item.CurrentSelection == "Yes")
            {
                try
                {
                    MultiWorld.Instance.MultiWorldSettings.URL = multiWorldMenu.URLInput.text;
                    Log($"Trying to connect to {MultiWorld.Instance.MultiWorldSettings.URL}");
                    MultiWorld.Instance.Connection = new ClientConnection();
                    MultiWorld.Instance.Connection.Connect();
                    MultiWorld.Instance.Connection.ReadyConfirmReceived = (int num, string players) => UpdateReadyPlayersLabel(multiWorldMenu, num, players);
                    item.SetSelection("Yes");
                }
                catch
                {
                    Log("Failed to connect!");
                    item.SetSelection("No");
                    MultiWorld.Instance.Connection = null;
                    return;
                }

                multiWorldMenu.URLLabel.SetActive(false);
                multiWorldMenu.URLInput.gameObject.SetActive(false);

                multiWorldMenu.NicknameLabel.SetActive(true);
                multiWorldMenu.NicknameInput.gameObject.SetActive(true);

                multiWorldMenu.RoomLabel.SetActive(true);
                multiWorldMenu.RoomInput.gameObject.SetActive(true);

                RandomizerMod.RandomizerMod.Instance.SetStartGameButtonVisibility(false);
                multiWorldMenu.MultiWorldReadyBtn.Button.gameObject.SetActive(true);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(true);
            }
            else
            {
                multiWorldMenu.URLLabel.SetActive(true);
                multiWorldMenu.URLInput.gameObject.SetActive(true);

                multiWorldMenu.NicknameLabel.SetActive(false);
                multiWorldMenu.NicknameInput.gameObject.SetActive(false);

                multiWorldMenu.RoomLabel.SetActive(false);
                multiWorldMenu.RoomInput.gameObject.SetActive(false);

                RandomizerMod.RandomizerMod.Instance.SetStartGameButtonVisibility(true);
                multiWorldMenu.MultiWorldReadyBtn.Button.gameObject.SetActive(false);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(false);
                MultiWorld.Instance.Connection = null;
            }
        }

        private static void MultiWorldReadyChanged(MultiWorldMenu multiWorldMenu, RandoMenuItem<bool> item)
        {
            if (item.CurrentSelection)
            {
                MultiWorld.Instance.Connection.ReadyUp(multiWorldMenu.RoomInput.text);
                multiWorldMenu.StartMultiWorldBtn.gameObject.SetActive(true);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(false);
            }
            else
            {
                MultiWorld.Instance.Connection.Unready();
                multiWorldMenu.StartMultiWorldBtn.gameObject.SetActive(false);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(true);
                multiWorldMenu.MultiWorldReadyBtn.SetName("Ready");
                multiWorldMenu.ReadyPlayersLabel.transform.Find("Text").GetComponent<Text>().text = "";
            }
        }

        private static void UpdateReadyPlayersLabel(MultiWorldMenu multiWorldMenu, int num, string players)
        {
            if (multiWorldMenu.MultiWorldReadyBtn.CurrentSelection)
            {
                multiWorldMenu.MultiWorldReadyBtn.SetName($"Ready ({num})");
                multiWorldMenu.ReadyPlayersLabel.transform.Find("Text").GetComponent<Text>().text = players;

                if (num == -1)
                {
                    multiWorldMenu.StartMultiWorldBtn.gameObject.SetActive(false);
                }
            }
        }
        private static void InitiateGame()
        {
            MultiWorld.Instance.Connection.InitiateGame();
        }
    }
}
