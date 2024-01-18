using MultiWorldLib;
using MultiWorldServer.Loggers;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultiWorldServer.Game
{
    public class GameSessionFactory
    {
        private readonly Config config;
        private readonly LogWriter defaultLogWriter;

        public GameSessionFactory(Config config, LogWriter defaultLogWriter)
        {
            this.config = config;
            this.defaultLogWriter = defaultLogWriter;
        }

        public GameSession CreateGameSession(int randoId, Mode mode)
        {
            return new GameSession(randoId, mode, GetLogWriter(randoId));
        }

        public GameSession CreateGameSession(int randoId, List<int> playerIds, Mode mode)
        {
            return new GameSession(randoId, playerIds, mode, GetLogWriter(randoId));
        }

        internal LogWriter GetLogWriter(int randoId)
        {
            if (config.CreateLogPerGameSession)
                return LogWriterFactory.CreateLogger(GetGameSessionLogFileName(randoId),
                    config.MaxGameLogSize, FileMode.OpenOrCreate);
            return defaultLogWriter;
        }

        private string GetGameSessionLogFileName(int randoId)
        {
            return $"games\\{randoId}.txt";
        }
    }
}
