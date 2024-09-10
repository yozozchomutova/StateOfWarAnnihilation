using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class ArrayWorker
{
    #region [Functions] +,- Operations with arrays
    public static (byte[], byte[]) SplitByteArray(byte[] sourceArray, int index)
    {
        if (index < 0 || index > sourceArray.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be within the bounds of the array.");

        byte[] firstArray = new byte[index];
        byte[] secondArray = new byte[sourceArray.Length - index];

        Buffer.BlockCopy(sourceArray, 0, firstArray, 0, index);
        Buffer.BlockCopy(sourceArray, index, secondArray, 0, secondArray.Length);
        return (firstArray, secondArray);
    }

    public static byte[] ConcatByteArrays(byte[] firstArray, byte[] secondArray)
    {
        byte[] result = new byte[firstArray.Length + secondArray.Length];
        Buffer.BlockCopy(firstArray, 0, result, 0, firstArray.Length);
        Buffer.BlockCopy(secondArray, 0, result, firstArray.Length, secondArray.Length);
        return result;
    }
    #endregion
    #region [Functions] Float arrays
    public static byte[] Float2DToByte1D(float[,] floatArray)
    {
        int rows = floatArray.GetLength(0);
        int cols = floatArray.GetLength(1);
        byte[] byteArray = new byte[rows * cols * sizeof(float)];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    public static float[,] Byte1DToFloat2D(byte[] byteArray, int rows, int cols)
    {
        float[,] floatArray = new float[rows, cols];
        Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
        return floatArray;
    }

    public static byte[] Float3DToByte1D(float[,,] floatArray)
    {
        int d1 = floatArray.GetLength(0);
        int d2 = floatArray.GetLength(1);
        int d3 = floatArray.GetLength(2);
        byte[] byteArray = new byte[d1 * d2 * d3 * sizeof(float)];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    public static float[,,] Byte1DToFloat3D(byte[] byteArray, int d1, int d2, int d3)
    {
        float[,,] floatArray = new float[d1, d2, d3];
        Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
        return floatArray;
    }
    #endregion
    #region [Functions] <T> Serializable class conversion
    public static byte[] SerializableToBytes<T>(T obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public static T BytesToSerializable<T>(byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(data))
        {
            object obj = bf.Deserialize(ms);
            return (T)obj;
        }
    }
    #endregion
}