using Snapshot_SilkSong.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.States
{
    [System.Serializable]
    public class PersistentInfo
    {
        public GameObject targetObject;
        public string path;
        public string sceneName;
        public bool isActive;
        public Vector3 savedLocalPosition;
        public Quaternion savedLocalRotation;
        public Vector3 savedLocalScale;

        public PersistentInfo(GameObject gameObject, string path, string sceneName, bool isActive, Transform originalTransform)
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

    public class PersistentState
    {
        public List<PersistentInfo> healthManagers = new List<PersistentInfo>();

        // 保存实体状态
        public static void SavePersistentState(PersistentState persistentState, string path)
        {
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "PersistentState");

            persistentState.healthManagers.ForEach(info => GameObject.Destroy(info.targetObject));
            persistentState.healthManagers.Clear();

            foreach (var obj in FindPersistentBoolItemInDirectChildren())
            {
                //Debug.Log("Saving Persistent: " + obj.path);
                var originalObj = obj.targetObject;
                var clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "/PersistentState").transform);
                clone.SetActive(false);
                clone.name = originalObj.name;

                var newInfo = new PersistentInfo(clone, obj.path, originalObj.scene.name, obj.isActive, originalObj.transform);
                persistentState.healthManagers.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);
        }

        // 恢复实体状态
        public static void LoadPersistentState(PersistentState persistentState, string path)
        {
            FindPersistentBoolItemInDirectChildren().ForEach(obj =>
            {
                if (obj.targetObject != null)
                    GameObject.Destroy(obj.targetObject);
            });

            foreach (var savedInfo in persistentState.healthManagers)
            {
                //Debug.Log("Load Persistent: " + savedInfo.path);

                var clone = GameObject.Instantiate(savedInfo.targetObject);

                ObjectFinder.PlaceGameObjectToPath(clone, savedInfo.path, savedInfo.sceneName);
                clone.name = savedInfo.targetObject.name;
                clone.transform.localPosition = savedInfo.savedLocalPosition;
                clone.transform.localRotation = savedInfo.savedLocalRotation;
                clone.transform.localScale = savedInfo.savedLocalScale;

                clone.SetActive(savedInfo.isActive);
            }
        }

        public static List<PersistentInfo> FindPersistentBoolItemInDirectChildren()
        {
            var result = new List<PersistentInfo>();

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

                    Transform parent = obj.transform.parent;
                    if (parent != null)
                    {
                        var battleWave = parent.GetComponent<HealthManager>();
                        var battleScene = parent.GetComponent<BattleScene>();
                        var liftPlatform = obj.GetComponent<LiftPlatform>();

                        if (battleWave != null || battleScene != null || liftPlatform != null)
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
                        result.Add(new PersistentInfo(
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