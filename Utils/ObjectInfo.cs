using UnityEngine;

namespace Snapshot_SilkSong.Utils
{
    [System.Serializable]

    public class ObjectInfo
    {
        public GameObject targetObject;
        public string path;
        public string sceneName;
        public bool isActive;
        public Vector3 savedLocalPosition;
        public Quaternion savedLocalRotation;
        public Vector3 savedLocalScale;

        public ObjectInfo(GameObject gameObject, string path, string sceneName, bool isActive, Transform originalTransform)
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

}
