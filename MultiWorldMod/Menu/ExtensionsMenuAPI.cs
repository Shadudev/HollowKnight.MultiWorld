using MenuChanger;
using MenuChanger.MenuElements;
using MultiWorldLib.ExportedAPI;

namespace MultiWorldMod.Menu
{
    /// <summary>
    /// This inheritence is required to export the API implemented and shared between the mods
    /// </summary>
    internal class ExtensionsMenuAPI : ExportedExtensionsMenuAPI
    {
        internal List<BaseButton> ConstructExtensionsMenus(MenuPage landingPage)
        {
            List<BaseButton> extensionButtons = new();
            foreach (var extensionMenuCtor in extensionMenusCtors)
                extensionButtons.Add(extensionMenuCtor.Invoke(landingPage));

            return extensionButtons;
        }

        internal void InvokeOnMenuReverted() => InvokeOnMenuRevertedInternal();
        internal void InvokeOnConnected() => InvokeOnConnectedInternal();
        internal void InvokeOnDisconnected() => InvokeOnDisconnectedInternal();
        internal void InvokeOnReady() => InvokeOnReadyInternal();
        internal void InvokeOnUnready() => InvokeOnUnreadyInternal();
        internal void InvokeOnLockSettings() => InvokeOnLockSettingsInternal();
        internal void InvokeOnGameStarted() => InvokeOnGameStartedInternal();
        internal void InvokeOnGameJoined() => InvokeOnGameJoinedInternal();

        internal void InvokeRoomStateUpdated(int playersCount, string[] playersNames) =>
            InvokeRoomStateUpdatedInternal(playersCount, playersNames);

        internal void ResetMenuEvents() => ResetMenuEventsInternal();
    }
}
