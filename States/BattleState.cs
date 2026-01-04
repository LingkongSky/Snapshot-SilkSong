using Snapshot_SilkSong.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.BattleState
{
    [System.Serializable]
    public class BattleInfo
    {
        public GameObject targetObject;
        public string path;
        public string sceneName; 

        public Vector3 savedLocalPosition;
        public Quaternion savedLocalRotation;
        public Vector3 savedLocalScale;

        public BattleInfo(GameObject gameObject, string path, string sceneName, Transform originalTransform)
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
    路径：DontDestroyOnLoad/1/BattleState/
    */
    public class BattleState
    {
        public List<BattleInfo> BattleSceneList;

        public BattleState()
        {
            BattleSceneList = new List<BattleInfo>();
        }

        // 保存战斗场景状态
        public static void SaveBattleState(BattleState battleState, String path)
        {
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "BattleStates");

            // 清理旧数据
            foreach (BattleInfo battleScene in battleState.BattleSceneList)
            {
                GameObject.Destroy(battleScene.targetObject);
            }

            battleState.BattleSceneList.Clear();

            // 获取当前场景需要保存的对象
            List<BattleInfo> tempBattleScenes = FindBattleScene();

            foreach (BattleInfo obj in tempBattleScenes)
            {
                GameObject originalObj = obj.targetObject;
                GameObject clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "BattleStates/").transform);
                clone.SetActive(false);
                clone.name = originalObj.name;
                BattleInfo newInfo = new BattleInfo(clone, obj.path, originalObj.scene.name, originalObj.transform);
                battleState.BattleSceneList.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);

        }

        // 恢复战斗场景状态
        public static void LoadBattleState(BattleState battleState, String path)
        {
            // 清理当前场景中已存在的同类对象
            List<BattleInfo> currentSceneObjects = FindBattleScene();
            foreach (BattleInfo obj in currentSceneObjects)
            {
                if (obj.targetObject != null)
                    GameObject.Destroy(obj.targetObject);
            }

            // 从存档列表恢复
            foreach (BattleInfo savedInfo in battleState.BattleSceneList)
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

        public static List<BattleInfo> FindBattleScene()
        {
            List<BattleInfo> battleSceneObjects = new List<BattleInfo>();

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

                    battleSceneObjects.Add(new BattleInfo(obj, path, obj.scene.name, obj.transform));
                }
            }
            return battleSceneObjects;
        }

    }
}