using ItemChanger;

namespace MultiWorldMod.Items.Remote.UIDefs
{
    internal class RemoteString : IString
    {
        public IString Inner { get; set; }
        public int PlayerId { get; set; }

        public string Value => Inner.Value;

        public RemoteString(IString inner, int playerId)
        {
            Inner = inner;
            PlayerId = playerId;
        }

        public IString Clone()
        {
            return (IString)MemberwiseClone();
        }
    }
}