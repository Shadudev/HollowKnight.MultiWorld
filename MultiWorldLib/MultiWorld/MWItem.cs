using System;

namespace MultiWorldLib
{
    [Serializable]
    public class MWItem
    {
        public int PlayerId { get; set; }
        public string Item { get; set; }

        public MWItem()
        {
            PlayerId = -1;
            Item = "";
        }

        public MWItem(int playerId, string item)
        {
            PlayerId = playerId;
            Item = item;
        }

        public MWItem(string itemId)
        {
            (PlayerId, Item) = LanguageStringManager.ExtractPlayerID(itemId);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType()) return false;
            MWItem other = (MWItem)obj;
            return PlayerId == other.PlayerId && Item == other.Item;
        }

        public override int GetHashCode()
        {
            return (PlayerId, Item).GetHashCode();
        }

        public override string ToString()
        {
            return LanguageStringManager.AddPlayerId(Item, PlayerId);
        }

        public static explicit operator MWItem(string s)
        {
            return new MWItem(s);
        }
    }
}