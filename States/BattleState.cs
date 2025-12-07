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

        public Vector3 savedLocalPosition;
        public Quaternion savedLocalRotation;
        public Vector3 savedLocalScale;

        public BattleInfo(GameObject gameObject, string path, Transform originalTransform)
        {
            this.targetObject = gameObject;
            this.path = path;

            // 从原始 Transform 中记录局部信息
            this.savedLocalPosition = originalTransform.localPosition;
            this.savedLocalRotation = originalTransform.localRotation;
            this.savedLocalScale = originalTransform.localScale;
        }
    }

    /*
    BattleScene中
    切换Gates的PlayerMakerFSM的Active导致门状态丢失（Battle Gate Mossbone(1)）
    切换Wave 1的Avtive导致Boss丢失
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
                GameObject clone = GameObject.Instantiate(originalObj);
                clone.SetActive(false);
                clone.name = originalObj.name;
                UnityEngine.Object.DontDestroyOnLoad(clone);
                BattleInfo newInfo = new BattleInfo(clone, obj.path, originalObj.transform);
                battleState.BattleSceneList.Add(newInfo);
            }
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

                SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());

                // 恢复父级结构
                ObjectFinder.PlaceGameObjectToPath(clone, savedInfo.path);

                clone.transform.localPosition = savedInfo.savedLocalPosition;
                clone.transform.localRotation = savedInfo.savedLocalRotation;
                clone.transform.localScale = savedInfo.savedLocalScale;
                clone.SetActive(true);


            }
        }

        public static List<BattleInfo> FindBattleScene()
        {
            List<BattleInfo> battleSceneObjects = new List<BattleInfo>();

            BattleScene[] allComponents = GameObject.FindObjectsByType<BattleScene>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Scene currentActiveScene = SceneManager.GetActiveScene();

            foreach (BattleScene component in allComponents)
            {
                if (component == null) continue;

                GameObject obj = component.gameObject;

                if (obj.scene != currentActiveScene)
                {
                    continue;
                }

                string path = ObjectFinder.GetGameObjectPath(obj);
                Debug.Log(path);
                battleSceneObjects.Add(new BattleInfo(obj, path, obj.transform));
            }
            return battleSceneObjects;
        }

    }
}