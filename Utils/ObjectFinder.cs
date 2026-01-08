using UnityEngine;
using UnityEngine.SceneManagement;
namespace Snapshot_SilkSong.Utils
{
    public class ObjectFinder
    {

        // 检测游戏对象是否存在，不存在则创建
        public static void EnsureDontDestroyOnLoadObject(string path, string name)
        {
            string fullPath = string.IsNullOrEmpty(path) ? name : $"{path}/{name}";

            if (GameObject.Find(fullPath) != null)
                return;

            GameObject parent = string.IsNullOrEmpty(path) ? null : EnsureParentPath(path);
            GameObject obj = new GameObject(name);

            UnityEngine.Object.DontDestroyOnLoad(obj);

            if (parent != null)
                obj.transform.SetParent(parent.transform);
        }

        private static GameObject EnsureParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            string currentPath = "";
            Transform parent = null;

            foreach (string part in path.Split('/'))
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                currentPath += (currentPath.Length > 0 ? "/" : "") + part;
                GameObject obj = GameObject.Find(currentPath);

                if (obj == null)
                {
                    obj = new GameObject(part);
                    UnityEngine.Object.DontDestroyOnLoad(obj);

                    if (parent != null)
                        obj.transform.SetParent(parent);
                }

                parent = obj.transform;
            }

            return parent?.gameObject;
        }



        // 获取游戏对象的完整路径
        public static string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;

            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        // 根据路径设置游戏对象的父级
        public static void PlaceGameObjectToPath(GameObject obj, string path, string sceneName)
        {
            if (string.IsNullOrEmpty(path))
                return;

            string[] pathParts = path.Split('/');

            Scene targetScene = SceneManager.GetActiveScene(); // 默认为当前场景

            if (!string.IsNullOrEmpty(sceneName))
            {
                Scene namedScene = SceneManager.GetSceneByName(sceneName);
                if (namedScene.IsValid() && namedScene.isLoaded)
                {
                    targetScene = namedScene;
                    // 在设置父级之前移动场景
                    if (obj.scene != targetScene)
                    {
                        SceneManager.MoveGameObjectToScene(obj, targetScene);
                    }
                }
                else
                {
                    Debug.LogWarning($"Scene '{sceneName}' is not loaded, using active scene instead.");
                }
            }

            // 如果是根节点，不需要设置父级
            if (pathParts.Length == 1)
            {
                obj.transform.SetParent(null, false);
                obj.name = pathParts[0];
                return;
            }

            // 构建父级路径
            Transform parentTransform = null;

            // 处理每个父级层级
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                string parentName = pathParts[i];

                if (parentTransform == null)
                {
                    // 在目标场景中查找根物体
                    GameObject[] rootObjects = targetScene.GetRootGameObjects();
                    GameObject foundRoot = null;
                    foreach (GameObject rootObj in rootObjects)
                    {
                        if (rootObj.name == parentName)
                        {
                            foundRoot = rootObj;
                            break;
                        }
                    }

                    if (foundRoot == null)
                    {
                        // 创建缺失的父级
                        GameObject newParent = new GameObject(parentName);
                        newParent.transform.position = Vector3.zero;
                        newParent.transform.rotation = Quaternion.identity;
                        newParent.transform.localScale = Vector3.one;
                        parentTransform = newParent.transform;

                        // 确保新父级在正确场景中
                        SceneManager.MoveGameObjectToScene(newParent, targetScene);
                    }
                    else
                    {
                        parentTransform = foundRoot.transform;
                    }
                }
                else
                {
                    // 在现有父级下查找子物体
                    Transform childTransform = null;
                    for (int j = 0; j < parentTransform.childCount; j++)
                    {
                        if (parentTransform.GetChild(j).name == parentName)
                        {
                            childTransform = parentTransform.GetChild(j);
                            break;
                        }
                    }

                    if (childTransform == null)
                    {
                        // 创建缺失的父级
                        GameObject newParent = new GameObject(parentName);
                        newParent.transform.SetParent(parentTransform, false);
                        newParent.transform.localPosition = Vector3.zero;
                        newParent.transform.localRotation = Quaternion.identity;
                        newParent.transform.localScale = Vector3.one;
                        parentTransform = newParent.transform;
                    }
                    else
                    {
                        parentTransform = childTransform;
                    }
                }
            }

            // 设置最终父级
            if (parentTransform != null)
            {
                obj.transform.SetParent(parentTransform, false);
            }
            else
            {
                obj.transform.SetParent(null, false);
            }

            // 设置对象名称（最后一个路径部分）
            obj.name = pathParts[pathParts.Length - 1];
        }

        public static void DeleteHealthManagerImmediate(GameObject rootObject)
        {
            if (rootObject == null)
            {
                return;
            }

            HealthManager[] healthManagers = rootObject.GetComponentsInChildren<HealthManager>(true);

            if (healthManagers.Length == 0)
            {
                return;
            }

            foreach (HealthManager healthManager in healthManagers)
            {
                if (healthManager != null && healthManager.gameObject != null)
                {
                    string objectPath = GetGameObjectPath(healthManager.gameObject);
                    UnityEngine.Object.DestroyImmediate(healthManager.gameObject);
                }
            }
        }

  
        // 查找目标对象
        public static GameObject FindGameObjectByPath(string sceneName, string path)
        {
            GameObject sourceObj = GameObject.Find(path);
            if (sourceObj == null)
            {
                sourceObj = FindGameObjectInHierarchy(path);
            }

            return sourceObj;
        }

        private static GameObject FindGameObjectInHierarchy(string path)
        {
            string[] pathParts = path.Split('/');
            GameObject current = null;

            foreach (string part in pathParts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                if (current == null)
                {
                    current = GameObject.Find(part);
                }
                else
                {
                    Transform child = current.transform.Find(part);
                    current = child != null ? child.gameObject : null;
                }

                if (current == null) break;
            }

            return current;
        }
    }
}