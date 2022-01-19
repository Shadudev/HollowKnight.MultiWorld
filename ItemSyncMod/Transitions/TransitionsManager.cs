using ItemChanger;
using ItemChanger.Modules;
using ItemSyncMod.Randomizer;
using RandomizerMod.IC;

namespace ItemSyncMod.Transitions
{
    internal class TransitionsManager
    {
        internal static void Setup()
        {
            ItemChangerMod.Modules.Add<TransitionsFoundSyncer>();
        }
        internal static void MarkTransitionFound(string source, string target)
        {
            GetTransitionFields(target, out string sceneName, out string gateName);
            Transition t = new(sceneName, gateName);
            TrackerUpdate.SendTransitionFound(t);

            if (ItemChangerMod.Modules.Get<CompletionPercentOverride>() is CompletionPercentOverride cpo)
            {
                cpo.FoundTransitions.Add(t);
            }
        }

        // Based on ItemChanger.Transition.ToString()
        private static void GetTransitionFields(string transition, out string sceneName, out string gateName)
        {
            sceneName = transition.Substring(0, transition.IndexOf('['));
            gateName = transition.Substring(sceneName.Length + 1, transition.Length - sceneName.Length - 2);
        }
    }
}
