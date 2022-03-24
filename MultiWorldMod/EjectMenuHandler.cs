using ItemChanger;
using MultiWorldLib;
using MultiWorldMod.Items;
using MultiWorldMod.Items.Remote.Tags;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MultiWorldMod
{
    class EjectMenuHandler
    {
        private static readonly string EJECT_PROMPT_TEXT = "Eject From MultiWorld";
        private static readonly string EJECT_SECOND_PROMPT_TEXT = "Press again to eject";

        private static PauseMenuButton ejectButton = null;
        private static int ejectedItemsCount = -1;
        internal static void Initialize()
        {
            if (ejectButton != null)
            {
                LogHelper.LogWarn("Re-initializing eject menu button");
                UnityEngine.Object.Destroy(ejectButton);
            }
            ejectButton = CreateNewButton();

            On.UIManager.UIGoToPauseMenu += OnPause;
            On.UIManager.UIClosePauseMenu += OnUnpause;
            On.UIManager.ReturnToMainMenu += Deinitialize;
        }

        private static void UIManager_UIGoToPauseMenu()
        {
            throw new NotImplementedException();
        }

        private static PauseMenuButton CreateNewButton()
        {
            MenuScreen pauseScreen = UIManager.instance.pauseMenuScreen;
            PauseMenuButton exitButton = (PauseMenuButton)pauseScreen.defaultHighlight.FindSelectableOnUp();

            PauseMenuButton ejectButton = UnityEngine.Object.Instantiate(exitButton.gameObject).GetComponent<PauseMenuButton>();
            ejectButton.name = "EjectButton";
            ejectButton.pauseButtonType = (PauseMenuButton.PauseButtonType)3;

            ejectButton.transform.SetParent(exitButton.transform.parent);
            ejectButton.transform.localScale = exitButton.transform.localScale;

            ejectButton.transform.localPosition = new Vector2(
                exitButton.transform.localPosition.x, exitButton.transform.localPosition.y - 250);

            Transform textTransform = ejectButton.transform.Find("Text");
            UnityEngine.Object.Destroy(textTransform.GetComponent<AutoLocalizeTextUI>());

            SetButtonText(ejectButton, EJECT_PROMPT_TEXT);

            EventTrigger eventTrigger = ejectButton.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = ejectButton.gameObject.AddComponent<EventTrigger>();
            else
                eventTrigger.triggers.Clear();

            EventTrigger.Entry submitEntry = new() { eventID = EventTriggerType.Submit };
            submitEntry.callback.AddListener(Eject);
            EventTrigger.Entry pointerClickEntry = new() { eventID = EventTriggerType.PointerClick };
            pointerClickEntry.callback.AddListener(Eject);

            eventTrigger.triggers.AddRange(new EventTrigger.Entry[] { submitEntry, pointerClickEntry });
            UnityEngine.Object.DontDestroyOnLoad(ejectButton.gameObject);

            return ejectButton;
        }

        private static void Eject(BaseEventData arg)
        {
            if (GetButtonTextComponent(ejectButton).text.StartsWith(EJECT_PROMPT_TEXT))
            {
                SetButtonText(ejectButton, EJECT_SECOND_PROMPT_TEXT);
                return;
            }

            LogHelper.Log("Ejecting from MultiWorld");
            SetButtonText(ejectButton, "Ejecting, Please Wait");

            List<(int, string)> itemsToSend = new();
            Dictionary<AbstractItem, AbstractPlacement> remoteItemsPlacements = ItemManager.GetRemoteItemsPlacements();
            foreach (AbstractItem item in remoteItemsPlacements.Keys)
            {
                RemoteItemTag tag = item.GetTag<RemoteItemTag>();
                if (tag.CanBeGiven())
                    tag.CollectForEjection(remoteItemsPlacements[item], itemsToSend);
            }

            ejectedItemsCount = itemsToSend.Count;
            MultiWorldMod.Connection.SendItems(itemsToSend);
        }

        private static void OnPause(On.UIManager.orig_UIGoToPauseMenu orig, UIManager self)
        {
            orig(self);
            ejectButton.gameObject.SetActive(true);
        }

        private static void OnUnpause(On.UIManager.orig_UIClosePauseMenu orig, UIManager self)
        {
            orig(self);
            SetButtonText(ejectButton, EJECT_PROMPT_TEXT);
            ejectButton.gameObject.SetActive(false);
        }

        private static IEnumerator Deinitialize(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
        {
            yield return orig(self);
            On.UIManager.UIGoToPauseMenu -= OnPause;
            On.UIManager.UIClosePauseMenu -= OnUnpause;
            On.UIManager.ReturnToMainMenu -= Deinitialize;
            UnityEngine.Object.Destroy(ejectButton);
            ejectButton = null;
        }

        internal static void UpdateButton(int itemsCount)
        {
            // There was no eject attempt
            if (ejectedItemsCount == -1) return;

            if (itemsCount == ejectedItemsCount)
            {
                SetButtonText(ejectButton, "Ejected Successfully");
                ejectedItemsCount = -1;
            }
            else
            {
                SetButtonText(ejectButton, "Eject Failed, Try Again");
            }
        }

        private static void SetButtonText(PauseMenuButton ejectButton, string text)
        {
            GetButtonTextComponent(ejectButton).text = text;
        }

        private static Text GetButtonTextComponent(PauseMenuButton ejectButton)
        {
            return ejectButton.transform.Find("Text").GetComponent<Text>();
        }
    }
}
