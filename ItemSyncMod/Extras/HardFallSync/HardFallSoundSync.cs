using ItemSyncMod.Items;
using MultiWorldLib;
using UnityEngine;
using static ItemSyncMod.Items.ItemManager;

namespace ItemSyncMod.Extras.HardFallSync
{
    internal class HardFallSoundSync : IAdditionalFeatureModule
    {
        private static readonly string ID = "hero_land_hard";
        private readonly AudioClip audio;
        private static readonly System.Random random = new(); // Just to spare some allocations

        public HardFallSoundSync()
        {
            audio = ItemChanger.Internal.SoundManager.FromStream(
                typeof(ItemSyncMod).Assembly.GetManifestResourceStream($"ItemSyncMod.Resources.Sounds.{ID}.wav"), ID);
        }

        public List<(string, string)> GetPreloadNames()
        {
            return new();
        }
        public void SavePreloads(Dictionary<string, Dictionary<string, GameObject>> preloads)
        {
            // This shouldn't do anything...
        }

        public void Hook()
        {
            On.HeroController.DoHardLanding += HeroController_DoHardLanding;
            OnItemReceived += OnItemGive;
        }

        public void Unhook()
        {
            On.HeroController.DoHardLanding -= HeroController_DoHardLanding;
            OnItemReceived -= OnItemGive;
        }

        private void HeroController_DoHardLanding(On.HeroController.orig_DoHardLanding orig, HeroController self)
        {
            orig(self);

            if (random.Next(1000) != 789) return;

            LogHelper.LogDebug("I'm so chonky, everyone can hear me when I hard fall");
            ItemManager.SendItemToAll(ID);
        }

        private void OnItemGive(DataReceivedEvent itemReceivedEvent)
        {
            if (itemReceivedEvent.Content == ID)
            {
                itemReceivedEvent.Handled = true;
                AudioPlayer.PlayAudio(audio);
            }
        }
    }
}
