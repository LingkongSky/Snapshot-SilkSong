using System;
using System.Collections;
using UnityEngine;
using static GameManager;

namespace Snapshot_SilkSong.SceneState
{
    public class SceneState
    {
        public String currentScene;
        public String entryGate;
        public UnityEngine.Vector3 position;

        public static void SaveSceneState(SceneState sceneState, String path)
        {
            GameManager gameData = GameManager.instance;
            HeroController heroData = HeroController.instance;

            sceneState.currentScene = gameData.GetSceneNameString();
            sceneState.entryGate = gameData.GetEntryGateName();
            sceneState.position = heroData.transform.position;
        }

        public static IEnumerator LoadSceneStateCoroutine(SceneState sceneState, String path)
        {
            GameManager gameData = GameManager.instance;
            HeroController heroData = HeroController.instance;

            if (heroData == null || gameData == null || sceneState == null)
            {
                Debug.LogError($"Failed to teleport: Data Null");
                yield break;
            }

            string currentScene = gameData.sceneName;
    
            if (currentScene != sceneState.currentScene)
            {
                yield return WaitForSceneLoadAndTeleport(sceneState.currentScene,sceneState.position);
            }
            else
            {
                // 同一场景,直接传送
                heroData.transform.position = sceneState.position;
                yield return null;
            }
        }


        private static IEnumerator WaitForSceneLoadAndTeleport(String targetScene, UnityEngine.Vector3 targetPosition)
        {

            SceneLoadInfo info = new GameManager.SceneLoadInfo
            {

                SceneName = targetScene,
                EntryGateName = "left1",
                HeroLeaveDirection = GlobalEnums.GatePosition.unknown,
                EntryDelay = 0f,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                AlwaysUnloadUnusedAssets = true,
                EntrySkip = true // 处理场景进入动画
            };

            GameManager.instance.BeginSceneTransition(info);

            yield return new WaitWhile(() =>
            {
                var gm = GameManager.instance;
                var hc = HeroController.instance;
                if (gm == null || hc == null) return true;
                return gm.IsInSceneTransition || !hc.isHeroInPosition || hc.cState.transitioning;
            });

            yield return new WaitUntil(() =>
            {
                var hc = HeroController.instance;
                return hc != null && hc.CanInput();
            });

            // 传送到目标位置
            if (targetPosition != UnityEngine.Vector3.zero && HeroController.instance != null)
            {
                HeroController.instance.transform.position = targetPosition;
            }
        }
    }
}