using System;
using System.Reflection;
using System.Threading;

namespace MultiWorldMod
{
    internal class SettingsSync
    {
        private bool shouldWaitForSettings = true, shouldUploadSettings = false, shouldSync = false;
		private readonly object waitForSettingsLock = new object();

		public void AddSpoilerInitListener()
		{
			Type.GetType("RandomizerMod.Events, RandomizerMod3.0")
				.GetEvent("OnSpoilerLogInit", BindingFlags.Public | BindingFlags.Static)
				.AddEventHandler(null, (Action)SpoilerInitListener);
		}

		// Spoiler is initialized after all Randomizer mod settings from menu are set to SaveSettings instance
		public void SpoilerInitListener()
		{
			if (shouldSync)
			{
				lock (waitForSettingsLock)
				{
					if (shouldUploadSettings)
					{
						string settingsJson = UnityEngine.JsonUtility.ToJson(RandomizerMod.RandomizerMod.Instance.Settings);
						ItemSync.Instance.Connection.UploadRandomizerSettings(settingsJson);
					} 
					else if (shouldWaitForSettings)
					{
						Monitor.Wait(waitForSettingsLock);
					} 
				}
			}
			LogHelper.Log("SettingsSync succeeded");
		}

		public void MarkShouldUploadSettings()
		{
			lock (waitForSettingsLock)
			{
				shouldUploadSettings = true;
				shouldWaitForSettings = false;
			}
		}

		public void ApplyRandomizerSettings(string settingsJson)
        {
			LogHelper.Log("Applying received settings");
			bool originalExtraPlats = RandomizerMod.RandomizerMod.Instance.Settings.ExtraPlatforms;
			bool originalNPCItemDialogue = RandomizerMod.RandomizerMod.Instance.Settings.NPCItemDialogue;

			RandomizerMod.RandomizerMod.Instance.UnhookRandomizer();
			RandomizerMod.RandomizerMod.Instance.Settings =
				UnityEngine.JsonUtility.FromJson<RandomizerMod.SaveSettings>(settingsJson);

			RandomizerMod.RandomizerMod.Instance.Settings.ExtraPlatforms = originalExtraPlats;
			RandomizerMod.RandomizerMod.Instance.Settings.NPCItemDialogue = originalNPCItemDialogue;
			
			lock (waitForSettingsLock)
			{
				shouldWaitForSettings = false;
				Monitor.Pulse(waitForSettingsLock);
			}
		}

        internal void Reset()
        {
			shouldWaitForSettings = true;
			shouldUploadSettings = false;
		}

		internal bool ShouldWaitForSettings()
        {
			return shouldWaitForSettings;

		}

        internal void ToggleSync(bool value)
        {
			shouldSync = value;
        }
    }
}
