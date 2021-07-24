using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MultiWorldMod
{
    class EjectMenuHandler
    {
        private static GameObject _gameObject = null;
        
        internal static void Initialize()
        {
            if (_gameObject != null)
            {
                LogHelper.LogWarn("Double initializing eject menu handler");
                Object.Destroy(_gameObject);
            }

            _gameObject = CreateNewButton();

            On.UIManager.GoToPauseMenu += OnPause;
            On.UIManager.UIClosePauseMenu += OnUnpause;
            On.UIManager.ReturnToMainMenu += Deinitialize;

        }

        private static GameObject CreateNewButton()
        {
            GameObject gameObject = new GameObject();

            // Make sure that our UI is an overlay on the screen
            gameObject.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            // Also scale the UI with the screen size
            var canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            Object.DontDestroyOnLoad(gameObject);

            MenuButton button = gameObject.AddComponent<MenuButton>();
            Object.DontDestroyOnLoad(button);
            button.transform.SetParent(gameObject.transform, false);

            button.transform.position = new Vector2(0, 400);
            Text text = button.gameObject.AddComponent<Text>();
            text.text = "Eject From MultiWorld";
            text.fontSize = 36;
            RandomizerMod.Extensions.MenuButtonExtensions.AddEvent(button, EventTriggerType.Submit, Eject);

            return gameObject;
        }

        private static void Eject(BaseEventData arg)
        {
            // Future plan - send a collection of items rather than single messages per
            // TODO iterate itemplacements, send all unfound remote items
            // Has to go through GiveItem to prevent a client from sending the same items multiple times

        }

        private static IEnumerator OnPause(On.UIManager.orig_GoToPauseMenu orig, UIManager self)
        {
            yield return orig(self);
            _gameObject.SetActive(true);
        }

        private static void OnUnpause(On.UIManager.orig_UIClosePauseMenu orig, UIManager self)
        {
            orig(self);
            _gameObject.SetActive(false);
        }

        private static IEnumerator Deinitialize(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
        {
            yield return orig(self);
            On.UIManager.GoToPauseMenu -= OnPause;
            On.UIManager.UIClosePauseMenu -= OnUnpause;
            Object.Destroy(_gameObject);
            _gameObject = null;
        }
    }
}
