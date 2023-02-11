using ItemChanger;
using ItemChanger.Tags;
using ItemSyncMod.Items.DisplayMessageFormatter;
using Newtonsoft.Json;

namespace ItemSyncMod.Items
{
    internal class SyncedItemTag : Tag, IInteropTag
    {
        public static readonly string Local = "Local";
        public static readonly string Remote = "Remote";
        public static readonly string FromFieldName = "From";

        public string ItemID, FirstSender = null, MostRecentSender = null;
        public bool Given = false, WasObtainedLocallySet = false, WasObtainedLocally;
        public IDisplayMessageFormatter Formatter = new DefaultRemoteFormatter();

        public bool GetWasObtainedLocally => WasObtainedLocallySet && WasObtainedLocally;

        [JsonIgnore] private AbstractItem parent;
        [JsonIgnore] private bool isLocalPickUp = true; // Avoid sending received persistent items

        public string Message => "SyncedItemTag";

        public override void Load(object parent)
        {
            this.parent = (AbstractItem)parent;
            this.parent.AfterGive += AfterGiveItem;
        }

        public override void Unload(object parent)
        {
            this.parent = (AbstractItem)parent;
            this.parent.AfterGive -= AfterGiveItem;
        }

        public void AfterGiveItem(ReadOnlyGiveEventArgs args)
        {
            if (!WasObtainedLocallySet)
            {
                WasObtainedLocallySet = true;
                WasObtainedLocally = true;
            }

            if (isLocalPickUp && (!Given || IsItemSomewhatPersistent()))
            {
                Given = true;
                ItemManager.SendItemToAll(ItemID);
            }
        }

        public void GiveThisItem(AbstractPlacement placement, string from)
        {
            Given = true;
            
            if (!WasObtainedLocallySet)
            {
                WasObtainedLocallySet = true;
                WasObtainedLocally = false;
                FirstSender = from;
            }

            if (!parent.IsObtained())
            {
                isLocalPickUp = false;

                MostRecentSender = from;
                parent.OnGive += WrapUIDefAsReceived;
                parent.Give(placement, ItemManager.GetItemSyncStandardGiveInfo());
                parent.OnGive -= WrapUIDefAsReceived;

                isLocalPickUp = true;
            }
        }

        private static void WrapUIDefAsReceived(ReadOnlyGiveEventArgs giveEventArgs)
        {
            SyncedItemTag tag = giveEventArgs.Orig.GetTag<SyncedItemTag>();
            giveEventArgs.Item.UIDef = ReceivedItemUIDef.Convert(
                giveEventArgs.Item.UIDef, tag.MostRecentSender, tag.Formatter);
        }

        private bool IsItemSomewhatPersistent()
        {
            return parent.GetTag(out IPersistenceTag tag) && tag.Persistence != Persistence.Single;
        }

        public bool TryGetProperty<T>(string propertyName, out T value)
        {
            if (propertyName == Local && WasObtainedLocally is T retLocal)
            {
                value = retLocal;
                return true;
            }
            else if (propertyName == Remote && !WasObtainedLocally is T retRemote)
            {
                value = retRemote;
                return true;
            }
            else if (propertyName == FromFieldName && !WasObtainedLocally && FirstSender is T from)
            {
                value = from;
                return true;
            }
            value = default;
            return false;
        }
    }
}
