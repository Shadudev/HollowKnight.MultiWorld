using ItemChanger;
using ItemChanger.Modules;
using MultiWorldMod.Items.Remote.UIDefs;

namespace MultiWorldMod.Items.Remote
{
    public class RemoteNotchCostUI : Module
    {
        public Dictionary<int, Dictionary<int, int>> RemoteCharmsCosts { get; set; } = new();

        public override void Initialize()
        {
            Events.OnStringGet += AddRemoteNotchCostToCharmName;
        }

        public override void Unload()
        {
            Events.OnStringGet -= AddRemoteNotchCostToCharmName;
        }

        public void AddPlayerNotchCosts(int playerId, Dictionary<int, int> costs)
        {
            LogHelper.LogDebug($"Received {MultiWorldMod.MWS.GetPlayerName(playerId)}'s charms: {string.Join(", ", costs)}");
            RemoteCharmsCosts[playerId] = costs;
        }

        private void AddRemoteNotchCostToCharmName(StringGetArgs args)
        {
            if (args.Source is RemoteString rs && rs.Inner is LanguageString ls && ls.key.StartsWith("CHARM_NAME_"))
            {
                string i = ls.key.Substring(11); // remove "CHARM_NAME_" prefix
                int j = i.IndexOf('_');
                if (j != -1) i = i.Substring(0, j); // remove "_A" suffix, etc such as on White Fragment

                int charmCostsSeparator = args.Current.LastIndexOf(" [");
                string baseCharmString;
                if (charmCostsSeparator == -1)
                    baseCharmString = args.Current;
                else
                    baseCharmString = args.Current.Substring(0, charmCostsSeparator);


                args.Current = GetCharmStringWithRemoteCost(baseCharmString, rs.PlayerId, int.Parse(i));
            }
        }

        private string GetCharmStringWithRemoteCost(string baseCharmString, int playerId, int charmId)
        {
            if (RemoteCharmsCosts.ContainsKey(playerId))
                return $"{baseCharmString} [{RemoteCharmsCosts[playerId][charmId]}]";
            return $"{baseCharmString}";
        }
    }
}
