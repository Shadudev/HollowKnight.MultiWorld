using ItemChanger;
using ItemChanger.UIDefs;

namespace MultiWorldMod.Items.Remote.UIDefs
{
    internal class RemoteCharmUIDef : RemoteItemUIDef
    {
        public static new UIDef Create(string name, int playerId)
        {
            return new RemoteCharmUIDef((MsgUIDef)Finder.GetItem(name).GetResolvedUIDef(), playerId);
        }

        public RemoteCharmUIDef(MsgUIDef msgDef, int playerId) : base(msgDef, playerId)
        {
            name = new RemoteString(msgDef.name, playerId);
        }
    }
}
