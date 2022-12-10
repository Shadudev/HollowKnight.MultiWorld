using ItemChanger;
using ItemChanger.Internal.Menu;
using Modding.Menu;
using Modding.Menu.Config;
using MultiWorldMod.Items.Remote.Tags;
using UnityEngine.UI;

namespace MultiWorldMod.Menu
{
    internal class ObtainRemotePlacementsMenu
    {
        private const string MENU_TITLE = "Obtain Remotely Placed Items";
        private const string DESCRIPTION = "Obtain all your items placed in someone else's world";

        private const string PLAYER_TO_OBTAIN_FROM_BUTTON_TITLE = "Player to Obtain From";
        private const string OBTAIN_ITEMS_BUTTON_TITLE = "Click to Obtain";
        private const string CLICK_TO_OBTAIN_TEXT = "Click to obtain items";
        private const string VERIFY_OBTAIN_CLICKED_TEXT = "Click again to confirm obtaining";

        private MenuButton playerNameDisplay = null;
        private MenuButton obtainItemsButton = null;

        public int currentPlayerID = -1;
        public List<string> playerNames = new();

        public void Reset()
        {
            playerNames.Clear();
            currentPlayerID = -1;
            ChangePlayer(0);
        }

        public void LoadNames(List<string> playerNames)
        {
            currentPlayerID = 0;
            this.playerNames.AddRange(playerNames);
            ChangePlayer(1);
        }

        internal void AddThirdPartyEjectMenu(ModMenuScreenBuilder builder, MenuScreen modListMenu)
        {
            MenuBuilder menuBuilder = ModMenuHelper.CreateMenuBuilder(MENU_TITLE,
                builder.menuBuilder.Screen, out _);

            // Based on ItemChanger.Internal.Menu.ModMenuScreenBuilder.AddButton
            MenuButtonConfig playerDisplayConfig = new()
            {
                Label = PLAYER_TO_OBTAIN_FROM_BUTTON_TITLE,
                Description = new DescriptionInfo { Text = "" },
                Proceed = false,
                SubmitAction = (_) => ChangePlayer(1),
                CancelAction = (_) => ChangePlayer(-1)
            };

            MenuButtonConfig ovtainItemsConfig = new()
            {
                Label = OBTAIN_ITEMS_BUTTON_TITLE,
                Description = new DescriptionInfo { Text = "" },
                Proceed = false,
                SubmitAction = ObtainItemsClicked,
                CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu)
            };

            menuBuilder.AddContent(RegularGridLayout.CreateVerticalLayout(105f),
                c =>
                {
                    c.AddMenuButton(PLAYER_TO_OBTAIN_FROM_BUTTON_TITLE, playerDisplayConfig, out playerNameDisplay);
                    c.AddMenuButton(OBTAIN_ITEMS_BUTTON_TITLE, ovtainItemsConfig, out obtainItemsButton);
                }
            );
            
            builder.AddSubpage(MENU_TITLE, DESCRIPTION, menuBuilder.Build());
        }

        private void ChangePlayer(int offset)
        {
            string playerName = "";

            if (playerNames.Count > 1)
            {
                currentPlayerID = CalculateID(offset);
                if (currentPlayerID == MultiWorldMod.MWS.PlayerId)
                    currentPlayerID = CalculateID(offset);

                playerName = playerNames[currentPlayerID];
            }

            GetDescriptionText(playerNameDisplay).text = playerName;
            GetDescriptionText(obtainItemsButton).text = CLICK_TO_OBTAIN_TEXT;
        }

        private int CalculateID(int offset)
        {
            return (currentPlayerID + offset + playerNames.Count) % playerNames.Count;
        }

        private void ObtainItemsClicked(MenuButton button)
        {
            if (playerNames.Count == 0) { }

            string currentText = GetDescriptionText(button).text;
            string newDescription = null;
            switch (currentText)
            {
                case CLICK_TO_OBTAIN_TEXT:
                    newDescription = VERIFY_OBTAIN_CLICKED_TEXT;
                    break;
                case VERIFY_OBTAIN_CLICKED_TEXT:
                    foreach (AbstractPlacement placement in ItemChanger.Internal.Ref.Settings.GetPlacements())
                    {
                        if (placement.GetTag(out RemotePlacementTag tag) && tag.LocationOwnerID == currentPlayerID)
                        {
                            placement.GiveAll(new GiveInfo()
                            {
                                Container = "MultiWorld",
                                FlingType = FlingType.DirectDeposit,
                                MessageType = MessageType.Corner,
                                Transform = null,
                                Callback = null
                            });
                        }
                    }
                    break;
            }

            if (newDescription != null)
            {
                GetDescriptionText(button).text = newDescription;
            }
        }

        private Text GetDescriptionText(MenuButton button)
        {
            return button.transform.Find("Description").GetComponent<Text>();
        }
    }
}
