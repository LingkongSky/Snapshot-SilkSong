using Snapshot_SilkSong.BattleState;
using Snapshot_SilkSong.EnemyState;
using Snapshot_SilkSong.PlayerState;
using Snapshot_SilkSong.SceneState;
using Snapshot_SilkSong.States;
using Snapshot_SilkSong.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Snapshot
{
    public class MemorySnapshot
    {
        public PlayerState playerState;
        public SceneState sceneState;
        public EnemyState enemyState;
        public BattleState battleState;
        public PersistentState persistentState;
        public LiftState liftState;
        public bool isActive;
        public DateTime timestamp;

        public MemorySnapshot()
        {
            playerState = new PlayerState();
            sceneState = new SceneState();
            enemyState = new EnemyState();
            battleState = new BattleState();
            persistentState = new PersistentState();
            liftState = new LiftState();
            isActive = false;
            timestamp = DateTime.Now;
        }
    }

    public class Manager
    {
        private bool loadCoroutineRunning = false;
        private Dictionary<string, MemorySnapshot> snapshots;

        public Manager()
        {
            snapshots = new Dictionary<string, MemorySnapshot>();
        }

        public void Save(string slotName)
        {
            Debug.Log("Start to save the Snapshot");
            StateController.IsFsmEnabled = false;
            ObjectFinder.EnsureDontDestroyOnLoadObject("", slotName);
            Debug.Log(slotName);

            try
            {
                // 检查并创建快照实例
                if (!snapshots.ContainsKey(slotName))
                {
                    snapshots[slotName] = new MemorySnapshot();
                }



                // 保存玩家状态
                PlayerState.SavePlayerState(snapshots[slotName].playerState, slotName);
                // 保存场景状态
                SceneState.SaveSceneState(snapshots[slotName].sceneState, slotName);
                // 保存战斗场景状态
                BattleState.SaveBattleState(snapshots[slotName].battleState, slotName);
                // 保存敌人状态
                EnemyState.SaveEnemyState(snapshots[slotName].enemyState, slotName);
                // 保存持久化状态
                PersistentState.SavePersistentState(snapshots[slotName].persistentState, slotName);
                // 保存电梯状态
                //LiftState.SaveLiftState(snapshots[slotName].liftState, slotName);

                snapshots[slotName].isActive = true;
                snapshots[slotName].timestamp = DateTime.Now;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game state: {e.Message}");
            }

            GameManager.instance.StartCoroutine(EnableFsmAfterDelay());
            Debug.Log("Over saved");
        }

        private IEnumerator EnableFsmAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            StateController.IsFsmEnabled = true;
        }

        public void Load(string slotName)
        {
            if (!loadCoroutineRunning && snapshots.ContainsKey(slotName) && snapshots[slotName].isActive)
            {
                GameManager.instance.StartCoroutine(LoadCoroutine(slotName));
            }
        }

        private IEnumerator LoadCoroutine(string slotName)
        {
            loadCoroutineRunning = true;

            Debug.Log("Start to load the Snapshot");
            StateController.IsFsmEnabled = false;

            yield return SceneState.LoadSceneStateCoroutine(snapshots[slotName].sceneState, slotName);

            PlayerState.LoadPlayerState(snapshots[slotName].playerState, slotName);
            BattleState.LoadBattleState(snapshots[slotName].battleState, slotName);
            EnemyState.LoadEnemyState(snapshots[slotName].enemyState, slotName);
            PersistentState.LoadPersistentState(snapshots[slotName].persistentState, slotName);
            //LiftState.LoadLiftState(snapshots[slotName].liftState, slotName);

            // 延迟0.5秒后恢复FSM
            yield return new WaitForSeconds(0.5f);
            StateController.IsFsmEnabled = true;

            // 重置协程标志
            loadCoroutineRunning = false;

            // 触发UI更新
            ToolItemManager.SendEquippedChangedEvent(true);
            HeroController.instance.AddGeo(0);
            HeroController.instance.AddShards(0);
            HeroController.instance.AddSilk(0, false);
            HeroController.instance.AddHealth(0);

            Debug.Log("Over loaded");
        }
    }
}