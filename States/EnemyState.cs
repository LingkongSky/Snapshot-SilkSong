using Snapshot_SilkSong.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Dictionary<string, int> directChildNameCounts = new Dictionary<string, int>();

        public EnemyInfo(GameObject gameObject, string path, bool isActive, Transform originalTransform)
        {
            this.targetObject = gameObject;
            this.path = path;
            this.isActive = isActive;

            // 从原始 Transform 中记录局部信息
            this.savedLocalPosition = originalTransform.localPosition;
            this.savedLocalRotation = originalTransform.localRotation;
            this.savedLocalScale = originalTransform.localScale;

            // 记录所有子GameObject的完整路径，并确保每个名字的子实体只有一个
            RecordChildPathsWithUniqueNames(originalTransform);
        }

        // 递归记录所有子对象的路径，确保每个直接子对象名称唯一
        private void RecordChildPathsWithUniqueNames(Transform parentTransform, string currentPath = "")
        {
            HashSet<string> recordedChildNamesInCurrentLevel = new HashSet<string>();

            foreach (Transform child in parentTransform)
            {
                if (recordedChildNamesInCurrentLevel.Contains(child.name))
                {
                    continue; 
                }

                recordedChildNamesInCurrentLevel.Add(child.name);

                string childPath = string.IsNullOrEmpty(currentPath)
                    ? child.name
                    : $"{currentPath}/{child.name}";

                childPaths.Add(childPath);

                // 递归记录孙子对象
                if (child.childCount > 0)
                {
                    RecordChildPathsWithUniqueNames(child, childPath);
                }
            }
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
        public static void SaveEnemyState(EnemyState enemyState, String path)
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

                // 创建新的EnemyInfo，会自动记录子对象路径
                EnemyInfo newInfo = new EnemyInfo(clone, obj.path, obj.isActive, originalObj.transform);
                enemyState.healthManagers.Add(newInfo);

                Debug.Log($"Saved {newInfo.childPaths.Count} child paths for {originalObj.name}");
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

                // 清理不在记录中的子GameObject，并确保每个名字的子实体只有一个
                CleanupUnrecordedChildrenAndEnsureUniqueNames(clone.transform, savedInfo.childPaths);
            }
        }

        // 清理不在记录中的子GameObject，并确保每个名字的子实体只有一个
        private static void CleanupUnrecordedChildrenAndEnsureUniqueNames(Transform parentTransform, List<string> recordedChildPaths)
        {
            // 收集所有子对象，包括嵌套的
            List<Transform> allChildren = new List<Transform>();
            GetAllChildren(parentTransform, allChildren);

            allChildren.Remove(parentTransform);

            Dictionary<Transform, Dictionary<string, List<Transform>>> childrenByParent = new Dictionary<Transform, Dictionary<string, List<Transform>>>();

            foreach (Transform child in allChildren)
            {
                Transform parent = child.parent;
                if (!childrenByParent.ContainsKey(parent))
                {
                    childrenByParent[parent] = new Dictionary<string, List<Transform>>();
                }

                if (!childrenByParent[parent].ContainsKey(child.name))
                {
                    childrenByParent[parent][child.name] = new List<Transform>();
                }

                childrenByParent[parent][child.name].Add(child);
            }

            // 处理每个父对象下的子对象
            foreach (var parentEntry in childrenByParent)
            {
                Transform currentParent = parentEntry.Key;
                Dictionary<string, List<Transform>> childrenByName = parentEntry.Value;

                foreach (var nameEntry in childrenByName)
                {
                    string childName = nameEntry.Key;
                    List<Transform> childrenWithSameName = nameEntry.Value;

                    // 获取相对于父对象的路径列表
                    List<string> childPaths = new List<string>();
                    foreach (Transform child in childrenWithSameName)
                    {
                        string childPath = GetRelativePath(parentTransform, child);
                        childPaths.Add(childPath);
                    }

                    if (childrenWithSameName.Count > 1)
                    {
                        for (int i = 1; i < childrenWithSameName.Count; i++)
                        {
                            string pathToDelete = GetRelativePath(parentTransform, childrenWithSameName[i]);
                            GameObject.Destroy(childrenWithSameName[i].gameObject);
                        }
                    }

                    // 检查每个子实体是否在记录中
                    foreach (Transform child in childrenWithSameName)
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
                        }
                    }
                }
            }
        }
        private static void GetAllChildren(Transform parent, List<Transform> children)
        {
            children.Add(parent);

            foreach (Transform child in parent)
            {
                GetAllChildren(child, children);
            }
        }

        private static string GetRelativePath(Transform parent, Transform child)
        {
            if (child == parent)
                return "";

            List<string> pathSegments = new List<string>();
            Transform current = child;

            while (current != null && current != parent)
            {
                pathSegments.Insert(0, current.name);
                current = current.parent;
            }

            if (current != parent)
            {
                return "";
            }

            return string.Join("/", pathSegments);
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