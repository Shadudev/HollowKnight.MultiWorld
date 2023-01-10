using System.Collections.Generic;
using MultiWorldLib.Messaging;
using MultiWorldLib.Messaging.Definitions;

namespace MultiWorldServer
{
    class PlayerSession
    {
        public string Name;
        public int randoId;
        public int playerId;
        public ulong uid;

        public readonly Dictionary<MWConfirmableMessage, int> MessagesToConfirm = new Dictionary<MWConfirmableMessage, int>();

        public PlayerSession(string Name, int randoId, int playerId, ulong uid)
        {
            this.Name = Name;
            this.randoId = randoId;
            this.playerId = playerId;
            this.uid = uid;
        }

        public void QueueConfirmableMessage(MWConfirmableMessage message) 
        {
            QueueConfirmableMessage(message, MultiWorldLib.Consts.DEFAULT_TTL);
        }

        public void QueueConfirmableMessage(MWConfirmableMessage message, int ttl)
        {
            lock (MessagesToConfirm)
                MessagesToConfirm[message] = ttl;
        }

        public List<MWConfirmableMessage> ConfirmMessage<T>(T message) where T : MWMessage, IConfirmMessage
        {
            List<MWConfirmableMessage> confirmedMessages = new List<MWConfirmableMessage>();

            lock (MessagesToConfirm)
            {
                foreach (MWConfirmableMessage confirmableMessage in MessagesToConfirm.Keys)
                {
                    if (message.Confirms(confirmableMessage))
                        confirmedMessages.Add(confirmableMessage);
                }

                confirmedMessages.ForEach(msg => MessagesToConfirm.Remove(msg));
            }

            return confirmedMessages;
        }
    }
}
