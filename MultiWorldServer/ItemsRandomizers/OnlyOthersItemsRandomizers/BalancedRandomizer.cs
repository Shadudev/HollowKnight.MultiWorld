using MultiWorldLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWorldServer.ItemsRandomizers.OnlyOthersItemsRandomizers
{
    /*
     * Given a case of 3 players, A=300; B=240; C=180 items
     * Distribution will be:
     * A's world: 150 + 100 (250 total, 50 fillers)
     * B's world: 171 + 80 (251 total, 11 extras)
     * C's world: 129 + 90 (219 total, 39 extras)
     */

    internal class BalancedRandomizer : IItemsRandomizer
    {
        private List<PlayerItemsPool> playersItemsPools;
        private Random random;
        private int totalItemsAmount;

        public BalancedRandomizer(List<PlayerItemsPool> playersItemsPools, MultiWorldSettings settings) :
            base(playersItemsPools, settings) { }

        public override Placement[] GetPlayerItems(int playerId) => playersItemsPools[playerId].Placements;

        public override List<PlayerItemsPool> Randomize()
        {
            throw new NotImplementedException();
        }
    }
}
