namespace MultiWorldLib
{
    public class PlayerItemsPool
    {
        public int PlayerId { get; set; }
        public (int, string, string)[] ItemsPool { get; set; }

        public PlayerItemsPool(int playerId, (int, string, string)[] itemsPool)
        {
            PlayerId = playerId;
            ItemsPool = itemsPool;
        }
    }
}
