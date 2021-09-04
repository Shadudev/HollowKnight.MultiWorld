using Modding;
using System;
using System.Reflection;
using System.Threading;

namespace MultiWorldMod
{
    internal class SettingsSync
    {
        private bool spoilerInitialized = false, shouldWaitForSettings = true, shouldSync = false;
		private readonly object spoilerInitLock = new object();
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
				lock (spoilerInitLock)
				{
					spoilerInitialized = true;
					Monitor.Pulse(spoilerInitLock);
				}

				lock (waitForSettingsLock)
				{
					if (shouldWaitForSettings)
					{
						Monitor.Wait(waitForSettingsLock);
					}
				}
			}
		}

		public void UploadRandomizerSettings()
		{
			lock (waitForSettingsLock)
			{
				shouldWaitForSettings = false;
				// If for any reason this happens after SpoilerInitListener, continue rando flow
				Monitor.Pulse(waitForSettingsLock);
			}

			lock (spoilerInitLock)
			{
				if (!spoilerInitialized)
					Monitor.Wait(spoilerInitLock);
			}

			string settingsJson = UnityEngine.JsonUtility.ToJson(RandomizerMod.RandomizerMod.Instance.Settings);
			ItemSync.Instance.Connection.UploadRandomizerSettings(settingsJson);
		}

		public void ApplyRandomizerSettings(string settingsJson)
        {
			bool originalNPCItemDialogue = RandomizerMod.RandomizerMod.Instance.Settings.NPCItemDialogue;
			RandomizerMod.RandomizerMod.Instance.Settings =
				UnityEngine.JsonUtility.FromJson<RandomizerMod.SaveSettings>(settingsJson);
			RandomizerMod.RandomizerMod.Instance.Settings.NPCItemDialogue = originalNPCItemDialogue;
			
			lock (waitForSettingsLock)
			{
				Monitor.Pulse(waitForSettingsLock);
			}
		}

        internal void Reset()
        {
			spoilerInitialized = false;
			shouldWaitForSettings = true;
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
