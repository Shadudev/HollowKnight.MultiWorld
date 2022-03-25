using ItemChanger;

namespace MultiWorldMod.Items.Remote.Tags
{
    internal class RemoteNotchCostTag : Tag, IShopNotchCostTag
    {
        public int Cost { get; set; }
        public int GetNotchCost(AbstractItem item) => Cost;
    }
}
