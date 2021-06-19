namespace MultiWorldLib
{
    public class PlayerItemsPool
    {
        public int ReadyId { get; set; }
        public (int, string, string)[] ItemsPool { get; set; }

        public string Nickname;
        public int PlayerId { get; set; }

        public PlayerItemsPool(int readyId, (int, string, string)[] itemsPool, string nickname)
        {
            ReadyId = readyId;
            ItemsPool = itemsPool;
            Nickname = nickname;
            PlayerId = -1;
        }
    }
}
