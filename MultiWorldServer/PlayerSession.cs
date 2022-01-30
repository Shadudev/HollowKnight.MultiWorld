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

        public readonly List<MWConfirmableMessage> MessagesToConfirm = new List<MWConfirmableMessage>();

        public PlayerSession(string Name, int randoId, int playerId, ulong uid)
        {
            this.Name = Name;
            this.randoId = randoId;
            this.playerId = playerId;
            this.uid = uid;
        }

        public void QueueConfirmableMessage(MWConfirmableMessage message) 
        {
            // TODO change this to add to a side queue which is added after the next cycle
            // This allows a workers pool to attempt sending it sooner (allowing quicker syncing)
            // More in the private Google Document
            lock (MessagesToConfirm)
            {
                MessagesToConfirm.Add(message);
            }
        }

        public List<MWConfirmableMessage> ConfirmMessage<T>(T message) where T : MWMessage, IConfirmMessage
        {
            List<MWConfirmableMessage> confirmedMessages = new List<MWConfirmableMessage>();

            lock (MessagesToConfirm)
            {
                for (int i = 0; i < MessagesToConfirm.Count; i++)
                {
                    MWConfirmableMessage confirmableMessage = MessagesToConfirm[i];
                    if (message.Confirms(confirmableMessage))
                    {
                        confirmedMessages.Add(confirmableMessage);
                        MessagesToConfirm.RemoveAt(i);
                        i--;
                    }
                }
            }

            return confirmedMessages;
        }
    }
}
