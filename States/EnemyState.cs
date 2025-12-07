using Snapshot_SilkSong.Utils;
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
            this.savedLocalPosition = originalTransform.localPosition;
            this.savedLocalRotation = originalTransform.localRotation;
            this.savedLocalScale = originalTransform.localScale;
        }
    }

    public class EnemyState
    {
        public List<EnemyInfo> healthManagers = new List<EnemyInfo>();

        // 保存实体状态
        public static void SaveEnemyState(EnemyState enemyState, string path)
        {
            enemyState.healthManagers.ForEach(info => GameObject.Destroy(info.targetObject));
            enemyState.healthManagers.Clear();

            foreach (var obj in FindHealthManagerInDirectChildren())
            {
                //Debug.Log("Saving Enemy: " + obj.path);
                var originalObj = obj.targetObject;
                var clone = GameObject.Instantiate(originalObj);
                clone.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(clone);
                clone.name = originalObj.name;

                var newInfo = new EnemyInfo(clone, obj.path, obj.isActive, originalObj.transform);
                enemyState.healthManagers.Add(newInfo);
            }
        }

        // 恢复实体状态
        public static void LoadEnemyState(EnemyState enemyState, string path)
        {
            FindHealthManagerInDirectChildren().ForEach(obj =>
            {
                if (obj.targetObject != null)
                    GameObject.Destroy(obj.targetObject);
            });

            foreach (var savedInfo in enemyState.healthManagers)
            {
                //Debug.Log("Load Enemy: " + savedInfo.path);

                var clone = GameObject.Instantiate(savedInfo.targetObject);
                SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());
                ObjectFinder.PlaceGameObjectToPath(clone, savedInfo.path);
                clone.name = savedInfo.targetObject.name;
                clone.transform.localPosition = savedInfo.savedLocalPosition;
                clone.transform.localRotation = savedInfo.savedLocalRotation;
                clone.transform.localScale = savedInfo.savedLocalScale;

                clone.SetActive(savedInfo.isActive);

            }
        }

        // 获取当前场景中的所有游戏实体对象
        public static List<EnemyInfo> FindHealthManagerInDirectChildren()
        {
            var result = new List<EnemyInfo>();
            var currentScene = SceneManager.GetActiveScene();

            foreach (var obj in GameObject.FindObjectsByType<HealthManager>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (obj == null || obj.gameObject.scene != currentScene) continue;

                // 检查父对象是否有 BattleWave 组件
                Transform parent = obj.transform.parent;
                if (parent != null)
                {
                    var battleWave = parent.GetComponent<BattleWave>();
                    if (battleWave != null)
                    {
                        continue;
                    }
                }

                bool hasHeroController = false;
                foreach (Transform child in obj.transform)
                {
                    if (child.GetComponent<HeroController>() != null)
                    {
                        hasHeroController = true;
                        break;
                    }
                }

                if (!hasHeroController)
                {
                    var gameObject = obj.gameObject;
                    result.Add(new EnemyInfo(
                        gameObject,
                        ObjectFinder.GetGameObjectPath(gameObject),
                        gameObject.activeSelf,
                        gameObject.transform
                    ));
                }
            }

            return result;
        }
    }
}