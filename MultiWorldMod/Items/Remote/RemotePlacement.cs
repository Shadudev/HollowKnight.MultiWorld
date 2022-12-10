using ItemChanger;
using MultiWorldMod.Items.Remote.Tags;

namespace MultiWorldMod.Items.Remote
{
    internal class RemotePlacement : AbstractPlacement
    {
        public RemotePlacement(string Name, int locationOwnerId) : base(Name)
        {
            RemotePlacementTag tag = AddTag<RemotePlacementTag>();
            tag.LocationOwnerID = locationOwnerId;
            tag.LocationOwnerName = MultiWorldMod.MWS.GetPlayerName(locationOwnerId);
        }

        protected override void OnLoad() { }

        protected override void OnUnload() { }
    }
}
