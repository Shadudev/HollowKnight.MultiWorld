namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ItemReceiveMessage)]
    public class MWItemReceiveMessage : MWMessage
    {
        public string Item { get; set; }
        public string Location { get; set; }
        public string From { get; set; }

        public MWItemReceiveMessage()
        {
            MessageType = MWMessageType.ItemReceiveMessage;
        }

        // I kinda don't like doing this here and not anywhere else, but it lets us use these for Server's unconfirmedItems set
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            MWItemReceiveMessage other = obj as MWItemReceiveMessage;
            return Item == other.Item && Location == other.Location && From == other.From;
        }

        public override int GetHashCode()
        {
            return (Item, Location, From).GetHashCode();
        }
    }

    public class MWItemReceiveDefinition : MWMessageDefinition<MWItemReceiveMessage>
    {
        public MWItemReceiveDefinition() : base(MWMessageType.ItemReceiveMessage)
        {
            Properties.Add(new MWMessageProperty<string, MWItemReceiveMessage>(nameof(MWItemReceiveMessage.Item)));
            Properties.Add(new MWMessageProperty<string, MWItemReceiveMessage>(nameof(MWItemReceiveMessage.Location)));
            Properties.Add(new MWMessageProperty<string, MWItemReceiveMessage>(nameof(MWItemReceiveMessage.From)));
        }
    }
}
