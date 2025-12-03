using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class UniversalObjectSerializer
{
    // 序列化对象到文件
    public static bool SaveObjectToFile(object obj, string filePath)
    {
        try
        {
            ObjectData data = SerializeObject(obj);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, data);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"保存对象失败: {e.Message}");
            return false;
        }
    }

    // 从文件加载对象
    public static object LoadObjectFromFile(string filePath)
    {
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ObjectData data = (ObjectData)formatter.Deserialize(fs);
                return DeserializeObject(data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载对象失败: {e.Message}");
            return null;
        }
    }

    // 对象数据容器
    [Serializable]
    private class ObjectData
    {
        public string TypeName;
        public string AssemblyName;
        public byte[] SerializedData;
        public ObjectData[] NestedObjects;
    }

    // 序列化核心方法
    private static ObjectData SerializeObject(object obj)
    {
        if (obj == null) return null;

        ObjectData data = new ObjectData();
        Type objectType = obj.GetType();

        data.TypeName = objectType.FullName;
        data.AssemblyName = objectType.Assembly.FullName;

        // 使用 BinaryFormatter 序列化对象
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            data.SerializedData = ms.ToArray();
        }

        return data;
    }

    // 反序列化核心方法
    private static object DeserializeObject(ObjectData data)
    {
        if (data == null) return null;

        try
        {
            // 加载程序集和类型
            Assembly assembly = Assembly.Load(data.AssemblyName);
            Type objectType = assembly.GetType(data.TypeName);

            if (objectType == null)
            {
                Debug.LogError($"无法找到类型: {data.TypeName}");
                return null;
            }

            // 反序列化对象
            using (MemoryStream ms = new MemoryStream(data.SerializedData))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"反序列化失败: {e.Message}");
            return null;
        }
    }
}