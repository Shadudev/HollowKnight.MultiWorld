﻿using RandomizerMod.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MultiWorldMenu = RandomizerMod.MultiWorld.MultiWorldMenu;
using static MultiWorldMod.LogHelper;

namespace MultiWorldMod
{
    internal static class MenuChanger
    {
        private const string ITEM_SYNC_DESCRIPTOR_STRING = "Items & Settings Sync";
        private const string SETTINGS_SYNC_DESCRIPTOR_STRING = "Settings Only Sync";
        
        private static MenuButton startRandoBtn = null, startMultiBtn = null;
        private static int readyChangeCount;

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
            multiWorldMenu.URLInput.text = ItemSync.Instance.MultiWorldSettings.URL;
            multiWorldMenu.NicknameInput.text = ItemSync.Instance.MultiWorldSettings.UserName;
            multiWorldMenu.NicknameInput.onEndEdit.AddListener(ChangeNickname);

            multiWorldMenu.MultiWorldBtn.Changed += item => MultiWorldChanged(multiWorldMenu, item);
            multiWorldMenu.MultiWorldReadyBtn.Changed += item => MultiWorldReadyChanged(multiWorldMenu, item);

            Object.Destroy(multiWorldMenu.StartMultiWorldBtn.GetComponent<StartGameEventTrigger>());
            multiWorldMenu.StartMultiWorldBtn.AddEvent(EventTriggerType.Submit, garbage => InitiateGame());

            multiWorldMenu.RejoinBtn.AddEvent(EventTriggerType.Submit, (data) =>
            {
                ItemSync.Instance.Connection.RejoinGame();
            });

            multiWorldMenu.MultiWorldBtn.SetName(ITEM_SYNC_DESCRIPTOR_STRING);
            ChangeButtonDescription(multiWorldMenu.StartMultiWorldBtn, ITEM_SYNC_DESCRIPTOR_STRING);

            readyChangeCount = 0;
        }

        internal static void StartGame()
        {
            // Patch for rejoining
            bool originalActivity = startMultiBtn.gameObject.activeSelf;
            startMultiBtn.gameObject.SetActive(true);

            if (readyChangeCount % 4 < 2)
            {
                GiveItem.AddMultiWorldItemHandlers();
                ItemSync.Instance.Settings.IsItemSync = true;
            }
            else
            {
                GiveItem.RemoveMultiWorldItemHandlers();
                ItemSync.Instance.Settings.IsItemSync = false;
            }

            startRandoBtn.gameObject.SetActive(true);
            ExecuteEvents.Execute(startRandoBtn.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
            startRandoBtn.gameObject.SetActive(false);
            // Revert
            startMultiBtn.gameObject.SetActive(originalActivity);

        }

        private static void ChangeNickname(string newNickname)
        {
            ItemSync.Instance.MultiWorldSettings.UserName = newNickname;
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
                    ItemSync.Instance.MultiWorldSettings.URL = multiWorldMenu.URLInput.text;
                    Log($"Trying to connect to {ItemSync.Instance.MultiWorldSettings.URL}");
                    ItemSync.Instance.Connection.Connect();
                    ItemSync.Instance.Connection.ReadyConfirmReceived = (int num, string players) => UpdateReadyPlayersLabel(multiWorldMenu, num, players);
                    ItemSync.Instance.SetSettingsSync(true);
                    item.SetSelection("Yes");
                }
                catch
                {
                    Log("Failed to connect!");
                    item.SetSelection("No");
                    ItemSync.Instance.Connection.Disconnect();
                    ItemSync.Instance.SetSettingsSync(false);
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

                startRandoBtn = multiWorldMenu.StartRandoBtn;
                startMultiBtn = multiWorldMenu.StartMultiWorldBtn;
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
            
                ItemSync.Instance.Connection.Disconnect();

                startRandoBtn = null;
                startMultiBtn = null;
            }
        }

        private static void MultiWorldReadyChanged(MultiWorldMenu multiWorldMenu, RandoMenuItem<bool> item)
        {
            readyChangeCount++;
            switch (readyChangeCount % 4)
            {
                case 0:
                    multiWorldMenu.MultiWorldBtn.SetName(ITEM_SYNC_DESCRIPTOR_STRING);
                    ChangeButtonDescription(multiWorldMenu.StartMultiWorldBtn, ITEM_SYNC_DESCRIPTOR_STRING);
                    break;
                case 2:
                    multiWorldMenu.MultiWorldBtn.SetName(SETTINGS_SYNC_DESCRIPTOR_STRING);
                    ChangeButtonDescription(multiWorldMenu.StartMultiWorldBtn, SETTINGS_SYNC_DESCRIPTOR_STRING);
                    break;
            }

            if (item.CurrentSelection)
            {
                ItemSync.Instance.Connection.ReadyUp(multiWorldMenu.RoomInput.text);
                multiWorldMenu.StartMultiWorldBtn.gameObject.SetActive(true);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(false);
                multiWorldMenu.NicknameInput.enabled = false;
                multiWorldMenu.RoomInput.enabled = false;
            }
            else
            {
                ItemSync.Instance.Connection.Unready();
                multiWorldMenu.StartMultiWorldBtn.gameObject.SetActive(false);
                multiWorldMenu.RejoinBtn.gameObject.SetActive(true);
                multiWorldMenu.MultiWorldReadyBtn.SetName("Ready");
                multiWorldMenu.NicknameInput.enabled = true;
                multiWorldMenu.RoomInput.enabled = true;
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
            ItemSync.Instance.InitiateGame();
        }

        private static void ChangeButtonDescription(MenuButton menuButton, string description)
        {
            Transform descTrans = menuButton.transform.Find("DescriptionText");
            Object.Destroy(descTrans.GetComponent<AutoLocalizeTextUI>());
            descTrans.GetComponent<Text>().text = description;
        }
    }
}
