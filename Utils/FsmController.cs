using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Snapshot_SilkSong.Utils
{
    // 控制器类
    public static class StateController
    {
        public static bool IsFsmEnabled = true;
        public static bool ShouldExecute() => IsFsmEnabled;

        public static long counter = 0;
    }

    namespace Snapshot_SilkSong.Patches
    {
        [HarmonyPatch]
        public class StatePatches
        {
            // 通用的前缀拦截方法
            private static bool CommonPrefix(string methodName)
            {
                bool shouldExecute = StateController.ShouldExecute();
                Debug.Log($"{StateController.counter++} [Patch] {methodName}  执行状态: {shouldExecute}");
                return shouldExecute;
            }

            // 为PlayMakerFSM方法创建通用补丁方法
            private static bool Prefix_PlayMakerFSM(string __originalMethodName)
            {
                string methodName = $"PlayMakerFSM.{__originalMethodName}";
                return CommonPrefix(methodName);
            }

            // 为其他类型的方法创建通用补丁方法
            private static bool Prefix_OtherType(string typeName, string methodName)
            {
                string fullMethodName = $"{typeName}.{methodName}";
                return CommonPrefix(fullMethodName);
            }


            [HarmonyPatch(typeof(PlayMakerFSM), "OnDisable"), HarmonyPrefix]
            static bool Prefix_ReEnterState() => Prefix_PlayMakerFSM("OnDisable");

            [HarmonyPatch(typeof(PlayMakerFSM), "OnEnable"), HarmonyPrefix]
            static bool Prefix_SwitchState() => Prefix_PlayMakerFSM("OnEnable");

            [HarmonyPatch(typeof(BattleScene), "OnDisable"), HarmonyPrefix]
            static bool Prefix_BattleScene() => Prefix_OtherType("BattleScene", "OnDisable");


            [HarmonyPatch(typeof(HealthManager), "AddPhysicalPusher"), HarmonyPrefix] // 用于处理怪物尸体
            static bool Prefix_AddPhysicalPusher() => Prefix_OtherType("HealthManager", "AddPhysicalPusher");
        }
    }
}