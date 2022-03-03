using ItemChanger;
using ItemChanger.Tags;
using Newtonsoft.Json;

namespace ItemSyncMod.Items
{
    internal class SyncedItemTag : Tag, IInteropTag
    {
        public static readonly string Local = "Local";
        public static readonly string Remote = "Remote";
        public static readonly string FromFieldName = "From";

        public string ItemID, From = null;
        public bool Given = false, WasObtainedLocallySet = false, WasObtainedLocally;

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

            if (!ItemManager.ShouldItemBeIgnored(ItemID) && isLocalPickUp && (!Given || IsItemSomewhatPersistent()))
            {
                Given = true;
                ItemSyncMod.ISSettings.AddSentItem(ItemID);
                ItemSyncMod.Connection.SendItemToAll(ItemID);
            }
        }

        public void GiveThisItem(string from)
        {
            Given = true;
            
            if (!WasObtainedLocallySet)
            {
                WasObtainedLocallySet = true;
                WasObtainedLocally = false;
                From = from;
            }

            if (!parent.IsObtained())
            {
                isLocalPickUp = false;
                UIDef orig = parent.UIDef;
                parent.UIDef = RemoteUIDef.TryConvert(orig, from);
                parent.Give(ItemManager.GetItemPlacement(ItemID), ItemManager.GetItemSyncStandardGiveInfo());
                parent.UIDef = orig;
                isLocalPickUp = true;
            }
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
            else if (propertyName == FromFieldName && !WasObtainedLocally && From is T from)
            {
                value = from;
                return true;
            }
            value = default;
            return false;
        }
    }
}
