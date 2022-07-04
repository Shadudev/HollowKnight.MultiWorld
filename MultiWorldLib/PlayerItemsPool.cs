namespace MultiWorldLib
{
    public class PlayerItemsPool
    {
        public int ReadyId { get; set; }
        public Placement[] Placements { get; set; }

        public string Nickname;
        public int PlayerId { get; set; }

        public PlayerItemsPool(int readyId, Placement[] placements, string nickname)
        {
            ReadyId = readyId;
            Placements = placements;
            Nickname = nickname;
            PlayerId = -1;
        }
    }
}
