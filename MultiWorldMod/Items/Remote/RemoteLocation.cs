using ItemChanger;
using ItemChanger.Locations;

namespace MultiWorldMod.Items.Remote
{
    internal class RemoteLocation : AutoLocation
    {
        public override AbstractPlacement Wrap()
        {
            return new RemotePlacement(name);
        }

        protected override void OnLoad() { }
        protected override void OnUnload() { }

        internal static AbstractLocation CreateDefault()
        {
            return new RemoteLocation()
            {
                name = RemotePlacement.REMOTE_PLACEMENT_NAME,
                flingType = FlingType.DirectDeposit,
                sceneName = null,
                Placement = null,
                tags = null
            };
        }
    }
}
