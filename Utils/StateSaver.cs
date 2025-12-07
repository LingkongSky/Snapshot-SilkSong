using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;

namespace Snapshot_SilkSong.Utils
{
    // --- FSM 状态数据结构 ---
    [System.Serializable]
    public class FsmSnapshot
    {
        public string fsmName;
        public string activeStateName;
        public List<FsmVarData> variables = new List<FsmVarData>();
    }

    [System.Serializable]
    public class FsmVarData
    {
        public string name;
        public VariableType type;
        public object value; // 注意：JsonUtility 无法序列化 object，如果需要存盘到文件，需要自行编写序列化逻辑。内存中保存没问题。
    }


    [System.Serializable]
    public class LayerSnapshot
    {
        public int layerIndex;
        public int stateHash;
        public float normalizedTime;
        public float weight;
    }

    [System.Serializable]
    public class ParamSnapshot
    {
        public string name;
        public AnimatorControllerParameterType type;
        public object value;
    }

    // --- 工具类：处理保存与恢复逻辑 ---
    public static class StateSaverUtils
    {
        // === FSM 处理 ===
        public static List<FsmSnapshot> SaveFsmState(GameObject target)
        {
            List<FsmSnapshot> snapshots = new List<FsmSnapshot>();
            PlayMakerFSM[] fsms = target.GetComponents<PlayMakerFSM>();

            foreach (var fsm in fsms)
            {
                FsmSnapshot snap = new FsmSnapshot();
                snap.fsmName = fsm.FsmName;
                snap.activeStateName = fsm.ActiveStateName;

                // 保存关键类型的变量 (Float, Int, Bool, String)
                // 这里只演示了基础类型，Vector3/GameObject等需要根据需求添加
                foreach (var floatVar in fsm.FsmVariables.FloatVariables)
                    snap.variables.Add(new FsmVarData { name = floatVar.Name, type = VariableType.Float, value = floatVar.Value });

                foreach (var intVar in fsm.FsmVariables.IntVariables)
                    snap.variables.Add(new FsmVarData { name = intVar.Name, type = VariableType.Int, value = intVar.Value });

                foreach (var boolVar in fsm.FsmVariables.BoolVariables)
                    snap.variables.Add(new FsmVarData { name = boolVar.Name, type = VariableType.Bool, value = boolVar.Value });

                foreach (var strVar in fsm.FsmVariables.StringVariables)
                    snap.variables.Add(new FsmVarData { name = strVar.Name, type = VariableType.String, value = strVar.Value });

                snapshots.Add(snap);
            }
            return snapshots;
        }

        public static void RestoreFsmState(GameObject target, List<FsmSnapshot> snapshots)
        {
            if (snapshots == null) return;
            PlayMakerFSM[] fsms = target.GetComponents<PlayMakerFSM>();

            foreach (var snap in snapshots)
            {
                foreach (var fsm in fsms)
                {
                    if (fsm.FsmName == snap.fsmName)
                    {
                        // 1. 恢复变量
                        foreach (var v in snap.variables)
                        {
                            if (v.type == VariableType.Float) fsm.FsmVariables.FindFsmFloat(v.name).Value = (float)v.value;
                            else if (v.type == VariableType.Int) fsm.FsmVariables.FindFsmInt(v.name).Value = (int)v.value;
                            else if (v.type == VariableType.Bool) fsm.FsmVariables.FindFsmBool(v.name).Value = (bool)v.value;
                            else if (v.type == VariableType.String) fsm.FsmVariables.FindFsmString(v.name).Value = (string)v.value;
                        }

                        // 2. 强制切换状态
                        // 注意：这可能会触发该状态的 OnEnter Action，如果不想触发需要更复杂的反射操作
                        if (!string.IsNullOrEmpty(snap.activeStateName))
                        {
                            fsm.SetState(snap.activeStateName);
                        }
                        break;
                    }
                }
            }
        }

        }
    
}