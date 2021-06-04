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
            multiWorldMenu.URLInput.text = MultiWorld.Instance.Settings.URL;
            multiWorldMenu.NicknameInput.text = MultiWorld.Instance.Settings.UserName;
            multiWorldMenu.NicknameInput.onEndEdit.AddListener(ChangeNickname);

            // toggle button event
            multiWorldMenu.MultiWorldBtn.Changed += item => MWChanged(multiWorldMenu, item);
            multiWorldMenu.MultiWorldReadyBtn.Changed += item => MWReadyChanged(multiWorldMenu, item);

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
                // MultiWorld.Instance.mwConnection.RejoinGame();
            });
        }

        private static void ChangeNickname(string newNickname)
        {
            MultiWorld.Instance.Settings.UserName = newNickname;
        }

        static void MWChanged(MultiWorldMenu multiWorldMenu, RandoMenuItem<string> item)
        {
            if (item.CurrentSelection == "Yes")
            {
                try
                {
                    MultiWorld.Instance.Settings.URL = multiWorldMenu.URLInput.text;
                    Log($"Trying to connect to {MultiWorld.Instance.Settings.URL}");
                    /*MultiWorld.Instance.mwConnection.Disconnect();
                    MultiWorld.Instance.mwConnection = new MultiWorld.ClientConnection();
                    MultiWorld.Instance.mwConnection.Connect();
                    MultiWorld.Instance.mwConnection.ReadyConfirmReceived = UpdateReady;*/
                    item.SetSelection("Yes");
                }
                catch
                {
                    Log("Failed to connect!");
                    item.SetSelection("No");
                    return;
                }

                multiWorldMenu.URLLabel.SetActive(false);
                multiWorldMenu.URLInput.gameObject.SetActive(false);

                multiWorldMenu.NicknameLabel.SetActive(true);
                multiWorldMenu.NicknameInput.gameObject.SetActive(true);

                multiWorldMenu.RoomLabel.SetActive(true);
                multiWorldMenu.RoomInput.gameObject.SetActive(true);

                multiWorldMenu.MultiWorldReadyBtn.Button.gameObject.SetActive(true);
                multiWorldMenu.MultiWorldReadyBtn.SetSelection(false);
                multiWorldMenu.MultiWorldReadyBtn.SetName("Ready");

                multiWorldMenu.ReadyPlayersLabel.transform.Find("Text").GetComponent<Text>().text = "";

                RandomizerMod.RandomizerMod.Instance.SetStartGameButtonVisibility(false);
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

                multiWorldMenu.MultiWorldReadyBtn.SetSelection(false);
                multiWorldMenu.MultiWorldReadyBtn.Button.gameObject.SetActive(false);
                multiWorldMenu.MultiWorldReadyBtn.SetName("Ready");

                multiWorldMenu.ReadyPlayersLabel.transform.Find("Text").GetComponent<Text>().text = "";

                RandomizerMod.RandomizerMod.Instance.SetStartGameButtonVisibility(true);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(false);

                multiWorldMenu.RejoinBtn.gameObject.SetActive(false);
                // MultiWorld.Instance.mwConnection.Disconnect();
            }
        }

        static void MWReadyChanged(MultiWorldMenu multiWorldMenu, RandoMenuItem<bool> item)
        {
            if (item.CurrentSelection)
            {
                // Multiworld.Instance.mwConnection.ReadyUp(roomInput.text);
                RandomizerMod.RandomizerMod.Instance.SetStartGameButtonVisibility(true);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(false);
            }
            else
            {
                // RandomizerMod.Instance.mwConnection.Unready();
                RandomizerMod.RandomizerMod.Instance.SetStartGameButtonVisibility(false);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(true);
                multiWorldMenu.MultiWorldReadyBtn.SetName("Ready");
                multiWorldMenu.ReadyPlayersLabel.transform.Find("Text").GetComponent<Text>().text = "";
            }
        }
    }
}
