using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;

namespace ItemSyncMod.SyncFeatures.SimpleKeysUsages
{
    public abstract class DoorUnlockLocation : AutoLocation
    {
        public string objectName, fsmName;

        public DoorUnlockLocation()
        {
            flingType = FlingType.DirectDeposit;
        }

        protected override void OnLoad()
        {
            Events.AddFsmEdit(sceneName, new(objectName, fsmName), OnEnable);
        }

        protected override void OnUnload()
        {
            Events.RemoveFsmEdit(sceneName, new(objectName, fsmName), OnEnable);
        }

        public abstract void OnEnable(PlayMakerFSM fsm);

        public static DoorUnlockLocation New(SimpleKeyUsageLocation location)
        {
            return location switch
            {
                SimpleKeyUsageLocation.Waterways => new WaterwaysUnlockLocation(),
                SimpleKeyUsageLocation.Jiji => new JijiUnlockLocation(),
                SimpleKeyUsageLocation.PleasureHouse => new PleasurehouseUnlockLocation(),
                SimpleKeyUsageLocation.Godhome => new GodhomeUnlockLocation(),
                _ => null,
            };
        }
    }

    public class WaterwaysUnlockLocation : DoorUnlockLocation
    {
        public WaterwaysUnlockLocation()
        {
            name = "Waterways_Entrance";
            sceneName = "Ruins1_05b";
            objectName = "Waterways Machine";
            fsmName = "Conversation Control";
        }

        public override void OnEnable(PlayMakerFSM fsm)
        {
            fsm.GetState("Activate").AddFirstAction(new AsyncLambda(GiveAll));
            fsm.GetState("Open").RemoveActionsOfType<SetPlayerDataInt>();
        }
    }

    internal class JijiUnlockLocation : DoorUnlockLocation
    {
        public JijiUnlockLocation()
        {
            name = "Jiji's_Door";
            sceneName = "Town";
            objectName = "Jiji Door";
            fsmName = "Conversation Control";
        }

        public override void OnEnable(PlayMakerFSM fsm)
        {
            fsm.GetState("Activate").AddFirstAction(new AsyncLambda(GiveAll));
            fsm.GetState("Activate").RemoveActionsOfType<PlayerDataIntAdd>();
        }
    }

    internal class PleasurehouseUnlockLocation : DoorUnlockLocation
    {
        public PleasurehouseUnlockLocation()
        {
            name = "Pleasure_House";
            sceneName = "Ruins2_04";
            objectName = "Inspect";
            fsmName = "Conversation Control";
        }

        public override void OnEnable(PlayMakerFSM fsm)
        {
            fsm.GetState("Open").AddFirstAction(new AsyncLambda(GiveAll));
            fsm.GetState("Open").RemoveActionsOfType<SetPlayerDataInt>();
        }
    }

    internal class GodhomeUnlockLocation : DoorUnlockLocation
    {
        public GodhomeUnlockLocation()
        {
            name = "Godhome";
            sceneName = "GG_Waterways";
            objectName = "Coffin";
            fsmName = "Conversation Control";
        }

        public override void OnEnable(PlayMakerFSM fsm)
        {
            fsm.GetState("Activate").AddFirstAction(new AsyncLambda(GiveAll));
            fsm.GetState("Activate").RemoveActionsOfType<PlayerDataIntAdd>();
        }
    }
}
