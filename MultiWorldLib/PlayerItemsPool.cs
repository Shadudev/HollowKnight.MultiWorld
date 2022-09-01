namespace MultiWorldLib
{
    public class PlayerItemsPool
    {
        public int ReadyId { get; set; }
        public Dictionary<string, (string, string)[]> ItemsPool { get; set; }

        public string Nickname;
        public int PlayerId { get; set; }

        public PlayerItemsPool(int readyId, Dictionary<string, (string, string)[]> itemsPool, string nickname)
        {
            ReadyId = readyId;
            ItemsPool = itemsPool;
            Nickname = nickname;
            PlayerId = -1;
        }
    }
}
