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
        // 加载协程引用
        private static bool loadCoroutine;
        private static MemorySnapshot snapshot;


        public Manager()
        {
            snapshot = new MemorySnapshot();

            // 创建存档文件夹
            var saveDir = Directory.CreateDirectory("snapshot_save");
            for (int i = 0; i < 8; i++)
            {
                Directory.CreateDirectory(Path.Combine(saveDir.FullName, i.ToString()));
            }
        }


        public void Save(String path)
        {
            Debug.Log("开始保存快照");

            StateController.IsFsmEnabled = false;

            try
            {
                // 保存玩家状态
                PlayerState.SavePlayerState(snapshot.playerState, path);
                // 保存场景状态
                SceneState.SaveSceneState(snapshot.sceneState, path);
                // 保存战斗场景状态
                BattleState.SaveBattleState(snapshot.battleState, path);
                // 保存敌人状态
                EnemyState.SaveEnemyState(snapshot.enemyState, path);

                snapshot.isActive = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game state: {e.Message}");
            }

            GameManager.instance.StartCoroutine(EnableFsmAfterDelay());
            Debug.Log("结束保存快照");
        }


        private static IEnumerator EnableFsmAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            StateController.IsFsmEnabled = true;
        }

        public void Load(String path)
        {
            if (!loadCoroutine && snapshot.isActive)
            {
                GameManager.instance.StartCoroutine(LoadCoroutine(path));
            }
        }

        private static IEnumerator LoadCoroutine(String path)
        {
            Debug.Log("开始还原快照");
            StateController.IsFsmEnabled = false;

            yield return SceneState.LoadSceneStateCoroutine(snapshot.sceneState, path);

            PlayerState.LoadPlayerState(snapshot.playerState, path);
            BattleState.LoadBattleState(snapshot.battleState, path);
            EnemyState.LoadEnemyState(snapshot.enemyState, path);


            // 延迟0.5秒后恢复FSM
            yield return new WaitForSeconds(0.5f);
            StateController.IsFsmEnabled = true;
            loadCoroutine = false;

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