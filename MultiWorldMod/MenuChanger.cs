using RandomizerMod.Extensions;
using UnityEngine.EventSystems;
using MultiWorldMenu = RandomizerMod.MultiWorld.MultiWorldMenu;
using static MultiWorldMod.LogHelper;
using UnityEngine.UI;
using UnityEngine;

namespace MultiWorldMod
{
    internal static class MenuChanger
    {
        static MenuButton startRandoBtn = null;

        public static void AddMultiWorldMenu()
        {
            startRandoBtn = null;
            MultiWorldMenu multiWorldMenu = RandomizerMod.RandomizerMod.Instance.CreateMultiWorldMenu();

            // Set menu objects (in)active
            multiWorldMenu.MultiWorldBtn.Button.gameObject.SetActive(true);
            multiWorldMenu.URLLabel.gameObject.SetActive(true);
            multiWorldMenu.URLInput.gameObject.SetActive(true);
            multiWorldMenu.NicknameLabel.gameObject.SetActive(false);
            multiWorldMenu.NicknameInput.gameObject.SetActive(false);
            multiWorldMenu.RoomLabel.gameObject.SetActive(false);
            multiWorldMenu.RoomInput.gameObject.SetActive(false);
            multiWorldMenu.RejoinBtn.gameObject.SetActive(false);
            multiWorldMenu.MultiWorldReadyBtn.Button.gameObject.SetActive(false);
            multiWorldMenu.StartMultiWorldBtn.gameObject.SetActive(false);

            // Load last values from settings
            multiWorldMenu.URLInput.text = MultiWorldMod.Instance.MultiWorldSettings.URL;
            multiWorldMenu.NicknameInput.text = MultiWorldMod.Instance.MultiWorldSettings.UserName;
            multiWorldMenu.NicknameInput.onEndEdit.AddListener(ChangeNickname);

            multiWorldMenu.MultiWorldBtn.Changed += item => MultiWorldChanged(multiWorldMenu, item);
            multiWorldMenu.MultiWorldReadyBtn.Changed += item => MultiWorldReadyChanged(multiWorldMenu, item);

            Object.Destroy(multiWorldMenu.StartMultiWorldBtn.GetComponent<StartGameEventTrigger>());
            multiWorldMenu.StartMultiWorldBtn.AddEvent(EventTriggerType.Submit, garbage => InitiateGame());

            multiWorldMenu.RejoinBtn.AddEvent(EventTriggerType.Submit, (data) =>
            {
                MultiWorldMod.Instance.Connection.RejoinGame();
            });
        }

        internal static void StartGame()
        {
            startRandoBtn.gameObject.SetActive(true);
            ExecuteEvents.Execute(startRandoBtn.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            startRandoBtn.gameObject.SetActive(false);
        }

        private static void ChangeNickname(string newNickname)
        {
            MultiWorldMod.Instance.MultiWorldSettings.UserName = newNickname;
        }

        private static void MultiWorldChanged(MultiWorldMenu multiWorldMenu, RandoMenuItem<string> item)
        {
            multiWorldMenu.StartMultiWorldBtn.gameObject.SetActive(false);
            multiWorldMenu.MultiWorldReadyBtn.SetSelection(false);
            multiWorldMenu.MultiWorldReadyBtn.SetName("Ready");
            multiWorldMenu.ReadyPlayersLabel.transform.Find("Text").GetComponent<Text>().text = "";
            startRandoBtn = null;

            if (item.CurrentSelection == "Yes")
            {
                try
                {
                    MultiWorldMod.Instance.MultiWorldSettings.URL = multiWorldMenu.URLInput.text;
                    Log($"Trying to connect to {MultiWorldMod.Instance.MultiWorldSettings.URL}");
                    MultiWorldMod.Instance.Connection.Connect();
                    MultiWorldMod.Instance.Connection.ReadyConfirmReceived = (int num, string players) => UpdateReadyPlayersLabel(multiWorldMenu, num, players);
                    item.SetSelection("Yes");
                }
                catch
                {
                    Log("Failed to connect!");
                    item.SetSelection("No");
                    MultiWorldMod.Instance.Connection.Disconnect();
                    MultiWorldMod.Instance.Connection = null;
                    return;
                }

                multiWorldMenu.URLLabel.SetActive(false);
                multiWorldMenu.URLInput.gameObject.SetActive(false);

                multiWorldMenu.NicknameLabel.SetActive(true);
                multiWorldMenu.NicknameInput.gameObject.SetActive(true);
                multiWorldMenu.NicknameInput.enabled = true;


                multiWorldMenu.RoomLabel.SetActive(true);
                multiWorldMenu.RoomInput.gameObject.SetActive(true);
                multiWorldMenu.RoomInput.enabled = true;

                multiWorldMenu.StartRandoBtn.gameObject.SetActive(false);
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

                multiWorldMenu.StartRandoBtn.gameObject.SetActive(true);
                multiWorldMenu.MultiWorldReadyBtn.Button.gameObject.SetActive(false);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(false);
            
                MultiWorldMod.Instance.Connection.Disconnect();
                MultiWorldMod.Instance.Connection = null;
            }
        }

        private static void MultiWorldReadyChanged(MultiWorldMenu multiWorldMenu, RandoMenuItem<bool> item)
        {
            if (item.CurrentSelection)
            {
                MultiWorldMod.Instance.Connection.ReadyUp(multiWorldMenu.RoomInput.text);
                multiWorldMenu.StartMultiWorldBtn.gameObject.SetActive(true);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(false);
                multiWorldMenu.NicknameInput.enabled = false;
                multiWorldMenu.RoomInput.enabled = false;
                startRandoBtn = multiWorldMenu.StartRandoBtn;
            }
            else
            {
                MultiWorldMod.Instance.Connection.Unready();
                multiWorldMenu.StartMultiWorldBtn.gameObject.SetActive(false);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(true);
                multiWorldMenu.MultiWorldReadyBtn.SetName("Ready");
                multiWorldMenu.NicknameInput.enabled = true;
                multiWorldMenu.RoomInput.enabled = true;
                multiWorldMenu.ReadyPlayersLabel.transform.Find("Text").GetComponent<Text>().text = "";
                startRandoBtn = null;
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
            MultiWorldMod.Instance.Connection.InitiateGame();
        }
    }
}
