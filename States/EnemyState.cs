using Snapshot_SilkSong.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.EnemyState
{

    public class EnemyState
    {
        public List<ObjectInfo> enemyList = new List<ObjectInfo>();

        // 保存实体状态
        public static void SaveEnemyState(EnemyState enemyState, string path)
        {
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "EnemyState");

            enemyState.enemyList.ForEach(info => GameObject.DestroyImmediate(info.targetObject));
            enemyState.enemyList.Clear();

            foreach (ObjectInfo enemy in enemyState.enemyList)
            {
                if (enemy.targetObject != null)
                    GameObject.DestroyImmediate(enemy.targetObject);
            }

            List<ObjectInfo> temphealthManager = FindHealthManagerInDirectChildren();
            if (temphealthManager == null || temphealthManager.Count == 0) return;

            foreach (var obj in temphealthManager)
            {
                //Debug.Log("Saving Enemy: " + obj.path);
                var originalObj = obj.targetObject;
                var clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "/EnemyState").transform);
                clone.SetActive(false);
                clone.name = originalObj.name;

                var newInfo = new ObjectInfo(clone, obj.path, originalObj.scene.name, obj.isActive, originalObj.transform);
                enemyState.enemyList.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);
        }

        // 恢复实体状态
        public static void LoadEnemyState(EnemyState enemyState, string path)
        {
            FindHealthManagerInDirectChildren().ForEach(obj =>
            {
                if (obj.targetObject != null)
                    GameObject.DestroyImmediate(obj.targetObject);
            });

            if (enemyState.enemyList == null || enemyState.enemyList.Count == 0) return;

            foreach (var savedInfo in enemyState.enemyList)
            {
                if (savedInfo.targetObject == null)
                {
                    continue;
                }

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
        public static List<ObjectInfo> FindHealthManagerInDirectChildren()
        {
            var result = new List<ObjectInfo>();

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
                        result.Add(new ObjectInfo(
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