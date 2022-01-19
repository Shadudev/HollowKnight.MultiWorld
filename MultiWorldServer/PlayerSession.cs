using System;
using System.Collections.Generic;
using MultiWorldLib.Messaging;
using MultiWorldLib.Messaging.Definitions;
using MultiWorldLib.Messaging.Definitions.Messages;

namespace MultiWorldServer
{
    class PlayerSession
    {
        public string Name;
        public int randoId;
        public int playerId;
        public ulong uid;

        public readonly List<ResendEntry> MessagesToConfirm = new List<ResendEntry>();

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
                MessagesToConfirm.Add(new ResendEntry(message));
            }
        }

        public List<MWConfirmableMessage> ConfirmMessage<T>(T message) where T : MWMessage, IConfirmMessage
        {
            List<MWConfirmableMessage> confirmedMessages = new List<MWConfirmableMessage>();

            lock (MessagesToConfirm)
            {
                for (int i = MessagesToConfirm.Count - 1; i >= 0; i--)
                {
                    MWConfirmableMessage confirmableMessage = MessagesToConfirm[i].Message;
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

        internal void ConfirmCharmNotchCosts(MWAnnounceCharmNotchCostsMessage message)
        {
            lock (MessagesToConfirm)
            {
                for (int i = MessagesToConfirm.Count - 1; i >= 0; i--)
                {
                    if (message.Confirms(MessagesToConfirm[i].Message))
                        continue;

                    MessagesToConfirm.RemoveAt(i);
                }
            }
        }

        internal void ConfirmCharmNotchCostsReceived(MWConfirmCharmNotchCostsReceivedMessage message)
        {
            lock (MessagesToConfirm)
            {
                for (int i = MessagesToConfirm.Count - 1; i >= 0; i--)
                {
                    if (!(MessagesToConfirm[i].Message is MWAnnounceCharmNotchCostsMessage))
                        continue;

                    MWAnnounceCharmNotchCostsMessage msg = MessagesToConfirm[i].Message as MWAnnounceCharmNotchCostsMessage;
                    if (msg.PlayerID == message.PlayerID)
                    {
                        MessagesToConfirm.RemoveAt(i);
                    }
                }
            }
        }
    }
}
