using System;

namespace MultiWorldLib
{
    [Serializable]
    public struct ResultData
    {
        public int playerId;
        public int randoId;
        public string[] nicknames;
        public Dictionary<string, (string, string)[]> PlayerItems { get; set; }
        public string ItemsSpoiler { get; set; }
    }
}
