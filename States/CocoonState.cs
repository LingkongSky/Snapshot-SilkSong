using Snapshot_SilkSong.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.CocoonState
{
    [System.Serializable]
    public class CocoonInfo
    {
        public GameObject targetObject;
        public string path;
        public string sceneName;

        public Vector3 savedLocalPosition;
        public Quaternion savedLocalRotation;
        public Vector3 savedLocalScale;

        public CocoonInfo(GameObject gameObject, string path, string sceneName, Transform originalTransform)
        {
            this.targetObject = gameObject;
            this.path = path;
            this.sceneName = sceneName;

            // 从原始 Transform 中记录局部信息
            this.savedLocalPosition = originalTransform.localPosition;
            this.savedLocalRotation = originalTransform.localRotation;
            this.savedLocalScale = originalTransform.localScale;
        }
    }
    /*
    路径：DontDestroyOnLoad/1/CocoonState/
    */
    public class CocoonState
    {
        public List<CocoonInfo> CocoonSceneList;

        public CocoonState()
        {
            CocoonSceneList = new List<CocoonInfo>();
        }

        // 保存
        public static void SaveCocoonState(CocoonState cocoonState, String path)
        {
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "CocoonStates");

            // 清理旧数据
            foreach (CocoonInfo cocoonScene in cocoonState.CocoonSceneList)
            {
                if (cocoonScene.targetObject != null)
                    GameObject.DestroyImmediate(cocoonScene.targetObject);
            }

            cocoonState.CocoonSceneList.Clear();

            // 获取当前场景需要保存的对象
            List<CocoonInfo> tempCocoonScenes = FindCocoonScene();
            if (tempCocoonScenes == null || tempCocoonScenes.Count == 0) return;

            foreach (CocoonInfo obj in tempCocoonScenes)
            {
                GameObject originalObj = obj.targetObject;
                GameObject clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "CocoonStates/").transform);
                clone.SetActive(false);
                clone.name = originalObj.name;
                CocoonInfo newInfo = new CocoonInfo(clone, obj.path, originalObj.scene.name, originalObj.transform);
                cocoonState.CocoonSceneList.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);

        }

        // 恢复
        public static void LoadCocoonState(CocoonState cocoonState, String path)
        {
            // 清理当前场景中已存在的同类对象
            List<CocoonInfo> currentSceneObjects = FindCocoonScene();
            foreach (CocoonInfo obj in currentSceneObjects)
            {
                if (obj.targetObject != null)
                    GameObject.DestroyImmediate(obj.targetObject);
            }

            if (cocoonState.CocoonSceneList == null || cocoonState.CocoonSceneList.Count == 0) return;

            // 从存档列表恢复
            foreach (CocoonInfo savedInfo in cocoonState.CocoonSceneList)
            {
                GameObject clone = GameObject.Instantiate(savedInfo.targetObject);
                clone.name = savedInfo.targetObject.name;

                // 恢复父级结构
                ObjectFinder.PlaceGameObjectToPath(clone, savedInfo.path, savedInfo.sceneName);

                clone.transform.localPosition = savedInfo.savedLocalPosition;
                clone.transform.localRotation = savedInfo.savedLocalRotation;
                clone.transform.localScale = savedInfo.savedLocalScale;
                clone.SetActive(true);
            }
        }

        public static List<CocoonInfo> FindCocoonScene()
        {
            List<CocoonInfo> cocoonSceneObjects = new List<CocoonInfo>();

            // 遍历所有已加载的场景
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.name == "DontDestroyOnLoad" ||
                    scene.name == "HideAndDontSave")
                {
                    continue;
                }

                GameObject[] rootObjects = scene.GetRootGameObjects();

                foreach (GameObject rootObj in rootObjects)
                {
                    FindCocoonObjectsRecursive(rootObj, scene, cocoonSceneObjects);
                }
            }
            return cocoonSceneObjects;
        }

        private static void FindCocoonObjectsRecursive(GameObject currentObj, Scene scene, List<CocoonInfo> cocoonList)
        {
            // 检查当前对象是否为需要的对象
            if (currentObj.name == "Hornet Cocoon Corpse(Clone)")
            {
                string path = ObjectFinder.GetGameObjectPath(currentObj);
                cocoonList.Add(new CocoonInfo(currentObj, path, scene.name, currentObj.transform));
            }

            // 递归遍历所有子对象
            foreach (Transform child in currentObj.transform)
            {
                FindCocoonObjectsRecursive(child.gameObject, scene, cocoonList);
            }
        }
    }
}