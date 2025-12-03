using Snapshot_SilkSong.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.EnemyState
{
    [System.Serializable]
    public class EnemyInfo
    {
        public GameObject targetObject;
        public string path;
        public bool isActive;

        public Vector3 savedLocalPosition;
        public Quaternion savedLocalRotation;
        public Vector3 savedLocalScale;

        public EnemyInfo(GameObject gameObject, string path, bool isActive, Transform originalTransform)
        {
            this.targetObject = gameObject;
            this.path = path;
            this.isActive = isActive;

            // 从原始 Transform 中记录局部信息
            this.savedLocalPosition = originalTransform.localPosition;
            this.savedLocalRotation = originalTransform.localRotation;
            this.savedLocalScale = originalTransform.localScale;
        }
    }

    public class EnemyState
    {
        public List<EnemyInfo> healthManagers;

        public EnemyState()
        {
            healthManagers = new List<EnemyInfo>();
        }

        // 保存实体状态
        public static void SaveEnemyState(EnemyState enemyState, String path)  // 如果还是存在组件状态不同步 对代码进行逻辑分步测试
        {
            // 清理旧数据
            foreach (EnemyInfo enemyInfo in enemyState.healthManagers)
            {
                GameObject.Destroy(enemyInfo.targetObject);
            }
            enemyState.healthManagers.Clear();

            // 获取当前场景需要保存的对象
            List<EnemyInfo> tempHealthManagers = FindHealthManagerInDirectChildren();

            foreach (EnemyInfo obj in tempHealthManagers)
            {
                Debug.Log("Saving Enemy: " + obj.path);
                GameObject originalObj = obj.targetObject;
                GameObject clone = GameObject.Instantiate(originalObj);
                UnityEngine.Object.DontDestroyOnLoad(clone);
                clone.name = originalObj.name;
                clone.SetActive(false);
                EnemyInfo newInfo = new EnemyInfo(clone, obj.path, obj.isActive, originalObj.transform);
                enemyState.healthManagers.Add(newInfo);
            }
        }

        // 恢复实体状态
        public static void LoadEnemyState(EnemyState enemyState, String path)
        {
            // 清理当前场景中已存在的同类对象
            List<EnemyInfo> currentSceneObjects = FindHealthManagerInDirectChildren();
            foreach (EnemyInfo obj in currentSceneObjects)
            {
                if (obj.targetObject != null)
                    GameObject.Destroy(obj.targetObject);
            }

            // 从存档列表恢复
            foreach (EnemyInfo savedInfo in enemyState.healthManagers)
            {
                Debug.Log("Load Enemy: " + savedInfo.path);

                GameObject clone = GameObject.Instantiate(savedInfo.targetObject);

                clone.SetActive(savedInfo.isActive);
                SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());
                ObjectFinder.PlaceGameObjectToPath(clone, savedInfo.path);

                clone.name = savedInfo.targetObject.name;

                // 恢复局部变换
                clone.transform.localPosition = savedInfo.savedLocalPosition;
                clone.transform.localRotation = savedInfo.savedLocalRotation;
                clone.transform.localScale = savedInfo.savedLocalScale;
            }
        }

        // 获取当前场景中的所有游戏实体对象
        public static List<EnemyInfo> FindHealthManagerInDirectChildren()
        {
            List<EnemyInfo> healthManagerObjects = new List<EnemyInfo>();

            HealthManager[] allObjects = GameObject.FindObjectsByType<HealthManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            Scene currentActiveScene = SceneManager.GetActiveScene();

            foreach (HealthManager obj in allObjects)
            {
                if (obj == null) continue;

                GameObject healthManagerObject = obj.gameObject;

                // 检查对象是否在当前活动场景中
                if (healthManagerObject.scene != currentActiveScene)
                {
                    continue;
                }

                // 检查是否有HeroController子对象
                bool hasHeroController = false;
                foreach (Transform child in obj.transform)
                {
                    if (child == null) continue;

                    HeroController heroController = child.GetComponent<HeroController>();
                    if (heroController != null)
                    {
                        hasHeroController = true;
                        break;
                    }
                }

                // 如果没有HeroController子对象，则保存这个HealthManager
                if (!hasHeroController)
                {
                    string path = ObjectFinder.GetGameObjectPath(healthManagerObject);
                    healthManagerObjects.Add(new EnemyInfo(
                        healthManagerObject,
                        path,
                        healthManagerObject.activeSelf,
                        healthManagerObject.transform
                    ));
                }
            }
            return healthManagerObjects;
        }

    }
}