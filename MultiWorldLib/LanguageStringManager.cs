using System.Text.RegularExpressions;

namespace MultiWorldLib
{
    public static class LanguageStringManager
    {
        public static (int PlayerId, string Item) ExtractPlayerID(string mwItem)
        {
            Regex prefix = new(@"^MW\((\d+)\)_");
            if (!prefix.IsMatch(mwItem)) return (-1, mwItem);
            int id = int.Parse(prefix.Match(mwItem).Groups[1].Value);
            return (id, prefix.Replace(mwItem, ""));
        }

        public static string AddPlayerId(string item, int playerId)
        {
            return "MW(" + (playerId) + ")_" + item;
        }
        
        public static (string item, int id) ExtractItemID(string input)
        {
            Regex suffix = new(@"_\(([-]?\d+)\)$");
            if (!suffix.IsMatch(input)) return (input, 0);
            Match m = suffix.Match(input);
            return (suffix.Replace(input, ""), int.Parse(m.Groups[1].Value));
        }

        public static string AddItemId(string str, int id)
        {
            return $"{str}_({id})";
        }

        public static string GetItemName(string newItemId)
        {
            return ExtractItemID(newItemId).item;
        }
    }
}
