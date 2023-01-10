using MenuChanger;
using MenuChanger.MenuElements;
using System.Collections.ObjectModel;

namespace MultiWorldLib.ExportedAPI
{
    public abstract class ExportedExtensionsMenuAPI
    {
        public delegate BaseButton OnExtensionMenuConstructionHandler(MenuPage landingPage);
        protected static HashSet<OnExtensionMenuConstructionHandler> extensionMenusCtors = new();
        
        /// <summary>
        /// Register your extension's menu constructor.
        /// </summary>
        /// <param name="ctorHandler"></param>
        public static void AddExtensionsMenu(OnExtensionMenuConstructionHandler ctorHandler)
        {
            extensionMenusCtors.Add(ctorHandler);
        }

        public static void RemoveExtensionsMenu(OnExtensionMenuConstructionHandler ctorHandler)
        {
            extensionMenusCtors.Remove(ctorHandler);
        }

        /// <summary>
        /// A class to encapsulate all menu state events.
        /// These events are to be registered to on every menu construction call.
        /// </summary>
        public static class MenuStateEvents
        {
            /// <summary>
            /// Invoked when the menu elements are set to their initial state (e.g. visibility).
            /// </summary>
            public static event MenuReverted OnMenuRevert;
            public delegate void MenuReverted();
            internal static void InvokeOnMenuReverted() => OnMenuRevert?.Invoke();

            /// <summary>
            /// Connected to the server.
            /// </summary>
            public static event Connected OnConnected;
            public delegate void Connected();
            internal static void InvokeOnConnected() => OnConnected?.Invoke();

            /// <summary>
            /// Disconnected from the server.
            /// </summary>
            public static event Disconnected OnDisconnected;
            public delegate void Disconnected();
            internal static void InvokeOnDisconnected() => OnDisconnected?.Invoke();

            /// <summary>
            /// Player clicked the ready button.
            /// </summary>
            public static event Ready OnReady;
            public delegate void Ready();
            internal static void InvokeOnReady() => OnReady?.Invoke();

            /// <summary>
            /// Player clicked the unready button, or was unreadied due to the server denying its ready request.
            /// </summary>
            public static event Unready OnUnready;
            public delegate void Unready(); 
            internal static void InvokeOnUnready() => OnUnready?.Invoke();

            /// <summary>
            /// Occurs when other players ready or un-ready from the room.
            /// </summary>
            public static event RoomStateUpdated OnRoomStateUpdated;
            public readonly record struct RoomState(int ReadyPlayersCount, ReadOnlyCollection<string> ReadyPlayersNames);
            public delegate void RoomStateUpdated(RoomState newState);
            internal static void InvokeOnRoomStateUpdated(int playersCount, string[] playersNames) => 
                OnRoomStateUpdated?.Invoke(new(playersCount, Array.AsReadOnly(playersNames)));

            /// <summary>
            /// Occurs once the game starts and players can join.
            /// </summary>
            public static event GameStarted OnGameStarted;
            public delegate void GameStarted();
            internal static void InvokeOnGameStarted() => OnGameStarted?.Invoke();

            /// <summary>
            /// Occurs when the client joins the game.
            /// </summary>
            public static event GameJoined OnGameJoined;
            public delegate void GameJoined();
            internal static void InvokeOnGameJoined() => OnGameJoined?.Invoke();

            /// <summary>
            /// Invoked once settings should be locked from any changes.
            /// Practically, it is called once a player pressed "Start Game".
            /// </summary>
            public static event LockSettings OnLockSettings;
            public delegate void LockSettings();
            internal static void InvokeOnLockSettings() => OnLockSettings?.Invoke();

            internal static void Reset()
            {
                OnMenuRevert = null;
                OnConnected = null;
                OnDisconnected = null;
                OnReady = null;
                OnUnready = null;
                OnRoomStateUpdated = null;
                OnLockSettings = null;
                OnGameStarted = null;
                OnGameJoined = null;
            }
        }

        // These are a workaround to allow inheriting classes to invoke the events in MenuStateEvents
        protected static void InvokeOnMenuRevertedInternal() => MenuStateEvents.InvokeOnMenuReverted();
        protected static void InvokeOnConnectedInternal() => MenuStateEvents.InvokeOnConnected();
        protected static void InvokeOnDisconnectedInternal() => MenuStateEvents.InvokeOnDisconnected();
        protected static void InvokeOnReadyInternal() => MenuStateEvents.InvokeOnReady();
        protected static void InvokeOnUnreadyInternal() => MenuStateEvents.InvokeOnUnready();
        protected static void InvokeOnLockSettingsInternal() => MenuStateEvents.InvokeOnLockSettings();
        protected static void InvokeOnGameStartedInternal() => MenuStateEvents.InvokeOnGameStarted();
        protected static void InvokeOnGameJoinedInternal() => MenuStateEvents.InvokeOnGameJoined();

        protected static void InvokeRoomStateUpdatedInternal(int playersCount, string[] playersNames) =>
            MenuStateEvents.InvokeOnRoomStateUpdated(playersCount, playersNames);

        protected static void ResetMenuEventsInternal() => MenuStateEvents.Reset();
    }
}
