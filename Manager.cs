using Snapshot_SilkSong.BattleState;
using Snapshot_SilkSong.EnemyState;
using Snapshot_SilkSong.PlayerState;
using Snapshot_SilkSong.SceneState;
using Snapshot_SilkSong.Utils;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Snapshot
{
    public class MemorySnapshot
    {
        public PlayerState playerState;
        public SceneState sceneState;
        public EnemyState enemyState;
        public BattleState battleState;
        public bool isActive;
        public DateTime timestamp;

        public MemorySnapshot()
        {
            playerState = new PlayerState();
            sceneState = new SceneState();
            enemyState = new EnemyState();
            battleState = new BattleState();
            isActive = false;
            timestamp = DateTime.Now;
        }
    }

    public class Manager
    {
        // 协程控制标志 - 改为实例字段
        private bool loadCoroutineRunning = false;
        private MemorySnapshot[] snapshot;

        public Manager()
        {
            // 正确初始化数组
            snapshot = new MemorySnapshot[9];

            // 初始化数组中的每个元素
            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i] = new MemorySnapshot();
            }

            // 创建存档文件夹
            var saveDir = Directory.CreateDirectory("snapshot_save");
            for (int i = 0; i < 9; i++)
            {
                Directory.CreateDirectory(Path.Combine(saveDir.FullName, i.ToString()));
            }
        }

        public void Save(string path)
        {
            Debug.Log("开始保存快照");
            StateController.IsFsmEnabled = false;
            ObjectFinder.EnsureDontDestroyOnLoadObject("", path);
            Debug.Log(path);

            try
            {
                int index = int.Parse(path);

                // 保存玩家状态
                PlayerState.SavePlayerState(snapshot[index].playerState, path);
                // 保存场景状态
                SceneState.SaveSceneState(snapshot[index].sceneState, path);
                // 保存战斗场景状态
                BattleState.SaveBattleState(snapshot[index].battleState, path);
                // 保存敌人状态
                EnemyState.SaveEnemyState(snapshot[index].enemyState, path);

                snapshot[index].isActive = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game state: {e.Message}");
            }

            GameManager.instance.StartCoroutine(EnableFsmAfterDelay());
            Debug.Log("结束保存快照");
        }

        private IEnumerator EnableFsmAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            StateController.IsFsmEnabled = true;
        }

        public void Load(string path)
        {
            int index = int.Parse(path);

            if (!loadCoroutineRunning && snapshot[index].isActive)
            {
                GameManager.instance.StartCoroutine(LoadCoroutine(path));
            }
        }

        private IEnumerator LoadCoroutine(string path)
        {
            loadCoroutineRunning = true;

            Debug.Log("开始还原快照");
            StateController.IsFsmEnabled = false;

            int index = int.Parse(path);

            yield return SceneState.LoadSceneStateCoroutine(snapshot[index].sceneState, path);

            PlayerState.LoadPlayerState(snapshot[index].playerState, path);
            BattleState.LoadBattleState(snapshot[index].battleState, path);
            EnemyState.LoadEnemyState(snapshot[index].enemyState, path);

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

            Debug.Log("结束还原快照");
        }
    }
}