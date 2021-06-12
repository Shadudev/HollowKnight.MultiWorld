namespace MultiWorldLib.Messaging.Definitions
{
    [MWMessageType(MWMessageType.SharedCore)]
    public class MWSharedCore : MWMessage
    {
        public MWSharedCore()
        {
            MessageType = MWMessageType.SharedCore;
        }
    }

    public class MWSharedCoreDefinition : MWMessageDefinition<MWSharedCore>
    {
        public MWSharedCoreDefinition() : base(MWMessageType.SharedCore) { }
    }
}
