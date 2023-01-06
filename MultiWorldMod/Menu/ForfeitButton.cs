using ItemChanger;
using ItemChanger.Internal.Menu;
using Modding.Menu;
using Modding.Menu.Config;
using MultiWorldMod.Items;
using MultiWorldMod.Items.Remote;
using System.Collections;
using UnityEngine.UI;

namespace MultiWorldMod.Menu
{
    class ForfeitButton
    {
        private static readonly string FORFEIT_PROMPT_TEXT = "Forfeit From MultiWorld";
        private static readonly string FORFEIT_INITIAL_DESC = "Send everyone else's items from your world";
        private static readonly string FORFEIT_SECOND_DESC = "Press again to forfeit";
        private static readonly string FORFEIT_FAILED = "Forfeit Failed, Try Again";

        private static MenuButton s_forfeitButton = null;
        private static int s_forfeitedItemsCount = -1;

        internal static void AddForfeitButton(ModMenuScreenBuilder builder, MenuScreen modListMenu)
        {
            // Based on ItemChanger.Internal.Menu.ModMenuScreenBuilder.AddButton
            MenuButtonConfig config = new()
            {
                Label = FORFEIT_PROMPT_TEXT,
                Description = new DescriptionInfo
                {
                    Text = FORFEIT_INITIAL_DESC
                },
                Proceed = false,
                SubmitAction = (_) => ForfeitClicked(),
                CancelAction = delegate
                {
                    UIManager.instance.UIGoToDynamicMenu(modListMenu);
                }
            };
            
            builder.buildActions.Add(delegate (ContentArea c)
            {
                c.AddMenuButton(FORFEIT_PROMPT_TEXT, config, out s_forfeitButton);
            });
        }

        internal static void Initialize()
        {
            On.UIManager.UIGoToPauseMenu += OnPause;
            On.UIManager.ReturnToMainMenu += OnReturnToMainMenu;

            s_forfeitedItemsCount = -1;
        }

        private static void OnPause(On.UIManager.orig_UIGoToPauseMenu orig, UIManager self)
        {
            orig(self);
            if (s_forfeitedItemsCount == -1)
                SetButtonDesc(FORFEIT_PROMPT_TEXT);
        }

        private static IEnumerator OnReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
        {
            yield return orig(self);
            s_forfeitedItemsCount = -1;
            SetButtonDesc(FORFEIT_PROMPT_TEXT);
        }

        private static void ForfeitClicked()
        {
            if (GetButtonDescriptionComponent(s_forfeitButton).text == FORFEIT_PROMPT_TEXT || 
                GetButtonDescriptionComponent(s_forfeitButton).text == FORFEIT_FAILED)
            {
                SetButtonDesc(FORFEIT_SECOND_DESC);
                return;
            }

            LogHelper.Log("Forfeiting others' items");
            SetButtonDesc("Forfeiting, Please Wait");

            List<(string, int)> itemsToSend = new();
            Dictionary<AbstractItem, AbstractPlacement> remoteItemsPlacements = ItemManager.GetRemoteItemsPlacements();
            foreach (RemoteItem item in remoteItemsPlacements.Keys)
            {
                if (item.CanBeGiven())
                    item.CollectForForfeiting(remoteItemsPlacements[item], itemsToSend);
            }

            s_forfeitedItemsCount = itemsToSend.Count;
            MultiWorldMod.Connection.SendItems(itemsToSend);
        }

        internal static void Enable()
        {
            s_forfeitButton.gameObject.SetActive(true);
        }

        internal static void Disable()
        {
            s_forfeitButton.gameObject.SetActive(false);
            s_forfeitedItemsCount = -1;
        }

        internal static void UpdateButton(int itemsCount)
        {
            // There was no prior forfeit attempt
            if (s_forfeitedItemsCount == -1) return;

            if (itemsCount == s_forfeitedItemsCount)
            {
                SetButtonDesc("Forfeited Successfully");
            }
            else
            {
                SetButtonDesc(FORFEIT_FAILED);
                s_forfeitedItemsCount = -1;
            }
        }

        private static void SetButtonDesc(string text)
        {
            GetButtonDescriptionComponent(s_forfeitButton).text = text;
        }

        private static Text GetButtonDescriptionComponent(MenuButton button)
        {
            return button.transform.Find("Description").GetComponent<Text>();
        }
    }
}
