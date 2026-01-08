using Snapshot_SilkSong.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.States
{
    public class PersistentState
    {
        public List<ObjectInfo> persistentList = new List<ObjectInfo>();

        // 保存实体状态
        public static void SavePersistentState(PersistentState persistentState, string path)
        {
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "PersistentState");

            foreach (ObjectInfo persistent in persistentState.persistentList)
            {
                if (persistent.targetObject != null)
                    GameObject.DestroyImmediate(persistent.targetObject);
            }

            persistentState.persistentList.Clear();


            List<ObjectInfo> tempPersistentList = FindPersistentBoolItemInDirectChildren();
            if (tempPersistentList == null || tempPersistentList.Count == 0) return;

            foreach (var obj in tempPersistentList)
            {
                var originalObj = obj.targetObject;
                var clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "/PersistentState").transform);
                clone.SetActive(false);
                clone.name = originalObj.name;

                var newInfo = new ObjectInfo(clone, obj.path, originalObj.scene.name, obj.isActive, originalObj.transform);
                persistentState.persistentList.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);
        }

        // 恢复实体状态
        public static void LoadPersistentState(PersistentState persistentState, string path)
        {
            FindPersistentBoolItemInDirectChildren().ForEach(obj =>
            {
                if (obj.targetObject != null)
                    GameObject.DestroyImmediate(obj.targetObject);
            });

            if (persistentState.persistentList == null || persistentState.persistentList.Count == 0) return;


            foreach (var savedInfo in persistentState.persistentList)
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

        public static List<ObjectInfo> FindPersistentBoolItemInDirectChildren()
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

                // 查找当前场景中的 PersistentBoolItem 对象
                foreach (var obj in GameObject.FindObjectsByType<PersistentBoolItem>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (obj == null || obj.gameObject.scene != scene) continue;

                    bool shouldExclude = false;
                    Transform currentParent = obj.transform.parent;

                    while (currentParent != null)
                    {
                        var healthManager = currentParent.GetComponent<HealthManager>();
                        var battleScene = currentParent.GetComponent<BattleScene>();
                        string name = obj.gameObject.name;

                        if (healthManager != null || battleScene != null || name == "Battle Scene")
                        {
                            shouldExclude = true;
                            break;
                        }

                        currentParent = currentParent.parent;
                    }

                    if (shouldExclude)
                    {
                        continue;
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
                        result.Add(new ObjectInfo(
                            gameObject,
                            ObjectFinder.GetGameObjectPath(gameObject),
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