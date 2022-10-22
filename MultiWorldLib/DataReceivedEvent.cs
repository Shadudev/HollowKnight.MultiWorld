namespace MultiWorldLib
{
    public class DataReceivedEvent
    {
        /// <summary>
        /// An ID to filter messages by
        /// </summary>
        public readonly string Label;
        /// <summary>
        /// The content of the message
        /// </summary>
        public readonly string Content;
        /// <summary>
        /// Sender's name
        /// </summary>
        public readonly string From;
        /// <summary>
        /// Toggled on after a message is processed by a callback
        /// </summary>
        public bool Handled { get; set; }

        public DataReceivedEvent(string label, string content, string from)
        {
            Label = label;
            Content = content;
            From = from;
            Handled = false;
        }
    }
}
