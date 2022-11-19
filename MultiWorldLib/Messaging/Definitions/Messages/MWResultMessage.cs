using MultiWorldLib.MultiWorld;

namespace MultiWorldLib.Messaging.Definitions.Messages
{
    [MWMessageType(MWMessageType.ResultMessage)]
    public class MWResultMessage : MWMessage
    {
        public int PlayerId { get; set; }
        public int RandoId { get; set; }
        public string[] Nicknames { get; set; }
        public SpoilerLogs ItemsSpoiler { get; set; }

        /// <summary>
        /// Placements for the player's own world. These include others' items.
        /// </summary>
        public Dictionary<string, (string item, string location)[]> Placements { get; set; }

        /// <summary>
        /// Placements for the player's owned items. These include locations in other players' worlds.
        /// </summary>
        public Dictionary<string, string> PlayerItemsPlacements { get; set; }
        public string GeneratedHash { get; set; } = "";

        public MWResultMessage()
        {
            MessageType = MWMessageType.ResultMessage;
        }
    }

    public class MWResultMessageDefinition : MWMessageDefinition<MWResultMessage>
    {
        public MWResultMessageDefinition() : base(MWMessageType.ResultMessage)
        {
            Properties.Add(new MWMessageProperty<int, MWResultMessage>(nameof(MWResultMessage.PlayerId)));
            Properties.Add(new MWMessageProperty<int, MWResultMessage>(nameof(MWResultMessage.RandoId)));
            Properties.Add(new MWMessageProperty<string[], MWResultMessage>(nameof(MWResultMessage.Nicknames)));
            Properties.Add(new MWMessageProperty<SpoilerLogs, MWResultMessage>(nameof(MWResultMessage.ItemsSpoiler)));
            Properties.Add(new MWMessageProperty<Dictionary<string, (string, string)[]>, MWResultMessage>(nameof(MWResultMessage.Placements)));
            Properties.Add(new MWMessageProperty<Dictionary<string, string>, MWResultMessage>(nameof(MWResultMessage.PlayerItemsPlacements)));
            Properties.Add(new MWMessageProperty<string, MWResultMessage>(nameof(MWResultMessage.GeneratedHash)));
        }
    }
}
