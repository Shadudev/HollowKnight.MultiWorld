namespace MultiWorldLib.Messaging.Definitions
{
    public interface IConfirmMessage
    {
        public bool Confirms(MWConfirmableMessage message);
    }
}