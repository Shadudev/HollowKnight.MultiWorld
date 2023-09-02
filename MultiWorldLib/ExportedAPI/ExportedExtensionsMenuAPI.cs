using MenuChanger;
using MenuChanger.MenuElements;
using System.Collections.ObjectModel;

namespace MultiWorldLib.ExportedAPI
{
    public abstract class ExportedExtensionsMenuAPI
    {
        /// <summary>
        /// Function singature for constructing an extension menu.
        /// </summary>
        /// <param name="landingPage">The parent page</param>
        /// <param name="events">Events to register to</param>
        /// <returns></returns>
        public delegate BaseButton OnExtensionMenuConstructionHandler(MenuPage landingPage, MenuStateEvents events);
        protected HashSet<OnExtensionMenuConstructionHandler> extensionMenusCtors = new();
        protected MenuStateEvents eventsManager = new();
        
        /// <summary>
        /// Register your extension's menu constructor.
        /// </summary>
        /// <param name="ctorHandler"></param>
        public void AddExtensionsMenu(OnExtensionMenuConstructionHandler ctorHandler)
        {
            extensionMenusCtors.Add(ctorHandler);
        }

        public void RemoveExtensionsMenu(OnExtensionMenuConstructionHandler ctorHandler)
        {
            extensionMenusCtors.Remove(ctorHandler);
        }

        /// <summary>
        /// A class to encapsulate all menu state events.
        /// These events are to be registered to on every menu construction call.
        /// </summary>
        public class MenuStateEvents
        {
            /// <summary>
            /// Invoked when the menu elements are set to their initial state (e.g. visibility).
            /// </summary>
            public event MenuReverted OnMenuRevert;
            public delegate void MenuReverted();

            /// <summary>
            /// Connected to the server.
            /// </summary>
            public event Connected OnConnected;
            public delegate void Connected();

            /// <summary>
            /// Disconnected from the server.
            /// </summary>
            public event Disconnected OnDisconnected;
            public delegate void Disconnected();

            /// <summary>
            /// Player clicked the ready button.
            /// </summary>
            public event Ready OnReady;
            public delegate void Ready();

            /// <summary>
            /// Player clicked the unready button, or was unreadied due to the server denying its ready request.
            /// </summary>
            public event Unready OnUnready;
            public delegate void Unready();

            /// <summary>
            /// Occurs when other players ready or un-ready from the room.
            /// </summary>
            public event RoomStateUpdated OnRoomStateUpdated;
            public readonly record struct RoomState(int ReadyPlayersCount, ReadOnlyCollection<string> ReadyPlayersNames);
            public delegate void RoomStateUpdated(RoomState newState);

            /// <summary>
            /// Occurs once the game starts and players can join.
            /// </summary>
            public event GameStarted OnGameStarted;
            public delegate void GameStarted();

            /// <summary>
            /// Occurs when the client joins the game.
            /// </summary>
            public event GameJoined OnGameJoined;
            public delegate void GameJoined();

            /// <summary>
            /// Invoked once settings should be locked from any changes.
            /// Practically, it is called once a player pressed "Start Game".
            /// </summary>
            public event LockSettings OnLockSettings;
            public delegate void LockSettings();
        }
    }
}
