using ItemChanger;
using ItemChanger.UIDefs;

namespace ItemSyncMod.Items
{
    public class RemoteUIDef : MsgUIDef
    {
        public static UIDef TryConvert(UIDef orig, string from)
        {
            if (orig is MsgUIDef msgDef)
                return new RemoteUIDef(msgDef, from);
            else return orig;
        }

        private RemoteUIDef(MsgUIDef msgDef, string from)
        {
            name = new BoxedString($"{msgDef.GetPostviewName()}\nFrom {from}");
            shopDesc = msgDef.shopDesc?.Clone();
            sprite = msgDef.sprite?.Clone();
        }
    }
}
