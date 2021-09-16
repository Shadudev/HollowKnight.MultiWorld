using System.Collections.Generic;
using UnityEngine;
using SereCore;
using HutongGames.PlayMaker.Actions;

namespace MultiWorldMod
{
    internal static class ObjectCache
    {
        public static AudioClip LurkerRoar;
        public static AudioClip FlukemarmRoar;
        public static AudioClip DungDefenderRoar;
        public static AudioClip CollectorRoar;
        public static AudioClip[] MarmuRoar = new AudioClip[3];
        public static AudioClip GorbRoar;

        public static void GetPrefabs(Dictionary<string, Dictionary<string, GameObject>> objectsByScene)
        {
            LurkerRoar = (AudioClip)objectsByScene[AdditionalFeatures.LURKER_SCENE]["Lurker Control"]
                .gameObject.transform.Find("Lurker Intro").gameObject.LocateMyFSM("Intro").GetState("Roar")
                .GetActionOfType<AudioPlayerOneShotSingle>().audioClip.Value;
            Object.DontDestroyOnLoad(LurkerRoar);

            FlukemarmRoar = (AudioClip)objectsByScene[AdditionalFeatures.FLUKEMARM_SCENE]["Fluke Mother"]
                .gameObject.LocateMyFSM("Fluke Mother").GetState("Roar Start")
                .GetActionOfType<AudioPlayerOneShotSingle>().audioClip.Value;
            Object.DontDestroyOnLoad(FlukemarmRoar);

            DungDefenderRoar = (AudioClip)objectsByScene[AdditionalFeatures.DUNG_DEFENDER_SCENE]["Dung Defender"]
                .gameObject.LocateMyFSM("Dung Defender").GetState("Rage Roar")
                .GetActionOfType<AudioPlayerOneShotSingle>().audioClip.Value;
            Object.DontDestroyOnLoad(DungDefenderRoar);

            CollectorRoar = (AudioClip)objectsByScene[AdditionalFeatures.COLLECTOR_SCENE]["Battle Scene"]
                .transform.Find("Jar Collector").gameObject.LocateMyFSM("Control").GetState("Roar")
                .GetActionOfType<AudioPlaySimple>().oneShotClip.Value;
            Object.DontDestroyOnLoad(CollectorRoar);

            try
            {
                MarmuRoar = objectsByScene[AdditionalFeatures.MARMU_SCENE]["Warrior"]
                    .transform.Find("Ghost Warrior Marmu").gameObject.LocateMyFSM("Control").GetState("Start Pause")
                    .GetActionOfType<AudioPlayerOneShot>().audioClips;
                foreach (var marmuRoar in MarmuRoar)
                    Object.DontDestroyOnLoad(marmuRoar);
            }
            catch (System.Exception e)
            {
                LogHelper.LogError(e.Message);
                LogHelper.LogError(e.StackTrace);
            }

            GorbRoar = (AudioClip)objectsByScene[AdditionalFeatures.GORB_SCENE]["Warrior"]
                .transform.Find("Ghost Warrior Slug").gameObject.LocateMyFSM("Attacking").GetState("Init")
            .GetActionOfType<AudioPlayerOneShotSingle>().audioClip.Value;
            Object.DontDestroyOnLoad(GorbRoar);
        }
    }
}
