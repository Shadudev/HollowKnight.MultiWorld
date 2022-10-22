using ItemChanger;
using ItemChanger.UIDefs;

namespace MultiWorldMod.Items.Remote.UIDefs
{
    internal class RemoteCharmUIDef : RemoteItemUIDef
    {
        public static new UIDef Create(AbstractItem item, int playerId)
        {
            return new RemoteCharmUIDef((MsgUIDef) item.UIDef, playerId);
        }

        public RemoteCharmUIDef(MsgUIDef msgDef, int playerId) : base(msgDef, playerId)
        {
            name = new RemoteString(msgDef?.name, playerId);
        }
    }
}
