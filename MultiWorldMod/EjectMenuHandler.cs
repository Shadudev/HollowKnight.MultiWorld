using MultiWorldLib;
using SereCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MultiWorldMod
{
    class EjectMenuHandler
    {
        private static PauseMenuButton ejectButton = null;
        private static int ejectedItemsCount = -1;
        
        internal static void Initialize()
        {
            if (ejectButton != null)
            {
                LogHelper.LogWarn("Double initializing eject menu handler");
                UnityEngine.Object.Destroy(ejectButton);
            }
            ejectButton = CreateNewButton();

            On.UIManager.GoToPauseMenu += OnPause;
            On.UIManager.UIClosePauseMenu += OnUnpause;
            On.UIManager.ReturnToMainMenu += Deinitialize;
        }

        private static PauseMenuButton CreateNewButton()
        {
            MenuScreen pauseScreen = Ref.UI.pauseMenuScreen;
            PauseMenuButton exitButton = (PauseMenuButton) pauseScreen.defaultHighlight.FindSelectableOnUp();
            
            PauseMenuButton ejectButton = UnityEngine.Object.Instantiate(exitButton.gameObject).GetComponent<PauseMenuButton>();
            ejectButton.name = "EjectButton";
            ejectButton.pauseButtonType = (PauseMenuButton.PauseButtonType) 3;

            ejectButton.transform.SetParent(exitButton.transform.parent);
            ejectButton.transform.localScale = exitButton.transform.localScale;

            ejectButton.transform.localPosition = new Vector2(
                exitButton.transform.localPosition.x, exitButton.transform.localPosition.y - 250);

            Transform textTransform = ejectButton.transform.Find("Text");
            UnityEngine.Object.Destroy(textTransform.GetComponent<AutoLocalizeTextUI>());

            SetButtonText(ejectButton, "Eject From MultiWorld");

            EventTrigger eventTrigger = ejectButton.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = ejectButton.gameObject.AddComponent<EventTrigger>();
            else
                eventTrigger.triggers.Clear();

            EventTrigger.Entry submitEntry = new EventTrigger.Entry { eventID = EventTriggerType.Submit };
            submitEntry.callback.AddListener(Eject);
            EventTrigger.Entry pointerClickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            pointerClickEntry.callback.AddListener(Eject);

            eventTrigger.triggers.AddRange(new EventTrigger.Entry[] { submitEntry, pointerClickEntry });

            return ejectButton;
        }

        // Future plan - send a collection of items in a single message rather than item per message
        private static void Eject(BaseEventData arg)
        {
            SetButtonText(ejectButton, "Ejecting, Please Wait");

            List<(int, string, string)> itemsToSend = new List<(int, string, string)>();
            foreach ((string item, string location) in GetUncheckedItemPlacements())
            {
                (int playerId, string itemName) = LanguageStringManager.ExtractPlayerID(item);
                if (playerId < 0) continue;
                LogHelper.Log("Eject: Sending item " + itemName + " to " + playerId);
                itemsToSend.Add((playerId, itemName, location));
            }

            ejectedItemsCount = itemsToSend.Count;
            MultiWorldMod.Instance.Connection.SendItems(itemsToSend);
        }

        private static (string, string)[] GetUncheckedItemPlacements()
        {
            return Array.FindAll(RandomizerMod.RandomizerMod.Instance.Settings.ItemPlacements,
                ilpair => !RandomizerMod.RandomizerMod.Instance.Settings.CheckLocationFound(ilpair.Item2));
        }

        private static IEnumerator OnPause(On.UIManager.orig_GoToPauseMenu orig, UIManager self)
        {
            yield return orig(self);
            ejectButton.gameObject.SetActive(true);
        }

        private static void OnUnpause(On.UIManager.orig_UIClosePauseMenu orig, UIManager self)
        {
            orig(self);
            ejectButton.gameObject.SetActive(false);
        }

        private static IEnumerator Deinitialize(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
        {
            yield return orig(self);
            On.UIManager.GoToPauseMenu -= OnPause;
            On.UIManager.UIClosePauseMenu -= OnUnpause;
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
            ejectButton.transform.Find("Text").GetComponent<Text>().text = text;
        }
    }
}
