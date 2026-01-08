using Snapshot_SilkSong.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.BossState
{
    [System.Serializable]
    public class BossInfo
    {
        public GameObject targetObject;
        public string path;
        public string sceneName;

        public Vector3 savedLocalPosition;
        public Quaternion savedLocalRotation;
        public Vector3 savedLocalScale;

        public BossInfo(GameObject gameObject, string path, string sceneName, Transform originalTransform)
        {
            this.targetObject = gameObject;
            this.path = path;
            this.sceneName = sceneName;

            // 从原始 Transform 中记录局部信息
            this.savedLocalPosition = originalTransform.localPosition;
            this.savedLocalRotation = originalTransform.localRotation;
            this.savedLocalScale = originalTransform.localScale;
        }
    }
    /*
    路径：DontDestroyOnLoad/1/BossState/
    */
    public class BossState
    {
        public List<BossInfo> BossSceneList;

        public BossState()
        {
            BossSceneList = new List<BossInfo>();
        }

        // 保存Boss场景状态
        public static void SaveBossState(BossState bossState, String path)
        {
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "BossStates");

            // 清理旧数据
            foreach (BossInfo bossScene in bossState.BossSceneList)
            {
                if (bossScene.targetObject != null)
                    GameObject.DestroyImmediate(bossScene.targetObject);
            }

            bossState.BossSceneList.Clear();

            // 获取当前场景需要保存的对象
            List<BossInfo> tempBossScenes = FindBossScene();

            if (tempBossScenes == null || tempBossScenes.Count == 0) return;

            foreach (BossInfo obj in tempBossScenes)
            {
                GameObject originalObj = obj.targetObject;
                GameObject clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "BossStates/").transform);
                ObjectFinder.DeleteHealthManagerImmediate(clone);
                clone.SetActive(false);
                clone.name = originalObj.name;
                BossInfo newInfo = new BossInfo(clone, obj.path, originalObj.scene.name, originalObj.transform);
                bossState.BossSceneList.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);
        }

        // 恢复Boss场景状态
        public static void LoadBossState(BossState bossState, String path)
        {
            // 清理当前场景中已存在的同名对象
            List<BossInfo> currentSceneObjects = FindBossScene();
            foreach (BossInfo obj in currentSceneObjects)
            {
                if (obj.targetObject != null)
                    GameObject.DestroyImmediate(obj.targetObject);
            }

            if (bossState.BossSceneList == null || bossState.BossSceneList.Count == 0) return;

            // 从存档列表恢复
            foreach (BossInfo savedInfo in bossState.BossSceneList)
            {
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

        public static List<BossInfo> FindBossScene()
        {
            List<BossInfo> bossSceneObjects = new List<BossInfo>();

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
                            bossSceneObjects.Add(new BossInfo(obj, path, obj.scene.name, obj.transform));
                        }
                    }
                }
            }
            return bossSceneObjects;
        }
    }
}