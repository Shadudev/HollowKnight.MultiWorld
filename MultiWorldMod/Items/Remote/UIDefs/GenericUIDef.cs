using ItemChanger;
using ItemChanger.UIDefs;

namespace MultiWorldMod.Items.Remote.UIDefs
{
    internal class GenericUIDef : MsgUIDef
    {
        public static UIDef Create(string name, int playerId)
        {
            return new RemoteItemUIDef(
                new MsgUIDef()
                {
                    name = new BoxedString(name.Replace('_', ' ')),
                    shopDesc = new BoxedString($"We don't really know what's with this item.\nYou better ask {MultiWorldMod.MWS.GetPlayerName(playerId)} or just forever deny this item's existence."),
                    sprite = new EmptySprite()
                },
                playerId);
        }
    }
}
