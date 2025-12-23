using Snapshot_SilkSong.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.States
{

    [System.Serializable]
    public class LiftInfo
    {
        public GameObject targetObject;
        public string path;
        public string sceneName;
        public bool isActive;
        public Vector3 savedLocalPosition;
        public Quaternion savedLocalRotation;
        public Vector3 savedLocalScale;

        public LiftInfo(GameObject gameObject, string path, string sceneName, bool isActive, Transform originalTransform)
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

    public class LiftState
    {
        public List<LiftInfo> healthManagers = new List<LiftInfo>();

        // 保存实体状态
        public static void SaveLiftState(LiftState liftState, string path)
        {
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "LiftState");

            liftState.healthManagers.ForEach(info => GameObject.Destroy(info.targetObject));
            liftState.healthManagers.Clear();

            foreach (var obj in FindLiftPlatformInDirectChildren())
            {
                var originalObj = obj.targetObject;
                var clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "/LiftState").transform);
                clone.SetActive(false);
                clone.name = originalObj.name;

                var newInfo = new LiftInfo(clone, obj.path, originalObj.scene.name, obj.isActive, originalObj.transform);
                liftState.healthManagers.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);

        }

        // 恢复实体状态
        public static void LoadLiftState(LiftState liftState, string path)
        {
            FindLiftPlatformInDirectChildren().ForEach(obj =>
            {
                if (obj.targetObject != null)
                    GameObject.Destroy(obj.targetObject);
            });

            foreach (var savedInfo in liftState.healthManagers)
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

        // 获取当前场景中的所有游戏实体对象
        public static List<LiftInfo> FindLiftPlatformInDirectChildren()
        {
            var result = new List<LiftInfo>();
            var currentScene = SceneManager.GetActiveScene();

            foreach (var obj in GameObject.FindObjectsByType<LiftPlatform>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var objParent = obj.gameObject.transform.parent;
                if (objParent == null || objParent.gameObject.scene != currentScene) continue;

                // 检查父对象是否有 BattleWave 组件
                Transform parent = objParent.transform.parent;
                if (parent != null)
                {
                    var battleWave = parent.GetComponent<BattleWave>();
                    var battleScene = parent.GetComponent<BattleScene>();

                    if (battleWave != null || battleScene != null)
                    {
                        continue;
                    }
                }

                var gameObject = objParent.gameObject;
                result.Add(new LiftInfo(
                    gameObject,
                    ObjectFinder.GetGameObjectPath(gameObject),
                     gameObject.scene.name,
                    gameObject.activeSelf,
                    gameObject.transform
                ));
                
            }

            return result;
        }
    }
}
