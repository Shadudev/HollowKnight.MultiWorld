using ItemChanger;
using ItemChanger.UIDefs;

namespace ItemSyncMod.SyncFeatures.SimpleKeysUsages
{
    internal class DoorUnlockItem : AbstractItem
    {
        private readonly SimpleKeyUsageLocation location;

        public DoorUnlockItem(SimpleKeyUsageLocation location)
        {
            // TODO Set the base members
            this.location = location;
            switch (location)
            {
                case SimpleKeyUsageLocation.Waterways:
                    name = "Waterways_Manhole_Unlocked";
                    break;
                case SimpleKeyUsageLocation.Jiji:
                    name = "Jiji's_Door_Unlocked";
                    break;
                case SimpleKeyUsageLocation.PleasureHouse:
                    name = "Pleasure_House_Unlocked";
                    break;
                case SimpleKeyUsageLocation.Godhome:
                    name = "Godhome_Unlocked";
                    break;
                default:
                    name = "Door_Unlocked";
                    break;
            }

            UIDef = new MsgUIDef()
            {
                name = new BoxedString(name.Replace('_', ' ')),
                shopDesc = new BoxedString($"Inform ItemSync developers if you see this."),
                sprite = new BoxedSprite(ItemChanger.Internal.SpriteManager.Instance.GetSprite("ShopIcons.SimpleKey"))
            };
        }

        public override void GiveImmediate(GiveInfo info)
        {
            switch (location)
            {
                case SimpleKeyUsageLocation.Waterways:
                    if (PlayerData.instance.openedWaterwaysManhole) return;

                    PlayerData.instance.openedWaterwaysManhole = true;
                    // TODO activate unlocking animation if current scene matches

                    break;
                case SimpleKeyUsageLocation.Jiji:
                    if (PlayerData.instance.jijiDoorUnlocked) return;
                
                    PlayerData.instance.jijiDoorUnlocked = true;
                    // TODO activate unlocking animation if current scene matches

                    break;
                case SimpleKeyUsageLocation.PleasureHouse:
                    if (PlayerData.instance.bathHouseOpened) return;
            
                    PlayerData.instance.bathHouseOpened = true;
                    // TODO activate unlocking animation if current scene matches
                    
                    break;
                case SimpleKeyUsageLocation.Godhome:
                    if (PlayerData.instance.godseekerUnlocked) return;

                    PlayerData.instance.godseekerUnlocked = true;
                    // TODO activate unlocking animation if current scene matches
                    break;
            }

            PlayerData.instance.simpleKeys--;
        }

        public static DoorUnlockItem New(SimpleKeyUsageLocation location)
        {
            switch (location)
            {
                case SimpleKeyUsageLocation.Waterways:
                case SimpleKeyUsageLocation.Jiji:
                case SimpleKeyUsageLocation.PleasureHouse:
                case SimpleKeyUsageLocation.Godhome:
                    return new DoorUnlockItem(location);
                default:
                    return null;
            }
        }
    }
}
