using Modding;
using SereCore;
using System;
using UnityEngine.SceneManagement;

namespace MultiWorld
{
	public class MultiWorld : Mod
	{
		public SaveSettings Settings { get; set; } = new SaveSettings();
		public override ModSettings SaveSettings
		{
			get => Settings = Settings ?? new SaveSettings();
			set => Settings = value is SaveSettings saveSettings ? saveSettings : Settings;
		}
		public static MultiWorld Instance
		{
			get; private set;
		}

		public static bool IsRando
		{
			get
			{
				return RandomizerMod.RandomizerMod.Instance.Settings.Randomizer;
			}
		}

		public override string GetVersion()
		{
			string ver = "0.0.1";
			return ver;
		}

		public override void Initialize()
		{
			if (Instance != null)
			{
				LogWarn("Initialized twice... Stop that.");
				return;
			}
			Instance = this;
			LogDebug("MultiWorld Initializing...");

			if (!DoesLoadedRandoSupportMW())
			{
				LogWarn("Loaded rando doesn't support multiworld, not doing a thing.");
			} 
			else
            {
				UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnMainMenu;
			}

		}

		private bool DoesLoadedRandoSupportMW()
		{
			try
			{
				Type[] types = typeof(RandomizerMod.RandomizerMod).GetInterfaces();
				return Array.Exists<Type>(types, type => type == typeof(RandomizerMod.MultiWorld.IMultiWorldCompatibleRandomizer));
			}
			catch (TypeLoadException e)
            {
				// Old RandomizerMod version (pre RandomizerMod.MultiWorld.IMultiWorldCompatibleRandomizer commit)
				return false;
			}
			catch (Exception e)
			{
				LogWarn("Failed to check for loaded Randomizer MultiWorld support: " + e.Message);
				return false;
			}
		}

		private void OnMainMenu(Scene from, Scene to)
		{
			if (Ref.GM.GetSceneNameString() == SceneNames.Menu_Title)
            {
                MenuChanger.AddMultiWorldMenu();
			}
		}
    }
}