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
        public List<string> childPaths = new List<string>();

        public EnemyInfo(GameObject gameObject, string path, bool isActive, Transform originalTransform)
        {
            this.targetObject = gameObject;
            this.path = path;
            this.isActive = isActive;
            this.savedLocalPosition = originalTransform.localPosition;
            this.savedLocalRotation = originalTransform.localRotation;
            this.savedLocalScale = originalTransform.localScale;

            RecordChildPathsWithUniqueNames(originalTransform);
        }

        private void RecordChildPathsWithUniqueNames(Transform parentTransform, string currentPath = "")
        {
            var recordedNames = new HashSet<string>();

            foreach (Transform child in parentTransform)
            {
                if (!recordedNames.Add(child.name)) continue;

                string childPath = string.IsNullOrEmpty(currentPath)
                    ? child.name
                    : $"{currentPath}/{child.name}";

                childPaths.Add(childPath);

                if (child.childCount > 0)
                {
                    RecordChildPathsWithUniqueNames(child, childPath);
                }
            }
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
                Debug.Log("Saving Enemy: " + obj.path);
                var originalObj = obj.targetObject;
                var clone = GameObject.Instantiate(originalObj);

                UnityEngine.Object.DontDestroyOnLoad(clone);
                clone.name = originalObj.name;
                clone.SetActive(false);

                var newInfo = new EnemyInfo(clone, obj.path, obj.isActive, originalObj.transform);
                enemyState.healthManagers.Add(newInfo);

                Debug.Log($"Saved {newInfo.childPaths.Count} child paths for {originalObj.name}");
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
                Debug.Log("Load Enemy: " + savedInfo.path);

                var clone = GameObject.Instantiate(savedInfo.targetObject);
                clone.SetActive(savedInfo.isActive);
                SceneManager.MoveGameObjectToScene(clone, SceneManager.GetActiveScene());
                ObjectFinder.PlaceGameObjectToPath(clone, savedInfo.path);
                clone.name = savedInfo.targetObject.name;

                var transform = clone.transform;
                transform.localPosition = savedInfo.savedLocalPosition;
                transform.localRotation = savedInfo.savedLocalRotation;
                transform.localScale = savedInfo.savedLocalScale;

                CleanupUnrecordedChildren(clone.transform, savedInfo.childPaths);
            }
        }

        // 清理不在记录中的子GameObject
        private static void CleanupUnrecordedChildren(Transform parentTransform, List<string> recordedChildPaths)
        {
            var childrenToProcess = new Queue<Transform>();
            childrenToProcess.Enqueue(parentTransform);

            while (childrenToProcess.Count > 0)
            {
                var current = childrenToProcess.Dequeue();
                var children = new List<Transform>();

                foreach (Transform child in current) children.Add(child);

                // 处理重复名称的子对象
                var nameGroups = new Dictionary<string, List<Transform>>();
                foreach (var child in children)
                {
                    if (!nameGroups.ContainsKey(child.name))
                        nameGroups[child.name] = new List<Transform>();
                    nameGroups[child.name].Add(child);
                }

                foreach (var group in nameGroups.Values)
                {
                    // 保留第一个，删除其他同名子对象
                    for (int i = 1; i < group.Count; i++)
                    {
                        GameObject.Destroy(group[i].gameObject);
                    }

                    // 检查是否在记录中
                    foreach (var child in group)
                    {
                        string childPath = GetRelativePath(parentTransform, child);
                        if (!recordedChildPaths.Contains(childPath))
                        {
                            Debug.Log($"Deleting unrecorded child: {childPath}");
                            GameObject.Destroy(child.gameObject);
                        }
                        else
                        {
                            Debug.Log($"Keeping recorded child: {childPath}");
                            childrenToProcess.Enqueue(child);
                        }
                    }
                }
            }
        }

        private static string GetRelativePath(Transform root, Transform child)
        {
            var pathSegments = new List<string>();
            var current = child;

            while (current != null && current != root)
            {
                pathSegments.Insert(0, current.name);
                current = current.parent;
            }

            return current == root ? string.Join("/", pathSegments) : "";
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