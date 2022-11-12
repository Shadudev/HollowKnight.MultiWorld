using ItemChanger;
using ItemChanger.Internal.Menu;
using Modding.Menu;
using Modding.Menu.Config;
using MultiWorldMod.Items;
using MultiWorldMod.Items.Remote;
using System.Collections;
using UnityEngine.UI;

namespace MultiWorldMod
{
    class EjectMenuHandler
    {
        private static readonly string EJECT_PROMPT_TEXT = "Eject From MultiWorld";
        private static readonly string EJECT_SECOND_PROMPT_TEXT = "Press again to eject";
        private static readonly string EJECT_FAILED = "Eject Failed, Try Again";

        private static MenuButton s_ejectButton = null;
        private static int s_ejectedItemsCount = -1;

        internal static void AddEjectButton(ModMenuScreenBuilder builder, MenuScreen modListMenu)
        {
            // Based on ItemChanger.Internal.Menu.ModMenuScreenBuilder.AddButton
            MenuButtonConfig config = new()
            {
                Label = EJECT_PROMPT_TEXT,
                Description = new DescriptionInfo
                {
                    Text = "Send everyone else's items from your world"
                },
                Proceed = false,
                SubmitAction = delegate
                {
                    EjectClicked();
                },
                CancelAction = delegate
                {
                    UIManager.instance.UIGoToDynamicMenu(modListMenu);
                }
            };
            
            builder.buildActions.Add(delegate (ContentArea c)
            {
                c.AddMenuButton(EJECT_PROMPT_TEXT, config, out s_ejectButton);
            });
        }

        internal static void Initialize()
        {
            On.UIManager.UIGoToPauseMenu += OnPause;
            On.UIManager.ReturnToMainMenu += OnReturnToMainMenu;

            s_ejectedItemsCount = -1;
        }

        private static void OnPause(On.UIManager.orig_UIGoToPauseMenu orig, UIManager self)
        {
            orig(self);
            if (s_ejectedItemsCount == -1)
                SetButtonText(EJECT_PROMPT_TEXT);
        }

        private static IEnumerator OnReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
        {
            yield return orig(self);
            s_ejectedItemsCount = -1;
            SetButtonText(EJECT_PROMPT_TEXT);
        }

        private static void EjectClicked()
        {
            if (GetButtonTextComponent(s_ejectButton).text == EJECT_PROMPT_TEXT || 
                GetButtonTextComponent(s_ejectButton).text == EJECT_FAILED)
            {
                SetButtonText(EJECT_SECOND_PROMPT_TEXT);
                return;
            }

            LogHelper.Log("Ejecting from MultiWorld");
            SetButtonText("Ejecting, Please Wait");

            List<(string, int)> itemsToSend = new();
            Dictionary<AbstractItem, AbstractPlacement> remoteItemsPlacements = ItemManager.GetRemoteItemsPlacements();
            foreach (RemoteItem item in remoteItemsPlacements.Keys)
            {
                if (item.CanBeGiven())
                    item.CollectForEjection(remoteItemsPlacements[item], itemsToSend);
            }

            s_ejectedItemsCount = itemsToSend.Count;
            MultiWorldMod.Connection.SendItems(itemsToSend);
        }

        internal static void Enable()
        {
            s_ejectButton.gameObject.SetActive(true);
        }

        internal static void Disable()
        {
            s_ejectButton.gameObject.SetActive(false);
            s_ejectedItemsCount = -1;
        }

        internal static void UpdateButton(int itemsCount)
        {
            // There was no eject attempt
            if (s_ejectedItemsCount == -1) return;

            if (itemsCount == s_ejectedItemsCount)
            {
                SetButtonText("Ejected Successfully");
            }
            else
            {
                SetButtonText(EJECT_FAILED);
                s_ejectedItemsCount = -1;
            }
        }

        private static void SetButtonText(string text)
        {
            GetButtonTextComponent(s_ejectButton).text = text;
        }

        private static Text GetButtonTextComponent(MenuButton ejectButton)
        {
            return ejectButton.transform.Find("Text").GetComponent<Text>();
        }
    }
}
