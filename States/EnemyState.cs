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
        public string sceneName;
        public bool isActive;
        public Vector3 savedLocalPosition;
        public Quaternion savedLocalRotation;
        public Vector3 savedLocalScale;

        public EnemyInfo(GameObject gameObject, string path, string sceneName, bool isActive, Transform originalTransform)
        {
            this.targetObject = gameObject;
            this.path = path;
            this.sceneName = sceneName;
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
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "EnemyState");

            enemyState.healthManagers.ForEach(info => GameObject.Destroy(info.targetObject));
            enemyState.healthManagers.Clear();

            foreach (var obj in FindHealthManagerInDirectChildren())
            {
                if (obj.path.Contains("Boss Scene"))
                {
                    continue;
                }

                //Debug.Log("Saving Enemy: " + obj.path);
                var originalObj = obj.targetObject;
                var clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "/EnemyState").transform);
                clone.SetActive(false);
                clone.name = originalObj.name;

                var newInfo = new EnemyInfo(clone, obj.path, originalObj.scene.name, obj.isActive, originalObj.transform);
                enemyState.healthManagers.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);
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

                var clone = GameObject.Instantiate(savedInfo.targetObject);

                ObjectFinder.PlaceGameObjectToPath(clone, savedInfo.path, savedInfo.sceneName);
                clone.name = savedInfo.targetObject.name;
                clone.transform.localPosition = savedInfo.savedLocalPosition;
                clone.transform.localRotation = savedInfo.savedLocalRotation;
                clone.transform.localScale = savedInfo.savedLocalScale;

                clone.SetActive(savedInfo.isActive);
            }
        }

        // 获取所有非特殊场景中的游戏实体对象
        public static List<EnemyInfo> FindHealthManagerInDirectChildren()
        {
            var result = new List<EnemyInfo>();

            // 遍历所有已加载的场景
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.name == "DontDestroyOnLoad" ||
                    scene.name == "HideAndDontSave")
                {
                    continue;
                }

                var allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                foreach (var obj in allObjects)
                {
                    if (obj == null || obj.gameObject.scene != scene) continue;

                    // 要求同时有生命组件及尸体组件
                    var healthManager = obj.GetComponent<HealthManager>();
                    var activeCorpse = obj.GetComponent<ActiveCorpse>();

                    if (healthManager == null && activeCorpse == null) continue;

                    // 检查所有祖先（父组件及以上）是否有BattleScene组件
                    bool hasBattleSceneInAncestors = false;
                    Transform currentParent = obj.transform.parent;

                    while (currentParent != null)
                    {
                        var battleScene = currentParent.GetComponent<BattleScene>();
                        if (battleScene != null)
                        {
                            hasBattleSceneInAncestors = true;
                            break;
                        }
                        currentParent = currentParent.parent; // 继续向上查找
                    }

                    if (hasBattleSceneInAncestors)
                    {
                        continue;
                    }

                    string path = ObjectFinder.GetGameObjectPath(obj);

                    if (path.Contains("Boss Scene"))
                    {
                        continue;
                    }

                    // 避免获取到玩家
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
                        string objectPath = ObjectFinder.GetGameObjectPath(gameObject);

                        if (objectPath.Contains("Boss Scene"))
                        {
                            continue;
                        }

                        result.Add(new EnemyInfo(
                            gameObject,
                            objectPath,
                            gameObject.scene.name,
                            gameObject.activeSelf,
                            gameObject.transform
                        ));
                    }
                }
            }

            return result;
        }
    }
}