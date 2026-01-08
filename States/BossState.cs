using Snapshot_SilkSong.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.BossState
{

    /*
    路径：DontDestroyOnLoad/1/BossState/
    */
    public class BossState
    {
        public List<ObjectInfo> BossSceneList;

        public BossState()
        {
            BossSceneList = new List<ObjectInfo>();
        }

        // 保存Boss场景状态
        public static void SaveBossState(BossState bossState, String path)
        {
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "BossStates");

            // 清理旧数据
            foreach (ObjectInfo bossScene in bossState.BossSceneList)
            {
                if (bossScene.targetObject != null)
                    GameObject.DestroyImmediate(bossScene.targetObject);
            }

            bossState.BossSceneList.Clear();

            // 获取当前场景需要保存的对象
            List<ObjectInfo> tempBossScenes = FindBossScene();

            if (tempBossScenes == null || tempBossScenes.Count == 0) return;

            foreach (ObjectInfo obj in tempBossScenes)
            {
                GameObject originalObj = obj.targetObject;
                GameObject clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "BossStates/").transform);
                ObjectFinder.DeleteHealthManagerImmediate(clone);
                clone.SetActive(false);
                clone.name = originalObj.name;
                ObjectInfo newInfo = new ObjectInfo(clone, obj.path, originalObj.scene.name, obj.isActive, originalObj.transform);
                bossState.BossSceneList.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);
        }

        // 恢复Boss场景状态
        public static void LoadBossState(BossState bossState, String path)
        {
            // 清理当前场景中已存在的同名对象
            List<ObjectInfo> currentSceneObjects = FindBossScene();
            foreach (ObjectInfo obj in currentSceneObjects)
            {
                if (obj.targetObject != null)
                    GameObject.DestroyImmediate(obj.targetObject);
            }

            if (bossState.BossSceneList == null || bossState.BossSceneList.Count == 0) return;

            // 从存档列表恢复
            foreach (ObjectInfo savedInfo in bossState.BossSceneList)
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

        public static List<ObjectInfo> FindBossScene()
        {
            List<ObjectInfo> bossSceneObjects = new List<ObjectInfo>();

            // 遍历所有已加载的场景
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.name == "DontDestroyOnLoad" ||
                    scene.name == "HideAndDontSave")
                {
                    continue;
                }

                // 查找场景中所有游戏对象
                GameObject[] allGameObjects = scene.GetRootGameObjects();

                foreach (GameObject rootObj in allGameObjects)
                {
                    // 查找所有名为"Boss Scene"的对象（包括子对象）
                    Transform[] allTransforms = rootObj.GetComponentsInChildren<Transform>(true);

                    foreach (Transform transform in allTransforms)
                    {
                        if (transform.gameObject.name == "Boss Scene")
                        {
                            GameObject obj = transform.gameObject;
                            string path = ObjectFinder.GetGameObjectPath(obj);
                            Debug.Log("Found Boss Scene: " + path);
                            bossSceneObjects.Add(new ObjectInfo(obj, path, obj.scene.name, obj.activeSelf, obj.transform));
                        }
                    }
                }
            }
            return bossSceneObjects;
        }
    }
}