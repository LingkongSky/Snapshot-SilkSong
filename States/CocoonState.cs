using Snapshot_SilkSong.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snapshot_SilkSong.CocoonState
{
    /*
    路径：DontDestroyOnLoad/1/CocoonState/
    */
    public class CocoonState
    {
        public List<ObjectInfo> CocoonSceneList;

        public CocoonState()
        {
            CocoonSceneList = new List<ObjectInfo>();
        }

        // 保存
        public static void SaveCocoonState(CocoonState cocoonState, String path)
        {
            ObjectFinder.EnsureDontDestroyOnLoadObject(path, "CocoonStates");

            // 清理旧数据
            foreach (ObjectInfo cocoonScene in cocoonState.CocoonSceneList)
            {
                if (cocoonScene.targetObject != null)
                    GameObject.DestroyImmediate(cocoonScene.targetObject);
            }

            cocoonState.CocoonSceneList.Clear();

            // 获取当前场景需要保存的对象
            List<ObjectInfo> tempCocoonScenes = FindCocoonScene();
            if (tempCocoonScenes == null || tempCocoonScenes.Count == 0) return;

            foreach (ObjectInfo obj in tempCocoonScenes)
            {
                GameObject originalObj = obj.targetObject;
                GameObject clone = GameObject.Instantiate(originalObj, GameObject.Find(path + "CocoonStates/").transform);
                clone.SetActive(false);
                clone.name = originalObj.name;
                ObjectInfo newInfo = new ObjectInfo(clone, obj.path, originalObj.scene.name, obj.isActive, originalObj.transform);
                cocoonState.CocoonSceneList.Add(newInfo);
            }

            UnityEngine.Object.DontDestroyOnLoad(GameObject.Find(path).transform);

        }

        // 恢复
        public static void LoadCocoonState(CocoonState cocoonState, String path)
        {
            // 清理当前场景中已存在的同类对象
            List<ObjectInfo> currentSceneObjects = FindCocoonScene();
            foreach (ObjectInfo obj in currentSceneObjects)
            {
                if (obj.targetObject != null)
                    GameObject.DestroyImmediate(obj.targetObject);
            }

            if (cocoonState.CocoonSceneList == null || cocoonState.CocoonSceneList.Count == 0) return;

            // 从存档列表恢复
            foreach (ObjectInfo savedInfo in cocoonState.CocoonSceneList)
            {
                if (savedInfo.targetObject == null)
                {
                    continue;
                }
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

        public static List<ObjectInfo> FindCocoonScene()
        {
            List<ObjectInfo> cocoonSceneObjects = new List<ObjectInfo>();

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

        private static void FindCocoonObjectsRecursive(GameObject currentObj, Scene scene, List<ObjectInfo> cocoonList)
        {
            // 检查当前对象是否为需要的对象
            if (currentObj.name == "Hornet Cocoon Corpse(Clone)")
            {
                string path = ObjectFinder.GetGameObjectPath(currentObj);
                cocoonList.Add(new ObjectInfo(currentObj, path, scene.name, currentObj.activeSelf,currentObj.transform));
            }

            // 递归遍历所有子对象
            foreach (Transform child in currentObj.transform)
            {
                FindCocoonObjectsRecursive(child.gameObject, scene, cocoonList);
            }
        }
    }
}