using MultiWorldMod.Items.Remote;
using RandomizerCore.Logic;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer.SpecialClasses
{
    public class RemotelyPlacedLogic
    {
        public static OptimizedLogicDef Get(string name, LogicManager lm)
        {
            return new(name, GetInaccessibleLogic(), lm);
        } 

        private static int[] GetInaccessibleLogic()
        {
            return new int[] { (int)LogicOperators.ANY };
        }
    }

    public class RemoteRandoLocation : RandoModLocation
    {
        public RemoteRandoLocation(LogicManager lm)
        {
            logic = RemotelyPlacedLogic.Get(RemotePlacement.REMOTE_PLACEMENT_NAME, lm);
            info = new LocationRequestInfo();
        }
    }
}
