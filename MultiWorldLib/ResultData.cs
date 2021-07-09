using System;

namespace MultiWorldLib
{
    [Serializable]
    public struct ResultData
    {
        public int playerId;
        public int randoId;
        public string[] nicknames;
        public (int, string, string)[] PlayerItems { get; set; }
        public string ItemsSpoiler { get; set; }
    }
}
