using ItemChanger;

namespace MultiWorldMod.Items.Remote
{
    // Simple item used for unfamiliar items (remote modded item, mod isn't installed locally)
    public class GenericAbstractItem : AbstractItem
    {
        public override void GiveImmediate(GiveInfo info) { }
    }
}
