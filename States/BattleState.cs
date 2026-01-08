using Snapshot_SilkSong.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.BattleState
{
    /*
    路径：DontDestroyOnLoad/1/BattleState/
    */
    public class BattleState
    {
        public List<ObjectInfo> BattleSceneList;

        public BattleState()
        {
            BattleSceneList = new List<ObjectInfo>();
        }

        // 保存战斗场景状态
        public static void SaveBattleState(BattleState battleState, String path)
        {
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "BattleStates");

            // 清理旧数据
            foreach (ObjectInfo battleScene in battleState.BattleSceneList)
            {
                if (battleScene.targetObject != null)
                    GameObject.DestroyImmediate(battleScene.targetObject);
            }

            battleState.BattleSceneList.Clear();

            // 获取当前场景需要保存的对象
            List<ObjectInfo> tempBattleScenes = FindBattleScene();

            if (tempBattleScenes == null || tempBattleScenes.Count == 0) return;

            foreach (ObjectInfo obj in tempBattleScenes)
            {
                GameObject originalObj = obj.targetObject;
                GameObject clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "BattleStates/").transform);
                ObjectFinder.DeleteHealthManagerImmediate(clone);
                clone.SetActive(false);
                clone.name = originalObj.name;
                ObjectInfo newInfo = new ObjectInfo(clone, obj.path, originalObj.scene.name, obj.isActive, originalObj.transform);
                battleState.BattleSceneList.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);

        }

        // 恢复战斗场景状态
        public static void LoadBattleState(BattleState battleState, String path)
        {
            // 清理当前场景中已存在的同类对象
            List<ObjectInfo> currentSceneObjects = FindBattleScene();
            foreach (ObjectInfo obj in currentSceneObjects)
            {
                if (obj.targetObject != null)
                    GameObject.DestroyImmediate(obj.targetObject);
            }

            if (battleState.BattleSceneList == null || battleState.BattleSceneList.Count == 0) return;

            // 从存档列表恢复
            foreach (ObjectInfo savedInfo in battleState.BattleSceneList)
            {
                if (savedInfo.targetObject == null)
                {
                    continue;
                }

                GameObject clone = GameObject.Instantiate(savedInfo.targetObject);
                clone.name = savedInfo.targetObject.name;

                // 恢复父级结构
                ObjectFinder.PlaceGameObjectToPath(clone, savedInfo.path, savedInfo.sceneName);

                clone.transform.localPosition = savedInfo.savedLocalPosition;
                clone.transform.localRotation = savedInfo.savedLocalRotation;
                clone.transform.localScale = savedInfo.savedLocalScale;
                clone.SetActive(true);
            }
        }

        public static List<ObjectInfo> FindBattleScene()
        {
            List<ObjectInfo> battleSceneObjects = new List<ObjectInfo>();

            // 遍历所有已加载的场景
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.name == "DontDestroyOnLoad" ||
                    scene.name == "HideAndDontSave")
                {
                    continue;
                }

                BattleScene[] allComponents = GameObject.FindObjectsByType<BattleScene>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                foreach (BattleScene component in allComponents)
                {
                    if (component == null) continue;

                    GameObject obj = component.gameObject;

                    if (obj.scene != scene)
                    {
                        continue;
                    }

                    string path = ObjectFinder.GetGameObjectPath(obj);

                    if (path.Contains("Boss Scene"))
                    {
                        continue;
                    }

                    battleSceneObjects.Add(new ObjectInfo(obj, path, obj.scene.name, obj.activeSelf, obj.transform));
                }
            }
            return battleSceneObjects;
        }

    }
}