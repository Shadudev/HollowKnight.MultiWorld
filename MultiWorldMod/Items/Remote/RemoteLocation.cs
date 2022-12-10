using ItemChanger;
using ItemChanger.Locations;
using MultiWorldMod.Items.Remote.Tags;

namespace MultiWorldMod.Items.Remote
{
    internal class RemoteLocation : AutoLocation
    {
        private int locationOwnerId;

        public override AbstractPlacement Wrap()
        {
            RemotePlacement placement = new(name);
            RemotePlacementTag tag = placement.AddTag<RemotePlacementTag>();
            tag.LocationOwnerID = locationOwnerId;
            tag.LocationOwnerName = MultiWorldMod.MWS.GetPlayerName(locationOwnerId);

            return placement;
        }

        protected override void OnLoad() { }
        protected override void OnUnload() { }

        internal static AbstractLocation Create(string location, int locationOwnerId)
        {
            return new RemoteLocation()
            {
                name = location,
                flingType = FlingType.DirectDeposit,
                sceneName = null,
                Placement = null,
                tags = null,
                locationOwnerId = locationOwnerId
            };
        }
    }
}
