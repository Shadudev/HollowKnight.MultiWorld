using System.Collections.Generic;

namespace MultiWorldServer
{
    public class PlayerItemsPool
    {
        public int ReadyId { get; set; }
        public Dictionary<string, (string, string)[]> ItemsPool { get; set; }
        public int RandoHash { get; set; }

        public string Nickname;
        public int PlayerId { get; set; }

        public PlayerItemsPool(int readyId, Dictionary<string, (string, string)[]> itemsPool, string nickname, int randoHash)
        {
            ReadyId = readyId;
            ItemsPool = itemsPool;
            RandoHash = randoHash;
            Nickname = nickname;
            PlayerId = -1;
        }
    }
}
