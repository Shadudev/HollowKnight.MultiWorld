namespace MultiWorldLib
{
    public class PlayerItemsPool
    {
        public int PlayerId { get; set; }
        public (int, string, string)[] ItemsPool { get; set; }

        public string Nickname;

        public PlayerItemsPool(int playerId, (int, string, string)[] itemsPool, string nickname)
        {
            PlayerId = playerId;
            ItemsPool = itemsPool;
            Nickname = nickname;
        }
    }
}
