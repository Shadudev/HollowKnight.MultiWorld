using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiWorldServer
{
    /// <summary>
    /// Ressembling the memory paging scheme, inactive game sessions are stored in files to reduce memory usage.
    /// Additional feature - wipe files that are a month old.
    /// </summary>
    internal class GameSessionPager
    {
        private readonly Dictionary<int, FileInfo> pagedGameSessionsFiles;

        public GameSessionPager(string storagePath)
        {
            pagedGameSessionsFiles = new DirectoryInfo(storagePath).GetFiles().ToDictionary(fileInfo => int.Parse(fileInfo.Name));
        }

        public bool IsGameSessionPagedOut(int randoId)
        {
            return pagedGameSessionsFiles.ContainsKey(randoId);
        }
    }
}
