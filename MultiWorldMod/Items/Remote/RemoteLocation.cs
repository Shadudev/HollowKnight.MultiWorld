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

        internal static AbstractLocation Create(string location)
        {
            return new RemoteLocation()
            {
                name = location,
                flingType = FlingType.DirectDeposit,
                sceneName = null,
                Placement = null,
                tags = null
            };
        }
    }
}
