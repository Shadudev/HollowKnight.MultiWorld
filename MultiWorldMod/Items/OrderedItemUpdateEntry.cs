using RandomizerCore;
using RandomizerCore.Logic;

namespace MultiWorldMod.Items
{
    internal delegate void AddToOrderedCollection(GeneralizedPlacement item);

    internal class OrderedItemUpdateEntry : UpdateEntry
    {
        private readonly GeneralizedPlacement itemPlacement;
        private readonly AddToOrderedCollection addToOrderedCollection;

        public OrderedItemUpdateEntry(GeneralizedPlacement itemPlacement, AddToOrderedCollection addToOrderedCollection)
        {
            this.itemPlacement = itemPlacement;
            this.addToOrderedCollection = addToOrderedCollection;
        }

        public override bool CanGet(ProgressionManager pm)
        {
            return itemPlacement.Location.CanGet(pm);
        }

        public override IEnumerable<Term> GetTerms()
        {
            return itemPlacement.Location.GetTerms();
        }

        public override void OnAdd(ProgressionManager pm)
        {
            addToOrderedCollection(itemPlacement);
            pm.Add(itemPlacement.Item);
            if (itemPlacement.Location is ILogicItem li) pm.Add(li);
        }

        public override void OnRemove(ProgressionManager pm)
        {
            return; // the pm handles removal via restriction
        }
    }
}
