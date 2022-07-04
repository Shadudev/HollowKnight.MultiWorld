namespace MultiWorldLib
{
    public class Item
    {
        public int OwnerID { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
    }

    public class Location
    {
        public int Index { get; set; }
        public int OwnerID { get; set; }
        public string Name { get; set; }
    }

    public class Placement
    {
        public Item Item { get; set; }
        public Location Location { get; set; }
    }
}
