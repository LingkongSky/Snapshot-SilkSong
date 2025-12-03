using Snapshot_SilkSong.BattleState;
using Snapshot_SilkSong.EnemyState;
using Snapshot_SilkSong.PlayerState;
using Snapshot_SilkSong.SceneState;
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

    // 处理BattleScene的Gate无法正常保存的问题
    // 处理Boss对象无法正常保存的问题
    
    public class Manager
    {
        //private static Dictionary<string, MemorySnapshot> savedSnapshots = new Dictionary<string, MemorySnapshot>();

        // 当前存档名称
        // private static string currentSaveName = "default";

        // 加载协程引用
        private static bool loadCoroutine;

        private static MemorySnapshot snapshot;


        public Manager() {
            snapshot = new MemorySnapshot();

            // 创建存档文件夹
            var saveDir = Directory.CreateDirectory("snapshot_save");
            for (int i = 0; i < 8; i++) {
                Directory.CreateDirectory(Path.Combine(saveDir.FullName, i.ToString()));
            }
        }


        public void Save(String path)
        {
            Debug.Log("开始保存快照");
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
                // 保存NPC状态
                //SaveNPCStates(snapshot.npcStates);

                // 保存到存档字典
                //savedSnapshots[currentSaveName] = snapshot;

                //Debug.Log($"Game state saved to memory: {currentSaveName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game state: {e.Message}");
            }
            Debug.Log("结束保存快照");

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
            /*
               if (!savedSnapshots.ContainsKey(currentSaveName))
               {
                   yield break;
               }

               MemorySnapshot snapshot = savedSnapshots[currentSaveName];
            */

            Debug.Log("开始还原快照");

            yield return SceneState.LoadSceneStateCoroutine(snapshot.sceneState, path);

            PlayerState.LoadPlayerState(snapshot.playerState, path);

            BattleState.LoadBattleState(snapshot.battleState, path);

            EnemyState.LoadEnemyState(snapshot.enemyState, path);

            // 恢复NPC状态
            //yield return LoadNPCStatesCoroutine(snapshot.npcStates);

            loadCoroutine = false;

            Debug.Log("结束还原快照");

        }



    }
}