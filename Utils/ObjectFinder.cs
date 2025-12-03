using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.Utils
{
    public class ObjectFinder
    {

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
        public static void PlaceGameObjectToPath(GameObject obj, string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            string[] pathParts = path.Split('/');

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
                    // 查找根物体
                    GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
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

    }
}
