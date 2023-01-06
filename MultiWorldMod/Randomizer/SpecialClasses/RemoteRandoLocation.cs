using RandomizerCore.Logic;
using RandomizerMod.RC;

namespace MultiWorldMod.Randomizer.SpecialClasses
{
    public class RemotelyPlacedLogic
    {
        public static LogicDef Get(string name, LogicManager lm)
        {
            return lm.FromString(new(name, "NONE"));
        } 
    }

    public class RemoteRandoLocation : RandoModLocation
    {
        public RemoteRandoLocation(LogicManager lm, string mwLocation)
        {
            logic = RemotelyPlacedLogic.Get(mwLocation, lm);
            info = new LocationRequestInfo();
        }
    }
}
