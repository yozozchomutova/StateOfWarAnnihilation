using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using UnityEngine;

[System.Serializable]
public abstract class Level
{
    #region [Functions] Static general functions
    public static Level GetMapLevelInstance(short major, short minor, short fix)
    {
        Level level = null;

        if (major == 4)         // 4.X.X
        {
            if (minor == 1)     // 4.1.X
            {
                if (fix == 0)   // 4.1.0
                {
                    level = new ML_4_1();
                }
            }
        } else if (major == 3)  // 3.X.X
        {

        }

        return level;
    }

    public static BinaryReader OpenFileReader(string path)
    {
        return new BinaryReader(new FileStream(path, FileMode.Open));
    }

    public static BinaryWriter OpenFileWriter(string path)
    {
        return new BinaryWriter(new FileStream(path, FileMode.Create));
    }

    public static (short, short, short) GetVersion(BinaryReader br)
    {
        var (major, minor, fix) = (br.ReadInt16(), br.ReadInt16(), br.ReadInt16());
        if (major > 5 || major < 1 || minor > 500 | minor < 1)
        {
            br.BaseStream.Position = 0;
            (major, minor, fix) = (br.ReadByte(), br.ReadByte(), br.ReadByte());
        }

        return (major, minor, fix);
    }
    #endregion
    #region [Functions] Saving/Writing
    protected abstract void WriteHeader_(BinaryWriter bw);
    public void WriteHeader(BinaryWriter bw)
    {
        //Always write version as first!
        var (major, minor, fix) = GameManager.GetGameVersion();
        bw.Write((short) major);    //int16 -> Major version
        bw.Write((short) minor);    //int16 -> Minor version
        bw.Write((short) fix);      //int16 -> Fix version

        WriteHeader_(bw);
    }

    protected abstract void WriteMapData_();
    public void WriteMapData(BinaryWriter bw)
    {
        WriteMapData_();

        byte[] mapLevelData = CompressionManager.Compress(SerializeManager.Serialize(this));
        Debug.Log("MapLevel: " + mapLevelData.Length + " |L ");
        bw.Write(mapLevelData.Length);      //int32 -> Map data length
        bw.Write(mapLevelData);             //byte[] -> Map data
    }
    #endregion
    #region [Functions] Loading/Reading
    protected abstract void LoadHeader_(BinaryReader br, Header header);
    public Header LoadHeader(BinaryReader br)
    {
        Header header = new Header();
        LoadHeader_(br, header);

        Debug.Log("Stream: " + br.BaseStream.Length + " |Pos: " + br.BaseStream.Position);
        header.mapLevelData_length = br.ReadInt32();                                                                        //int32 -> Level data length
        header.mapLevelData_offset = (int)(br.BaseStream.Position); //Set offset of starting readíng

        return header;
    }

    public abstract void LoadMapData_();
    public void LoadMapData(Header header, BinaryReader br)
    {
        br.BaseStream.Position = header.mapLevelData_offset;
        byte[] mapLevelBytes = br.ReadBytes(header.mapLevelData_length);
        Debug.Log("MAP DATA: " + header.mapLevelData_offset + " |Length: " + header.mapLevelData_length);
        Level l = SerializeManager.Deserialize<Level>(CompressionManager.Decompress(mapLevelBytes));
        l.LoadMapData_();
    }
    #endregion
    #region [Functions] Converting data types
    protected static short HM_floatToShort(float value)
    {
        return (short)(value * short.MaxValue);
    }

    protected static float HM_shortToFloat(short value)
    {
        return (float)value / short.MaxValue;
    }

    protected static byte AM_floatToByte(float value)
    {
        return (byte)(value * 255);
    }

    protected static float AM_byteToFloat(byte value)
    {
        return (float)value / 255f;
    }
    #endregion

    #region [Class] Header object
    public class Header
    {
        //File
        public string lvlName;
        public string fileLvlPath;

        //Version
        public int major, minor, fix;

        //Data
        public string description;
        public byte[] imgData;

        public int mapWidth, mapHeight;
        public int difficulty;

        //Full level data
        public int mapLevelData_offset, mapLevelData_length;
    }
    #endregion
}
