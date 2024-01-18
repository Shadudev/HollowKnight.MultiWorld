using MultiWorldLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiWorldServer.Game
{
    /// <summary>
    /// Ressembling the memory paging scheme, inactive game sessions are stored in files to reduce memory usage.
    /// Additional feature - wipe files that are a month old.
    /// </summary>
    internal class GameSessionPager
    {
        private static readonly Dictionary<Mode, char> modeToChar = new Dictionary<Mode, char>
            {
                {Mode.ItemSync, 'I' },
                {Mode.MultiWorld, 'M' }
            };
        private static readonly Dictionary<char, Mode> charToMode = modeToChar.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        private static readonly string FILE_EXTENSION = ".json";
        internal delegate GameSession PopGameSession(int randoId);

        private readonly GameSessionFactory gameSessionFactory;
        private readonly PopGameSession popGameSessionCallback;
        private readonly Dictionary<int, DateTime> pagePendingGameSessions;
        private readonly TimeSpan pagePendingTimeCap;
        private readonly Dictionary<Tuple<int, Mode>, string> pagedGameSessionsFiles;

        public GameSessionPager(Config config, GameSessionFactory gameSessionFactory, PopGameSession callback)
        {
            if (!Directory.Exists(config.PagedGamesDirectory))
                Directory.CreateDirectory(config.PagedGamesDirectory);

            pagedGameSessionsFiles = new DirectoryInfo(config.PagedGamesDirectory).GetFiles().ToDictionary(
                fileInfo => ParsePageId(fileInfo.Name), fileInfo => fileInfo.FullName);
            pagePendingGameSessions = new Dictionary<int, DateTime>();

            this.gameSessionFactory = gameSessionFactory;
            this.popGameSessionCallback = callback;
            pagePendingTimeCap = config.InactiveGameSessionPagingTime;
        }

        private Tuple<int, Mode> ParsePageId(string pageFileName)
        {
            
            Mode mode = charToMode[pageFileName[pageFileName.Length - 1 - FILE_EXTENSION.Length]];
            int randoId = int.Parse(pageFileName.Substring(0, pageFileName.Length - 2 - FILE_EXTENSION.Length));
            return new Tuple<int, Mode>(randoId, mode);
        }

        private string GeneratePageFileName(int randoId, Mode mode)
        {
            return randoId.ToString() + modeToChar[mode] + FILE_EXTENSION;
        }

        public bool IsGamePagedOut(int randoId, Mode mode)
        {
            lock (pagePendingGameSessions)
                return pagedGameSessionsFiles.ContainsKey(new Tuple<int, Mode>(randoId, mode));
        }

        public GameSession PageInGame(int randoId, Mode mode)
        {
            lock (pagedGameSessionsFiles)
            {
                Tuple<int, Mode> key = new Tuple<int, Mode>(randoId, mode);
                string pageFileName = pagedGameSessionsFiles[key];

                GameSession gameSession = JsonConvert.DeserializeObject<GameSession>(File.ReadAllText(pageFileName));
                gameSession.SetLogger(gameSessionFactory.GetLogWriter(randoId));

                pagedGameSessionsFiles.Remove(key);
                File.Delete(pageFileName);

                return gameSession;
            }
        }

        public void PageOutGame(GameSession gameSession)
        {
            lock (pagedGameSessionsFiles)
            {
                string serializedData = JsonConvert.SerializeObject(gameSession);
                string pageFileName = GeneratePageFileName(gameSession.GetRandoId(), gameSession.GetMode());
                File.WriteAllText(pageFileName, serializedData);
                pagedGameSessionsFiles.Add(new Tuple<int, Mode>(gameSession.GetRandoId(), gameSession.GetMode()), pageFileName);
            }
        }

        internal void MarkGameSessionForPaging(int randoId)
        {
            lock (pagePendingGameSessions)
                if (!pagePendingGameSessions.ContainsKey(randoId))
                    pagePendingGameSessions[randoId] = DateTime.Now;
        }

        internal void UnmarkGameSessionForPaging(int randoId)
        {
            lock (pagePendingGameSessions)
                pagePendingGameSessions.Remove(randoId);
        }

        internal void Update()
        {
            lock (pagePendingGameSessions)
            {
                List<int> randoIdsToPageOut = new List<int>();
                foreach (var pagePendingGameSession in pagePendingGameSessions)
                    if (pagePendingGameSession.Value - DateTime.Now > this.pagePendingTimeCap)
                        randoIdsToPageOut.Add(pagePendingGameSession.Key);
                foreach (int randoId in randoIdsToPageOut)
                    try
                    {
                        PageOutGame(popGameSessionCallback(randoId));
                    }
                    catch (InvalidOperationException) { }
            }
        }
    }
}
