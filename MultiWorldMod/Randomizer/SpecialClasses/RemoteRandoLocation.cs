using MultiWorldMod.Items.Remote;
using RandomizerCore.Logic;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer.SpecialClasses
{
    public class InaccessibleLogic : OptimizedLogicDef
    {
        public InaccessibleLogic(string name, LogicManager lm) : base(name, GetInaccessibleLogic(), lm) { }

        private static int[] GetInaccessibleLogic()
        {
            return new int[] { (int)LogicOperators.ANY };
        }
    }

    public class RemoteRandoLocation : RandoModLocation
    {
        public RemoteRandoLocation(LogicManager lm)
        {
            logic = new InaccessibleLogic(RemotePlacement.REMOTE_PLACEMENT_NAME, lm);
            info = new LocationRequestInfo();
        }
    }
}
