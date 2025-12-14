using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class DeepCopier
{
    public static T DeepCopy<T>(T obj)
    {
        if (obj == null) return default;

        if (obj.GetType().IsValueType || obj is string)
            return obj;

        try
        {
            using (var stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Deep Copy failed: {ex.Message}", ex);
        }
    }
}