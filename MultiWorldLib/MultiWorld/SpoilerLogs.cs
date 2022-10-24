namespace MultiWorldLib.MultiWorld
{
    [Serializable]
    public class SpoilerLogs
    {
        public string FullOrderedItemsLog { get; set; } = "";
        public Dictionary<string, string> IndividualWorldSpoilers { get; set; } = new Dictionary<string, string>();
    }
}
