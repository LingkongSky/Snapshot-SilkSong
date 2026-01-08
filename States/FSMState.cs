using HutongGames.PlayMaker;
using Snapshot;
using Snapshot_SilkSong.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Snapshot_SilkSong.States
{
    public class FSMState
    {
        public static void SaveFSMState(MemorySnapshot memorySnapshot)
        {
            List<ObjectInfo> enemyStateList = memorySnapshot.enemyState.enemyList;

            foreach (ObjectInfo enemyInfo in enemyStateList)
            {
                string fullPath = enemyInfo.sceneName + "/" + enemyInfo.path;
                GameObject targetObject = enemyInfo.targetObject;

                if (targetObject == null)
                {
                    Debug.LogWarning($"TargetObject is null for path: {fullPath}");
                    continue;
                }

                GameObject source = ObjectFinder.FindGameObjectByPath(enemyInfo.sceneName, enemyInfo.path);

                if (source != null)
                {
                    SyncAllFSMs(source, targetObject);
                    SynciTweens(source, targetObject);
                }
                else
                {
                    Debug.LogWarning($"Cannot find source GameObject for path: {fullPath}");
                }
            }
        }

        public static void LoadFSMState(MemorySnapshot memorySnapshot)
        {
            List<ObjectInfo> enemyStateList = memorySnapshot.enemyState.enemyList;

            foreach (ObjectInfo enemyInfo in enemyStateList)
            {
                string fullPath = enemyInfo.sceneName + "/" + enemyInfo.path;
                GameObject targetObject = enemyInfo.targetObject;

                if (targetObject == null)
                {
                    continue;
                }

                GameObject source = ObjectFinder.FindGameObjectByPath(enemyInfo.sceneName, enemyInfo.path);

                if (source != null)
                {
                    SyncAllFSMs(targetObject, source);
                    SynciTweens(targetObject, source);

                }
                else
                {
                    Debug.LogWarning($"Cannot find source GameObject for path: {fullPath}");
                }
            }


        }


        // FSM同步
        private static void CopyFsmVariables(FsmVariables source, FsmVariables target)
        {
            var srcFloats = source.FloatVariables;
            var dstFloats = target.FloatVariables;
            for (int i = 0; i < srcFloats.Length; i++) dstFloats[i].Value = srcFloats[i].Value;

            var srcInts = source.IntVariables;
            var dstInts = target.IntVariables;
            for (int i = 0; i < srcInts.Length; i++) dstInts[i].Value = srcInts[i].Value;

            var srcBools = source.BoolVariables;
            var dstBools = target.BoolVariables;
            for (int i = 0; i < srcBools.Length; i++) dstBools[i].Value = srcBools[i].Value;

            var srcStrings = source.StringVariables;
            var dstStrings = target.StringVariables;
            for (int i = 0; i < srcStrings.Length; i++) dstStrings[i].Value = srcStrings[i].Value;

            var srcVec3 = source.Vector3Variables;
            var dstVec3 = target.Vector3Variables;
            for (int i = 0; i < srcVec3.Length; i++) dstVec3[i].Value = srcVec3[i].Value;
        }

        private static void SyncAllFSMs(GameObject source, GameObject target)
        {
            PlayMakerFSM[] sourceFSMs = source.GetComponents<PlayMakerFSM>();
            PlayMakerFSM[] targetFSMs = target.GetComponents<PlayMakerFSM>();

            for (int i = 0; i < sourceFSMs.Length; i++)
            {
                if (i >= targetFSMs.Length) break;

                var srcFsm = sourceFSMs[i].Fsm;
                var dstFsm = targetFSMs[i].Fsm;

                CopyFsmVariables(srcFsm.Variables, dstFsm.Variables);
                string activeStateName = srcFsm.ActiveStateName;

                if (!string.IsNullOrEmpty(activeStateName))
                {
                    dstFsm.StartState = activeStateName;
                }
            }
        }


        private static void SynciTweens(GameObject source, GameObject target)
        {
            iTween[] sourceTweens = source.GetComponents<iTween>();
            iTween[] targetTweens = target.GetComponents<iTween>();

            for (int i = 0; i < sourceTweens.Length; i++)
            {
                if (i >= targetTweens.Length) break;

                // 同步参数
                SynciTweenParams(sourceTweens[i], targetTweens[i]);
            }
        }

        private static void SynciTweenParams(iTween source, iTween target)
        {
            // 使用反射获取私有字段
            var fields = typeof(iTween).GetFields(System.Reflection.BindingFlags.Instance |
                                                  System.Reflection.BindingFlags.NonPublic);

            // 关键字段列表
            string[] keyFields = { "time", "delay", "percentage", "namedcolorvalue",
                          "loopType", "easeType", "space", "isLocal", "useRealTime",
                          "physics", "reverse", "loop" };

            foreach (var fieldName in keyFields)
            {
                var field = System.Array.Find(fields, f => f.Name == fieldName);
                if (field != null)
                {
                    try
                    {
                        var value = field.GetValue(source);
                        field.SetValue(target, value);
                    }
                    catch { }
                }
            }

            // 同步 tweenArguments
            var argsField = typeof(iTween).GetField("tweenArguments",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (argsField != null)
            {
                var srcArgs = argsField.GetValue(source) as System.Collections.Hashtable;
                if (srcArgs != null)
                {
                    // 创建新的 Hashtable 并复制值类型和字符串
                    var dstArgs = new System.Collections.Hashtable();
                    foreach (System.Collections.DictionaryEntry entry in srcArgs)
                    {
                        if (entry.Value == null)
                        {
                            dstArgs[entry.Key] = null;
                            continue;
                        }

                        var valType = entry.Value.GetType();
                        if (valType.IsValueType || valType == typeof(string))
                        {
                            dstArgs[entry.Key] = entry.Value;
                        }
                        else if (entry.Value is UnityEngine.Object unityObj)
                        {
                            // Unity 对象保持原引用
                            dstArgs[entry.Key] = unityObj;
                        }
                    }
                    argsField.SetValue(target, dstArgs);
                }
            }
            // 获取状态字段
            var isRunningField = typeof(iTween).GetField("isRunning",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var percentageField = typeof(iTween).GetField("percentage",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var runningField = typeof(iTween).GetField("running",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (isRunningField == null || percentageField == null) return;

            bool sourceRunning = (bool)isRunningField.GetValue(source);
            float sourcePercentage = (float)percentageField.GetValue(source);

            // 同步运行状态和进度
            isRunningField.SetValue(target, sourceRunning);
            percentageField.SetValue(target, sourcePercentage);

            if (runningField != null)
            {
                runningField.SetValue(target, sourceRunning);
            }
        }

    }
}