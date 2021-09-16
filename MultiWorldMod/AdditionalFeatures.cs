using HutongGames.PlayMaker;
using SereCore;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiWorldMod
{
    class AdditionalFeatures
    {
        public const string LURKER_SCENE = "GG_Lurker";
        public const string FLUKEMARM_SCENE = "Waterways_12_boss";
        public const string DUNG_DEFENDER_SCENE = "Waterways_05_boss";
        public const string COLLECTOR_SCENE = "Ruins2_11_boss";
        public const string MARMU_SCENE = "Fungus3_40_boss";
        public const string GORB_SCENE = "Cliffs_02_boss";

        private const string LURKER_ROAR_STRING = "Lurker_Scream";
        private const string FLUKEMARM_ROAR_STRING = "Flukemarm_Scream";
        private const string DUNG_DEFENDER_ROAR_STRING = "Dung_Defender_Scream";
        private const string COLLECTOR_ROAR_STRING = "Collector_Scream";
        private const string MARMU_ROAR_STRING = "Marmu_Scream";
        private const string GORB_ROAR_STRING = "Gorb_Scream";

        readonly List<Action<Scene, Scene>> sceneTriggeredFeatures;

        class AdditionalFeatureAction : FsmStateAction
        {
            private readonly Action _method;

            public AdditionalFeatureAction(Action method)
            {
                _method = method;
            }

            public override void OnEnter()
            {
                try
                {
                    _method();
                }
                catch (Exception e)
                {
                    LogError("Error in MAction:\n" + e);
                }

                Finish();
            }
        }

        internal AdditionalFeatures()
        {
            sceneTriggeredFeatures = new List<Action<Scene, Scene>>
            {
                SyncScreamsByScene, 
                // Uncomment for objects dump per scene (from, to) => { LogHelper.Log($"Dumping {to.name} objects"); foreach (GameObject o in to.GetRootGameObjects()) LogHelper.Log(o); }
            };

            void AddScreamMeta(string screamString)
            {
                RandomizerMod.LanguageStringManager.SetString("UI", screamString, "");
                RandomizerMod.Randomization.LogicManager.EditItemDef(screamString, new RandomizerMod.Randomization.ReqDef
                {
                    action = RandomizerMod.GiveItemActions.GiveAction.None,
                    nameKey = screamString,
                });
            }
            AddScreamMeta(LURKER_ROAR_STRING);
            AddScreamMeta(FLUKEMARM_ROAR_STRING);
            AddScreamMeta(DUNG_DEFENDER_ROAR_STRING);
            AddScreamMeta(COLLECTOR_ROAR_STRING);
            AddScreamMeta(MARMU_ROAR_STRING);
            AddScreamMeta(GORB_ROAR_STRING);
        }

        private void SyncScreamsByScene(Scene from, Scene to)
        {
            switch (to.name)
            {
                case LURKER_SCENE:
                    GameObject.Find("Lurker Intro").LocateMyFSM("Intro").GetState("Roar").AddFirstAction(new AdditionalFeatureAction(() =>
                    {
                        LogHelper.Log("That's loud...");
                        if (new System.Random().Next(9) == 1)
                            ItemSync.Instance.Connection.SendItemToAll(LURKER_ROAR_STRING, LURKER_ROAR_STRING);
                    }));
                    break;
            }
        }

        private void SyncByEnabledFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);

            switch (self.gameObject.name)
            {
                case "Fluke Mother" when self.FsmName == "Fluke Mother":
                    self.GetState("Roar Start").AddFirstAction(new AdditionalFeatureAction(() =>
                    {
                        LogHelper.Log("Do I send this?");
                        if (new System.Random().Next(12) == 5)
                            ItemSync.Instance.Connection.SendItemToAll(FLUKEMARM_ROAR_STRING, FLUKEMARM_ROAR_STRING);
                    }));
                    break;
                case "Dung Defender" when self.FsmName == "Dung Defender":
                    self.GetState("First?").AddFirstAction(new AdditionalFeatureAction(() =>
                    {
                        LogHelper.Log("Should we send dung defender's scream?");
                        if (new System.Random().Next(9) == 6)
                            ItemSync.Instance.Connection.SendItemToAll(DUNG_DEFENDER_ROAR_STRING, DUNG_DEFENDER_ROAR_STRING);
                    }));
                    break;
                case "Jar Collector" when self.FsmName == "Control":
                    self.GetState("Roar").AddFirstAction(new AdditionalFeatureAction(() =>
                    {
                        LogHelper.Log("Should we send the collector's scream?");
                        if (new System.Random().Next(9) == 3)
                            ItemSync.Instance.Connection.SendItemToAll(DUNG_DEFENDER_ROAR_STRING, DUNG_DEFENDER_ROAR_STRING);
                    }));
                    break;
                case "Ghost Warrior Marmu" when self.FsmName == "Control":
                    self.GetState("Start Pause").AddFirstAction(new AdditionalFeatureAction(() =>
                    {
                        LogHelper.Log("It do be ballin'!");
                        if (new System.Random().Next(9) == 8)
                            ItemSync.Instance.Connection.SendItemToAll(MARMU_ROAR_STRING, MARMU_ROAR_STRING);
                    }));
                    break;
                case "Ghost Warrior Slug" when self.FsmName == "Attacking":
                    self.GetState("Init").AddFirstAction(new AdditionalFeatureAction(() =>
                    {
                        LogHelper.Log("Ascend!");
                        if (new System.Random().Next(9) == 7)
                            ItemSync.Instance.Connection.SendItemToAll(GORB_ROAR_STRING, GORB_ROAR_STRING);
                    }));
                    break;
            }
        }

        private static bool TryProcessSpecialPickup(RandomizerMod.GiveItemActions.GiveAction action,
            string itemName, string location, int geo)
        {
            switch (itemName)
            {
                case LURKER_ROAR_STRING:
                    PlayAudio(ObjectCache.LurkerRoar);
                    return true;
                case FLUKEMARM_ROAR_STRING:
                    PlayAudio(ObjectCache.FlukemarmRoar);
                    return true;
                case DUNG_DEFENDER_ROAR_STRING:
                    PlayAudio(ObjectCache.DungDefenderRoar);
                    return true;
                case COLLECTOR_ROAR_STRING:
                    PlayAudio(ObjectCache.CollectorRoar);
                    return true;
                case MARMU_ROAR_STRING:
                    PlayAudio(ObjectCache.MarmuRoar[new System.Random().Next(ObjectCache.MarmuRoar.Length)]);
                    return true;
                case GORB_ROAR_STRING:
                    PlayAudio(ObjectCache.GorbRoar);
                    return true;
            }

            return false;
        }

        private static void PlayAudio(AudioClip audioClip)
        {
            if (AreSoundsMuted()) return;

            AudioSource.PlayClipAtPoint(audioClip, new Vector3(
                Camera.main.transform.position.x - 2,
                Camera.main.transform.position.y,
                Camera.main.transform.position.z + 2),
                GameManager.instance.gameSettings.masterVolume * GameManager.instance.gameSettings.soundVolume);
            AudioSource.PlayClipAtPoint(audioClip, new Vector3(
                Camera.main.transform.position.x + 2,
                Camera.main.transform.position.y,
                Camera.main.transform.position.z + 2),
                GameManager.instance.gameSettings.masterVolume * GameManager.instance.gameSettings.soundVolume);
        }

        private static bool AreSoundsMuted()
        {
            return GameManager.instance.gameSettings.masterVolume == 0 || GameManager.instance.gameSettings.soundVolume == 0;
        }

        internal static IEnumerable<(string, string)> GetPreloadNames()
        {
            List<(string, string)> preloads = new List<(string, string)>
            {
                (LURKER_SCENE, "Lurker Control"),
                (FLUKEMARM_SCENE, "Fluke Mother"),
                (DUNG_DEFENDER_SCENE, "Dung Defender"),
                (COLLECTOR_SCENE, "Battle Scene"),
                (MARMU_SCENE, "Warrior"),
                (GORB_SCENE, "Warrior")
            };

            return preloads;
        }

        internal void Hook()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += HandleSceneChanges;
            On.PlayMakerFSM.OnEnable += SyncByEnabledFSM;
            RandomizerMod.GiveItemActions.ExternItemHandlers.Add(TryProcessSpecialPickup);
        }

        internal void Unhook()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= HandleSceneChanges;
            On.PlayMakerFSM.OnEnable -= SyncByEnabledFSM;
            if (RandomizerMod.GiveItemActions.ExternItemHandlers.Contains(TryProcessSpecialPickup))
                RandomizerMod.GiveItemActions.ExternItemHandlers.Remove(TryProcessSpecialPickup);
        }

        private void HandleSceneChanges(Scene from, Scene to)
        {
            foreach (var feature in sceneTriggeredFeatures)
            {
                try
                {
                    feature.Invoke(from, to);
                }
                catch (Exception e)
                {
                    LogHelper.LogError($"M: Error processing scene change `{from}`->`{to}`: {e.Message}");
                    LogHelper.LogError(e.StackTrace);
                }
            }
        }
    }
}
