namespace MultiWorld
{
    internal static class MenuChanger
    {
        public static void AddMultiWorldMenu()
        {
            /// Using exported MenuPage object - MW Menu Logic
            /// toggle button
            /// url textbox
            /// player name textbox
            /// room name textbox
            /// ready button
            /// ready players label

            /// Initiator Flow
            /// Once StartGame is Pressed
            /// Send Server a MWInitiateRequest
            /// Proceed into the MWGetRandoMessage Handler Flow
            /// 
            /// MWGetRandoMessage Handler Flow
            /// Create a RandoResult and Send To Server
            /// Proceed into the MWStartGameMessage
            /// 
            /// Listen for MWStartGameMessage
            /// StartGame with attached (modified by server) RandoResult

            /// Optional: Server Sends RandomizingGame Message
            /// Hide startGame Button - has to have a timeout to prevent deadlocks in lobby
            /// Also undo-able by unreadying/toggling off MW/backing off menu

            /*
            RandoMenuItem<string> multiworldBtn = new RandoMenuItem<string>(back, new Vector2(0, 480), "Multiworld", "No", "Yes");
            RandoMenuItem<bool> multiworldReadyBtn = new RandoMenuItem<bool>(back, new Vector2(0, 300), "Ready", false, true);
            multiworldReadyBtn.Button.gameObject.SetActive(false);

            InputField createInputObject()
            {
                GameObject gameObject = back.Clone("entry", MenuButton.MenuButtonType.Activate, new Vector2(0, 1130)).gameObject;
                Object.DestroyImmediate(gameObject.GetComponent<MenuButton>());
                Object.DestroyImmediate(gameObject.GetComponent<EventTrigger>());
                Object.DestroyImmediate(gameObject.transform.Find("Text").GetComponent<AutoLocalizeTextUI>());
                Object.DestroyImmediate(gameObject.transform.Find("Text").GetComponent<FixVerticalAlign>());
                Object.DestroyImmediate(gameObject.transform.Find("Text").GetComponent<ContentSizeFitter>());

                RectTransform rect = gameObject.transform.Find("Text").GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(337, 63.2f);

                InputField input = gameObject.AddComponent<InputField>();
                input.textComponent = gameObject.transform.Find("Text").GetComponent<Text>();
                input.colors = new ColorBlock
                {
                    highlightedColor = Color.yellow,
                    pressedColor = Color.red,
                    disabledColor = Color.black,
                    normalColor = Color.white,
                    colorMultiplier = 2f
                };

                return input;
            }
            
            InputField urlInput = createInputObject();
            urlInput.transform.localPosition = new Vector3(50, 415);
            // TODO urlInput.text = RandomizerMod.Instance.MWSettings.URL;
            urlInput.text = "127.0.0.1";
            urlInput.textComponent.fontSize = urlInput.textComponent.fontSize - 5;

            urlInput.caretColor = Color.white;
            urlInput.contentType = InputField.ContentType.Standard;
            urlInput.navigation = Navigation.defaultNavigation;
            urlInput.caretWidth = 8;
            urlInput.characterLimit = 0;

            InputField nicknameInput = createInputObject();
            nicknameInput.transform.localPosition = new Vector3(0, 415);
            // TODO nicknameInput.text = RandomizerMod.Instance.MWSettings.UserName;
            nicknameInput.text = "whoAmI";
            nicknameInput.textComponent.fontSize = nicknameInput.textComponent.fontSize - 5;

            nicknameInput.caretColor = Color.white;
            nicknameInput.contentType = InputField.ContentType.Standard;
            // TODO nicknameInput.onEndEdit.AddListener(ChangeNickname);
            nicknameInput.navigation = Navigation.defaultNavigation;
            nicknameInput.caretWidth = 8;
            nicknameInput.characterLimit = 15;

            nicknameInput.gameObject.SetActive(false);

            InputField roomInput = createInputObject();
            roomInput.transform.localPosition = new Vector3(0, 370);
            roomInput.text = "";
            roomInput.textComponent.fontSize = roomInput.textComponent.fontSize - 5;

            roomInput.caretColor = Color.white;
            roomInput.contentType = InputField.ContentType.Standard;
            roomInput.navigation = Navigation.defaultNavigation;
            roomInput.caretWidth = 8;
            roomInput.characterLimit = 15;

            roomInput.gameObject.SetActive(false);

            private static GameObject CreateLabel(MenuButton baseObj, Vector2 position, string text)
            {
                GameObject label = baseObj.Clone(text + "Label", MenuButton.MenuButtonType.Activate, position, text)
                    .gameObject;
                Object.Destroy(label.GetComponent<EventTrigger>());
                Object.Destroy(label.GetComponent<MenuButton>());
                return label;
            }

            GameObject urlLabel = CreateLabel(back, new Vector2(-155, 420), "URL:");
            urlLabel.transform.localScale = new Vector3(0.8f, 0.8f);
            GameObject nicknameLabel = CreateLabel(back, new Vector2(-300, 420), "Nickname:");
            nicknameLabel.transform.localScale = new Vector3(0.8f, 0.8f);
            nicknameLabel.SetActive(false);
            GameObject roomLabel = CreateLabel(back, new Vector2(-300, 375), "Room:");
            roomLabel.transform.localScale = new Vector3(0.8f, 0.8f);
            roomLabel.SetActive(false);
            GameObject readyPlayers = CreateLabel(back, new Vector2(-0, 540), "");
            readyPlayers.transform.localScale = new Vector3(0.5f, 0.5f);

            MenuButton rejoinBtn = back.Clone("Rejoin", MenuButton.MenuButtonType.Proceed, new Vector2(0, 230), "Rejoin");
            rejoinBtn.ClearEvents();
            rejoinBtn.AddEvent(EventTriggerType.Submit, (data) =>
            {
                // RandomizerMod.Instance.mwConnection.RejoinGame();
            });
            rejoinBtn.gameObject.SetActive(false);
            */
        }
    }
}
