using RandomizerCore;
using RandomizerCore.Logic;

namespace MultiWorldMod.Randomizer
{
    internal delegate void AddToOrderedCollection(ItemPlacement item);

    internal class OrderedItemUpdateEntry : UpdateEntry
    {
        private ItemPlacement itemPlacement;
        private AddToOrderedCollection addToOrderedCollection;

        public OrderedItemUpdateEntry(ItemPlacement itemPlacement, AddToOrderedCollection addToOrderedCollection)
        {
            this.itemPlacement = itemPlacement;
            this.addToOrderedCollection = addToOrderedCollection; ;
        }

        public override bool CanGet(ProgressionManager pm)
        {
            return itemPlacement.location.CanGet(pm);
        }

        public override IEnumerable<Term> GetTerms()
        {
            return itemPlacement.location.GetTerms();
        }

        public override void OnAdd(ProgressionManager pm)
        {
            addToOrderedCollection(itemPlacement);
            pm.Add(itemPlacement.item);
            if (itemPlacement.location is ILogicItem li) pm.Add(li);
        }

        public override void OnRemove(ProgressionManager pm)
        {
            return; // the pm handles removal via restriction
        }
    }
}
