using ItemChanger;
using ItemChanger.Tags;

namespace MultiWorldMod.Items.Remote.Tags
{
    internal class RemotePlacementTag : Tag, IInteropTag
    {
        private const string DO_NOT_MAKE_PIN = "DoNotMakePin";
        public bool DoNotMakePin = true;

        public string Message => "RandoSupplementalMetadata";

        public int LocationOwnerID { get; internal set; }
        public string LocationOwnerName { get; internal set; }

        public bool TryGetProperty<T>(string propertyName, out T value)
        {
            if (!string.IsNullOrEmpty(propertyName) && propertyName == DO_NOT_MAKE_PIN && DoNotMakePin is T doNotMakePin)
            {
                value = doNotMakePin;
                return true;
            }

            value = default;
            return false;
        }
    }
}
