using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Locations;
using ItemChanger.UIDefs;
using ItemSyncMod.Items;

namespace ItemSyncMod.SyncFeatures.SimpleKeysUsages
{
    internal class DoorUnlockItem : AbstractItem
    {
        private readonly SimpleKeyUsageLocation location;

        public DoorUnlockItem(SimpleKeyUsageLocation location)
        {
            this.location = location;
            name = location switch
            {
                SimpleKeyUsageLocation.Waterways => "Waterways_Manhole_Unlocked",
                SimpleKeyUsageLocation.Jiji => "Jiji's_Door_Unlocked",
                SimpleKeyUsageLocation.PleasureHouse => "Pleasure_House_Unlocked",
                SimpleKeyUsageLocation.Godhome => "Godhome_Unlocked",
                _ => "Door_Unlocked",
            };
            UIDef = new MsgUIDef()
            {
                name = new BoxedString(name.Replace('_', ' ')),
                shopDesc = new BoxedString($"Inform ItemSync developers if you see this."),
                sprite = new BoxedSprite(ItemChanger.Internal.SpriteManager.Instance.GetSprite("ShopIcons.SimpleKey"))
            };
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            ItemManager.OnItemReceived += ActivateAnimationIfSceneMatches;
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            ItemManager.OnItemReceived -= ActivateAnimationIfSceneMatches;
        }

        public override void GiveImmediate(GiveInfo info)
        {
            switch (location)
            {
                case SimpleKeyUsageLocation.Waterways:
                    if (PlayerData.instance.openedWaterwaysManhole) return;
                    PlayerData.instance.openedWaterwaysManhole = true;
                    break;
                case SimpleKeyUsageLocation.Jiji:
                    if (PlayerData.instance.jijiDoorUnlocked) return;
                    PlayerData.instance.jijiDoorUnlocked = true;
                    break;
                case SimpleKeyUsageLocation.PleasureHouse:
                    if (PlayerData.instance.bathHouseOpened) return;
                    PlayerData.instance.bathHouseOpened = true;
                    break;
                case SimpleKeyUsageLocation.Godhome:
                    if (PlayerData.instance.godseekerUnlocked) return;
                    PlayerData.instance.godseekerUnlocked = true;
                    break;
            }

            PlayerData.instance.simpleKeys--;
        }

        private void ActivateAnimationIfSceneMatches(ItemManager.ItemReceivedEvent itemReceivedEvent)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            switch (location)
            {
                case SimpleKeyUsageLocation.Waterways when currentScene == "Ruins1_05b":
                    PlayMakerFSM fsm = ObjectLocation.FindGameObject("Waterways Machine").
                        LocateMyFSM("Conversation Control");
                    // Ensures this isn't the client that used a key
                    if (fsm.ActiveStateName == "Idle")
                    {
                        fsm.SetState("Activate");
                        fsm.Fsm.Start();
                    }
                    break;
                case SimpleKeyUsageLocation.Jiji when currentScene == "Town":
                    fsm = ObjectLocation.FindGameObject("Jiji Door").
                        LocateMyFSM("Conversation Control");
                    if (fsm.ActiveStateName == "Idle")
                    {
                        fsm.SetState("Activate");
                        fsm.Fsm.Start();
                    }
                    break;
                case SimpleKeyUsageLocation.PleasureHouse when currentScene == "Ruins2_04":
                    fsm = ObjectLocation.FindGameObject("Inspect").
                        LocateMyFSM("Conversation Control");
                    if (fsm.ActiveStateName == "Idle")
                    {
                        fsm.SetState("Open");
                        fsm.Fsm.Start();
                    }
                    break;
                case SimpleKeyUsageLocation.Godhome when currentScene == "GG_Waterways":
                    fsm = ObjectLocation.FindGameObject("Coffin").
                        LocateMyFSM("Conversation Control");
                    if (fsm.ActiveStateName == "Idle")
                    {
                        fsm.SetState("Activate");
                        fsm.Fsm.Start();
                    }
                    break;
            }
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
