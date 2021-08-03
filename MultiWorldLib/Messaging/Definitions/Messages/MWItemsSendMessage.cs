using System.Collections.Generic;

namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemsSendMessage)]
    public class MWItemsSendMessage : MWMessage
    {
        public List<(int, string, string)> Items { get; set; }

        public MWItemsSendMessage()
        {
            MessageType = MWMessageType.ItemsSendMessage;
        }
    }

    public class MWItemsSendDefinition : MWMessageDefinition<MWItemsSendMessage>
    {
        public MWItemsSendDefinition() : base(MWMessageType.ItemsSendMessage)
        {
            Properties.Add(new MWMessageProperty<List<(int, string, string)>, MWItemsSendMessage>(nameof(MWItemsSendMessage.Items)));
        }
    }
}
