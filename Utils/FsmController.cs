using HarmonyLib;
using HutongGames.PlayMaker;
using System.Collections;
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

            // 为其他类型的方法创建通用补丁方法
            private static bool Prefix_OtherType(string typeName, string methodName)
            {
                string fullMethodName = $"{typeName}.{methodName}";
                bool shouldExecute = StateController.ShouldExecute();
                //Debug.Log($"{StateController.counter++} [Patch] {methodName}  执行状态: {shouldExecute}");
                return shouldExecute;
            }


            [HarmonyPatch(typeof(PlayMakerFSM), "OnDisable"), HarmonyPrefix] // 避免Disable时直接杀死实体
            static bool Prefix_PlayMakerFSM_OnDisable() => Prefix_OtherType("PlayMakerFSM", "OnDisable");

            [HarmonyPatch(typeof(PlayMakerFSM), "OnEnable"), HarmonyPrefix] // 避免Enable时实体重置状态
            static bool Prefix_Fsm_OnEnable() => Prefix_OtherType("Fsm", "OnEnable");


            [HarmonyPatch(typeof(PlayMakerFSM), "AddEventHandlerComponents"), HarmonyPrefix] // 避免Enable时实体重置状态
            static bool Prefix_Fsm_AddEventHandlerComponents() => Prefix_OtherType("Fsm", "AddEventHandlerComponents");

            [HarmonyPatch(typeof(PlayMakerFSM), "Start"), HarmonyPrefix] // 避免Enable时实体重置状态
            static bool Prefix_PlayMakerFSM_Awake() => Prefix_OtherType("PlayMakerFSM", "Start");

            [HarmonyPatch(typeof(HealthManager), "Awake"), HarmonyPrefix] // 避免TagDamager重复生成     
            static bool Prefix_PlayMakerFSM_Awake(HealthManager __instance) {
                TagDamageTaker tagDamageTaker = __instance.GetComponent<TagDamageTaker>();
                UnityEngine.Object.Destroy(tagDamageTaker);
                return true;
            }

            [HarmonyPatch(typeof(HealthManager), "HealToMax"), HarmonyPrefix]  //  防止实例化触发Awake时怪物血量回满        
            static bool PrefixHealthManager_HealToMax() => Prefix_OtherType("HealthManager", "HealToMax");

            [HarmonyPatch(typeof(HealthManager), "AddPhysicalPusher"), HarmonyPrefix] // 用于防止怪物尸体重复生成
            static bool Prefix_HealthManager_AddPhysicalPusher() => Prefix_OtherType("HealthManager", "AddPhysicalPusher");
            
        }
    }
}