using System;
using System.Collections.Generic;
using MultiWorldLib.Messaging;
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

        public void QueueConfirmableMessage(MWMessage message)
        {
            switch (message.MessageType)
            {
                case MWMessageType.ItemReceiveMessage:
                case MWMessageType.RequestCharmNotchCostsMessage:
                case MWMessageType.AnnounceCharmNotchCostsMessage:
                    lock (MessagesToConfirm)
                    {
                        MessagesToConfirm.Add(new ResendEntry(message));
                    }
                    break;

                default:
                   throw new InvalidOperationException("Server should only queue ItemReceive and Announce/Request of CharmNotchCosts messages for confirmation");
            }

        }

        public List<MWMessage> ConfirmMessage(MWMessage message)
        {
            if (message.MessageType == MWMessageType.ItemReceiveConfirmMessage)
            {
                return ConfirmItemReceive((MWItemReceiveConfirmMessage)message);
            }
            else
            {
                throw new InvalidOperationException("Must only confirm ItemReceive messages.");
            }
        }

        internal void ConfirmCharmNotchCosts(MWAnnounceCharmNotchCostsMessage message)
        {
            lock (MessagesToConfirm)
            {
                for (int i = MessagesToConfirm.Count - 1; i >= 0; i--)
                {
                    if (!(MessagesToConfirm[i].Message is MWRequestCharmNotchCostsMessage))
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

        private List<MWMessage> ConfirmItemReceive(MWItemReceiveConfirmMessage message)
        {
            List<MWMessage> confirmedMessages = new List<MWMessage>();

            lock (MessagesToConfirm)
            {
                for (int i = MessagesToConfirm.Count - 1; i >= 0; i--)
                {
                    if (!(MessagesToConfirm[i].Message is MWItemReceiveMessage))
                        continue;

                    MWItemReceiveMessage icm = MessagesToConfirm[i].Message as MWItemReceiveMessage;
                    if (icm.Item == message.Item && icm.From == message.From)
                    {
                        confirmedMessages.Add(icm);
                        MessagesToConfirm.RemoveAt(i);
                    }
                }
            }

            return confirmedMessages;
        }
    }
}
