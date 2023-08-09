using MenuChanger;
using MenuChanger.MenuElements;
using MultiWorldLib.ExportedAPI;

namespace ItemSyncMod.Menu
{
    /// <summary>
    /// This inheritence is required to export the API implemented and shared between the mods
    /// </summary>
    public class ExtensionsMenuAPI : ExportedExtensionsMenuAPI
    {
        internal static List<BaseButton> ConstructExtensionsMenus(MenuPage landingPage)
        {
            List<BaseButton> extensionButtons = new();
            foreach (var extensionMenuCtor in extensionMenusCtors)
            {
                var button = extensionMenuCtor.Invoke(landingPage);
                if (button != null) extensionButtons.Add(button);
            }

            return extensionButtons;
        }

        internal static void InvokeOnMenuReverted() => InvokeOnMenuRevertedInternal();
        internal static void InvokeOnConnected() => InvokeOnConnectedInternal();
        internal static void InvokeOnDisconnected() => InvokeOnDisconnectedInternal();
        internal static void InvokeOnReady() => InvokeOnReadyInternal();
        internal static void InvokeOnAddReadyMetadata(Dictionary<string, string> metadata) => InvokeOnAddReadyMetadataInternal(metadata);
        internal static void InvokeOnUnready() => InvokeOnUnreadyInternal();
        internal static void InvokeOnLockSettings() => InvokeOnLockSettingsInternal();
        internal static void InvokeOnGameStarted() => InvokeOnGameStartedInternal();
        internal static void InvokeOnGameJoined() => InvokeOnGameJoinedInternal();
        internal static void InvokeRoomStateUpdated(int playersCount, string[] playersNames) =>
            InvokeRoomStateUpdatedInternal(playersCount, playersNames);

        internal static void ResetMenuEvents() => ResetMenuEventsInternal();
    }
}
