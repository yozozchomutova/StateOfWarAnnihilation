using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ClassicSOWExporter
{
    private static readonly byte[] EDT_HEADER = { (byte)0x04, (byte)0x00, (byte)0x8e, (byte)0x26 };
    private static readonly byte[] EDT_ENDER = { (byte)0xE8, (byte)0x1D, (byte)0x00, (byte)0x00 };

    private static readonly byte[] MAP_HEADER = { (byte)0x04, (byte)0x56, (byte)0x45, (byte)0x52, (byte)0x37 };
    private static readonly byte[] MAP_ENDER = { (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00 };

    private static readonly byte[] MAP_FALSE = { (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00 };
    private static readonly byte[] MAP_FALSE2 = { (byte)0x00, (byte)0x00, (byte)0x00 };
    private static readonly byte[] MAP_TRUE = { (byte)0xFD, (byte)0xFF, (byte)0xFF, (byte)0x00 };
    private static readonly byte[] MAP_TRUE2 = { (byte)0x00, (byte)0x00, (byte)0x01 };

    private static readonly byte[] MAP_BORDER_BYTES = { (byte)0xFD, (byte)0xFF, (byte)0xFF, (byte)0x00, (byte)0xFD, (byte)0xFF, (byte)0xFF, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0 };

    private static readonly byte[] TIL_ENDER = { (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00 };

    public byte[] edtBytes;
    public byte[] mapBytes;
    public byte[] srfBytes;
    public byte[] tilBytes;
    public byte[] tmiBytes;

    public void exportAll(Camera photoshotCam, string levelFileName, int toMapSize)
    {
        exportEdt(toMapSize);
        exportMap(toMapSize);
        exportSrf(photoshotCam, toMapSize);
        exportTmiTil(toMapSize);

        //Save
        File.WriteAllBytes(levelFileName + ".edt", edtBytes);
        File.WriteAllBytes(levelFileName + ".map", mapBytes);
        File.WriteAllBytes(levelFileName + ".srf", srfBytes);
        File.WriteAllBytes(levelFileName + ".til", tilBytes);
        File.WriteAllBytes(levelFileName + ".tmi", tmiBytes);
    }

    public void exportEdt(int toMapSize)
    {
        //Concat
        List<Byte> byteList = new List<Byte>();

        writeToList(byteList, EDT_HEADER); //Header
        writeToList(byteList, new byte[] { (byte)0x02, 0x00, 0x00, 0x00 }); //Map index

        //Game properties
        int B = Team.BLUE;
        int G = Team.GREEN;
        edtWriteGameProperty(byteList, LevelData.teamStats[B].money, LevelData.teamStats[G].money);
        edtWriteGameProperty(byteList, LevelData.teamStats[B].destroyers, LevelData.teamStats[G].destroyers);
        edtWriteGameProperty(byteList, LevelData.teamStats[B].debrises, LevelData.teamStats[G].debrises);
        edtWriteGameProperty(byteList, LevelData.teamStats[B].carrybuses, LevelData.teamStats[G].carrybuses);
        edtWriteGameProperty(byteList, LevelData.teamStats[B].cyclones, LevelData.teamStats[G].cyclones);
        edtWriteGameProperty(byteList, LevelData.teamStats[B].jets, LevelData.teamStats[G].jets);

        writeToList(byteList, new byte[12]); // Unknown bytes

        //Blue turrets
        writeToList(byteList, new byte[] { (byte)(LevelData.teamStats[B].ant ? 0x01 : 0x00) }); // Cannon
        writeToList(byteList, new byte[] { (byte)(LevelData.teamStats[B].antiAircraft ? 0x01 : 0x00) }); // Antiair
        writeToList(byteList, new byte[] { (byte)(LevelData.teamStats[B].plasmatic ? 0x01 : 0x00) }); // Plasma
        writeToList(byteList, new byte[] { (byte)(LevelData.teamStats[B].machineGun ? 0x01 : 0x00) }); // Rotary
        writeToList(byteList, new byte[] { (byte)(LevelData.teamStats[B].granader ? 0x01 : 0x00) }); // Defragmentator
        writeToList(byteList, new byte[5]); // Unknown bytes

        //Green turrets
        writeToList(byteList, new byte[] { (byte)(LevelData.teamStats[G].ant ? 0x01 : 0x00) }); // Cannon
        writeToList(byteList, new byte[] { (byte)(LevelData.teamStats[G].antiAircraft ? 0x01 : 0x00) }); // Antiair
        writeToList(byteList, new byte[] { (byte)(LevelData.teamStats[G].plasmatic ? 0x01 : 0x00) }); // Plasma
        writeToList(byteList, new byte[] { (byte)(LevelData.teamStats[G].machineGun ? 0x01 : 0x00) }); // Rotary
        writeToList(byteList, new byte[] { (byte)(LevelData.teamStats[G].granader ? 0x01 : 0x00) }); // Defragmentator
        writeToList(byteList, new byte[17]); // Unknown bytes

        //Damage maybe?
        edtWriteGameProperty(byteList, 20, 20);

        //Annihilation, doesn't have general setting for enabling/disabling PVT/SMF for each team. Instead, it will detect if command center has atleast 1 SMF production enabled.
        bool blueHasPVT = false, greenHasPVT = false;
        foreach (Unit unit in LevelData.units)
        {
            if (unit is CommandCenter)
            {
                if (unit.team.id == Team.BLUE && (unit as CommandCenter).SMFcanProduce)
                    blueHasPVT = true;
                else if (unit.team.id == Team.GREEN && (unit as CommandCenter).SMFcanProduce)
                    greenHasPVT = true;
            }
        }

        edtWriteGameProperty(byteList, blueHasPVT ? 0 : 1, greenHasPVT ? 0 : 1);
        edtWriteGameProperty(byteList, LevelData.teamStats[B].research, LevelData.teamStats[G].research);

        writeToList(byteList, new byte[216]); // Unknown bytes
                                              //TODO Timer

        //Buildings
        List<Unit> unitsToWrite = new List<Unit>();
        unitsToWrite.AddRange(LevelData.units);
        unitsToWrite = unitsToWrite.OrderBy(x => (int)(x.unitType)).ToList();

        for (int i = 0; i < unitsToWrite.Count; i++)
        {
            Unit u = unitsToWrite[i];
            UnitSOWCW b = u.generateSOWCW(new UnitSOWCW(), toMapSize);

            if (b.teamId != -1)
            {
                if (u.unitType == Unit.UnitType.BUILDING || u.unitType == Unit.UnitType.TOWER)
                {
                    writeEdtBuilding(b, byteList);
                }
                else if (u.unitType == Unit.UnitType.UNIT || u.unitType == Unit.UnitType.STEPPER || u.unitType == Unit.UnitType.SMF)
                {
                    writeEdtUnit(b, byteList);
                }
            }
        }

        //Return
        writeToList(byteList, EDT_ENDER);
        edtBytes = byteListToArray(byteList);
    }

    public void exportMap(int toMapSize)
    {
        //Concat
        List<Byte> byteList = new List<Byte>();

        writeToList(byteList, MAP_HEADER); //Header
        writeToList(byteList, toShort((short)0, Order.LITTLE_ENDIAN)); //Screen vision X
        writeToList(byteList, toShort((short)0, Order.LITTLE_ENDIAN)); //Offset
        writeToList(byteList, toShort((short)0, Order.LITTLE_ENDIAN)); //Screen vision Y
        writeToList(byteList, toShort((short)0, Order.LITTLE_ENDIAN)); //Offset

        writeToList(byteList, toInt(toMapSize, Order.LITTLE_ENDIAN)); //Map width
        writeToList(byteList, toInt(toMapSize, Order.LITTLE_ENDIAN)); //Map height

        //Main data
        TerrainData td = LevelData.mainTerrain.terrainData;
        int halfAS = (int)((float)td.alphamapWidth / 2f);
        int quadAS = (int)((float)td.alphamapWidth / 4f);
        float[,,] alphaTextures = td.GetAlphamaps(quadAS, quadAS, halfAS, halfAS);

        //Initialize
        bool[][] tiles = new bool[toMapSize][];
        for (int x = 0; x < tiles.Length; x++)
        {
            tiles[x] = new bool[toMapSize];
            for (int y = 0; y < tiles[0].Length; y++)
            {
                tiles[x][y] = false;
            }
        }

        //Generate
        for (int x = 0; x < tiles.Length; x++)
        {
            for (int y = 0; y < tiles[0].Length; y++)
            {
                float nX = (float)x / (float)toMapSize; //Normalized X
                float nY = (float)y / (float)toMapSize; //Normalized Y
                int tX = (int)(nX * halfAS); //Terrain X
                int tY = (int)(nY * halfAS); //Terrain Y
                Vector3 magicPoint = new Vector3(
                    (nY / 2f + 0.25f) * td.size.x, 
                    LevelData.mainTerrain.SampleHeight(new Vector3((nY / 2f + 0.25f) * td.size.x, 0, (nX / 2f + 0.25f) * td.size.x)), 
                    (nX / 2f + 0.25f) * td.size.x);

                bool condition1 = (alphaTextures[tX, tY, 1] + alphaTextures[tX, tY, 4] + alphaTextures[tX, tY, 6] > 0.1f); //Is there any kind of rock texture
                if (!condition1 && x != 0 && x != tiles.Length-1) //Not first, not last
                {
                    condition1 = (alphaTextures[tX + 1, tY, 1] + alphaTextures[tX + 1, tY, 4] + alphaTextures[tX + 1, tY, 6] > 0.1f) ||
                        (alphaTextures[tX - 1, tY, 1] + alphaTextures[tX - 1, tY, 4] + alphaTextures[tX - 1, tY, 6] > 0.1f);
                }
                if (!condition1 && y != 0 && y != tiles[0].Length - 1) //Not first, not last
                {
                    condition1 = (alphaTextures[tX, tY + 1, 1] + alphaTextures[tX, tY + 1, 4] + alphaTextures[tX, tY + 1, 6] > 0.1f) ||
                        (alphaTextures[tX, tY - 1, 1] + alphaTextures[tX, tY - 1, 4] + alphaTextures[tX, tY - 1, 6] > 0.1f);
                }
                bool condition2 = (Physics.OverlapSphere(magicPoint, 2f, 1 << 12).Length > 0); //Is there map object nearby

                tiles[toMapSize - x - 1][toMapSize - y - 1] = condition1 || condition2;
            }
        }

        //Fill holes
        tiles = fixMapHoles(tiles);

        //Scale to actually fit map
        bool[][] scaledTiles = new bool[toMapSize][];
        for (int x = 0; x < scaledTiles.Length; x++)
        {
            scaledTiles[x] = new bool[toMapSize];
            for (int y = 0; y < scaledTiles[0].Length; y++)
            {
                scaledTiles[x][y] = false;
            }
        }

        for (int x = 0; x < toMapSize; x++)
        {
            for (int y = 0; y < toMapSize; y++)
            {
                if (x <= 3 || x >= tiles.Length - 4 || y <= 3 || y >= tiles[0].Length - 2)
                    continue;

                float nX = (float) x / (float) toMapSize; //Normalized X
                float nY = (float) y / (float) toMapSize; //Normalized Y

                int moveXpixels = Mathf.RoundToInt((nX - 0.5f) * 8f);
                int moveYpixels = Mathf.RoundToInt((nY - 0.6f) * 5f);
                //Debug.Log("X: " + x +  " |Y: " + y + " |MX: " + moveXpixels + " |MY: " + moveYpixels + " |C: " + tiles[x][y]);
                scaledTiles[x + moveXpixels][y + moveYpixels] = tiles[x][y];

            }
        }
        tiles = scaledTiles;

        //Yes, fix twice
        tiles = fixMapHoles(tiles);
        tiles = fixMapHoles(tiles);

        //Use
        for (int y = 0; y < tiles[0].Length; y++)
        {
            for (int x = 0; x < tiles.Length; x++)
            {
                writeToList(byteList, new byte[] { (byte)y, (byte)x }); //Position

                if (x == 0 || y == 0 || y == tiles[0].Length - 1 || x == tiles.Length - 1)
                { // Check if it's border
                    writeToList(byteList, MAP_BORDER_BYTES);
                }
                else
                {
                    bool tile = tiles[x][y];
                    writeToList(byteList, tile ? MAP_TRUE : MAP_FALSE); //Ground units can't pass
                    writeToList(byteList, MAP_FALSE); //Air units can't pass
                    writeToList(byteList, tile ? MAP_TRUE2 : MAP_FALSE2); //Buildings units can't pass
                    writeToList(byteList, new byte[4]); //Ending bytes
                }
            }
        }

        //Return
        writeToList(byteList, MAP_ENDER);
        mapBytes = byteListToArray(byteList);
    }

    private bool[][] fixMapHoles(bool[][] tiles)
    {
        for (int x = 0; x < tiles.Length; x++)
        {
            for (int y = 0; y < tiles[0].Length; y++)
            {
                if (!tiles[x][y])
                {
                    if (x != 0 && x != tiles.Length - 1) //Not first, not last
                    {
                        if (tiles[x + 1][y] && tiles[x - 1][y])
                            tiles[x][y] = true;
                    }

                    if (y != 0 && y != tiles[0].Length - 1) //Not first, not last
                    {
                        if (tiles[x][y + 1] && tiles[x][y - 1])
                            tiles[x][y] = true;
                    }
                }
            }
        }

        return tiles;
    }

    public void exportSrf(Camera photoshotCam, int toMapSize)
    {
        //IMPLEMENTED
        int mapSize = toMapSize * 32;
        Rect rect = new Rect(0, 0, mapSize, mapSize);
        RenderTexture renderTexture = new RenderTexture(mapSize, mapSize, 24);
        Texture2D screenShot = new Texture2D(mapSize, mapSize, TextureFormat.RGBA32, false);

        photoshotCam.targetTexture = renderTexture;
        photoshotCam.Render();

        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();

        photoshotCam.targetTexture = null;
        RenderTexture.active = null;

        MonoBehaviour.Destroy(renderTexture);
        renderTexture = null;

        //Start process
        byte[] srfHeaderBytes = Resources.Load<TextAsset>("SOWCW Exporting/srfHeader").bytes;

        //Fetch copy + paste bytes
        List<Byte> byteArrayOutputStream = new List<Byte>();
        byteArrayOutputStream.AddRange(srfHeaderBytes);

        //Apply textures

        byteArrayOutputStream.AddRange(screenShot.EncodeToJPG(100));

        //Return
        srfBytes = byteArrayOutputStream.ToArray();
    }

    public void exportSrf(int toMapSize)
    {
        Texture2D bcgImg = new Texture2D(toMapSize*32, toMapSize*32);

        byte[] srfHeaderBytes = Resources.Load<TextAsset>("SOWCW Exporting/srfHeader").bytes;
        Texture2D grassTxt = Resources.Load<Texture2D>("SOWCW Exporting/grass");
        Color[] grassColors = grassTxt.GetPixels(0, 0, grassTxt.width, grassTxt.height);
        Texture2D rockTxt = Resources.Load<Texture2D>("TerrainTextures/rock1");
        Color[] rockColors = rockTxt.GetPixels(0, 0, rockTxt.width, rockTxt.height);
        Texture2D mudTxt = Resources.Load<Texture2D>("TerrainTextures/mud1");
        Color[] mudColors = mudTxt.GetPixels(0, 0, mudTxt.width, mudTxt.height);
        Texture2D snowTxt = Resources.Load<Texture2D>("TerrainTextures/snow");
        Color[] snowColors = snowTxt.GetPixels(0, 0, snowTxt.width, snowTxt.height);

        //Fetch copy + paste bytes
        List<Byte> byteArrayOutputStream = new List<Byte>();
        byteArrayOutputStream.AddRange(srfHeaderBytes);

        //Apply textures
        float[,,] textures = LevelData.mainTerrain.terrainData.GetAlphamaps(128, 128, 256, 256);
        //File.WriteAllBytes("C:/Users/danek/AppData/Local/Result5.png", snowText.EncodeToPNG());
        //Color[] snowColors = snowText.GetPixels(0, 0, snowText.width, snowText.height);
        for (int x = 0; x < bcgImg.width; x++)
        {
            for (int y = 0; y < bcgImg.height; y++)
            {
                bcgImg.SetPixel(x, y, grassColors[x % 512 + y % 512 * 512]);

                //Convert to relative to game terrain
                int rX = (int)((float)x / bcgImg.width * 256f);
                int rY = (int)((float)y / bcgImg.height * 256f);

                if (textures[rX % 256, rY % 256, 1] > 0.1f)
                {
                    bcgImg.SetPixel(x, y, rockColors[x % 1024 + y % 1024 * 1024]);
                } else if (textures[rX % 256, rY % 256, 2] > 0.1f)
                {
                    bcgImg.SetPixel(x, y, mudColors[x % 512 + y % 512 * 512]);
                }/* else if (textures[rX % 256, rY % 256, 3] > 0.1f)
                {
                    bcgImg.SetPixel(x, y, snowColors[x % 512 + y % 512 * 512]);
                } else if (textures[rX % 256, rY % 256, 5] > 0.1f)
                {
                    bcgImg.SetPixel(x, y, snowColors[x % 512 + y % 512 * 512]);
                }*/
            }
        }



        byteArrayOutputStream.AddRange(bcgImg.EncodeToJPG());

        //Return
        srfBytes = byteArrayOutputStream.ToArray();
    }

    public void exportTmiTil(int toMapSize)
    {
        //TMI
        List<Byte> tmiByteList = new List<Byte>();

        //Header
        writeToList(tmiByteList, toShort((short)toMapSize, Order.LITTLE_ENDIAN));
        writeToList(tmiByteList, toShort((short)toMapSize, Order.LITTLE_ENDIAN));

        //Data
        for (int y = 0; y < toMapSize; y++)
        {
            for (int x = 0; x < toMapSize; x++)
            {
                writeToList(tmiByteList, toShort((short)0, Order.LITTLE_ENDIAN));
            }
        }

        tmiBytes = byteListToArray(tmiByteList);

        //TIL
        byte[] tilHeaderBytes = Resources.Load<TextAsset>("SOWCW Exporting/tilHeader").bytes;
        List<Byte> tilByteList = new List<Byte>();

        writeToList(tilByteList, tilHeaderBytes);
        writeToList(tilByteList, TIL_ENDER);

        tilBytes = byteListToArray(tilByteList);
    }

    private static void writeToList(List<Byte> byteList, byte[] bytes){
        foreach (Byte byte_ in bytes) {
            byteList.Add(byte_);
        }
    }

    private static byte[] byteListToArray(List<Byte> byteList)
    {
        byte[] bytes = new byte[byteList.Count];

        for (int i = 0; i < byteList.Count; i++)
        {
            bytes[i] = byteList[i];
        }

        return bytes;
    }

    private static void edtWriteGameProperty(List<Byte> byteList, int blueValue, int greenValue)
    {
        writeToList(byteList, toInt(blueValue, Order.LITTLE_ENDIAN));
        writeToList(byteList, toInt(greenValue, Order.LITTLE_ENDIAN));
        writeToList(byteList, new byte[] { 0x00, 0x00, 0x00, 0x00 });
    }

    public void writeEdtBuilding(UnitSOWCW b, List<byte> byteList)
    {
        writeToList(byteList, toInt(123, Order.LITTLE_ENDIAN)); // Building unit
        writeToList(byteList, new byte[] { (byte)b.unitId, 0, 0, 0 }); // Type
        writeToList(byteList, toInt(b.productionsAvailableFromStart, Order.LITTLE_ENDIAN)); // Production available from start
        writeToList(byteList, toInt(b.productionUnits[0], Order.LITTLE_ENDIAN)); // 1. Production unit
        writeToList(byteList, toInt(b.productionUnits[1], Order.LITTLE_ENDIAN)); // 2. Production unit
        writeToList(byteList, toInt(b.productionUnits[2], Order.LITTLE_ENDIAN)); // 3. Production unit
        writeToList(byteList, toInt(b.productionUnits[3], Order.LITTLE_ENDIAN)); // 4. Production unit
        writeToList(byteList, toInt(b.productionUnits[4], Order.LITTLE_ENDIAN)); // 5. Production unit
        writeToList(byteList, toInt(b.productionUnitUpgradeItems[0], Order.LITTLE_ENDIAN)); // 1. Production unit Upgrade count
        writeToList(byteList, toInt(b.productionUnitUpgradeItems[1], Order.LITTLE_ENDIAN)); // 2. Production unit Upgrade count
        writeToList(byteList, toInt(b.productionUnitUpgradeItems[2], Order.LITTLE_ENDIAN)); // 3. Production unit Upgrade count
        writeToList(byteList, toInt(b.productionUnitUpgradeItems[3], Order.LITTLE_ENDIAN)); // 4. Production unit Upgrade count
        writeToList(byteList, toInt(b.productionUnitUpgradeItems[4], Order.LITTLE_ENDIAN)); // 5. Production unit Upgrade count
        writeToList(byteList, toInt(Mathf.RoundToInt(b.HPpercentage * 13536), Order.LITTLE_ENDIAN)); // HP
        writeToList(byteList, toInt(b.teamId, Order.LITTLE_ENDIAN)); // Team color
        writeToList(byteList, toInt(b.hasSateliteProtection ? 1 : 0, Order.LITTLE_ENDIAN)); // Satellite protection
        writeToList(byteList, toInt(b.xTile, Order.LITTLE_ENDIAN)); // tile X
        writeToList(byteList, toInt(b.yTile, Order.LITTLE_ENDIAN)); // tile Y
        writeToList(byteList, toInt(Mathf.RoundToInt(b.HPpercentage * 100), Order.LITTLE_ENDIAN)); // HP (Percentage)
    }

    public void writeEdtUnit(UnitSOWCW b, List<byte> byteList)
    {
        writeToList(byteList, toInt(224, Order.LITTLE_ENDIAN)); // unit
        writeToList(byteList, new byte[] { (byte)b.unitId, 0, 0, 0 }); // Type
        writeToList(byteList, toInt(b.teamId, Order.LITTLE_ENDIAN)); // Team color
        writeToList(byteList, toInt(b.x, Order.LITTLE_ENDIAN)); // screen X
        writeToList(byteList, toInt(b.y, Order.LITTLE_ENDIAN)); // screen Y
    }

    public static byte[] toInt(int value, Order order)
    {
        byte[] bytes = new byte[4];

        if (order == Order.LITTLE_ENDIAN)
        {
            bytes[0] = (byte)(value);
            bytes[1] = (byte)(value >> 8);
            bytes[2] = (byte)(value >> 16);
            bytes[3] = (byte)(value >> 24);
        }
        else
        {
            bytes[0] = (byte)(value >> 24);
            bytes[1] = (byte)(value >> 16);
            bytes[2] = (byte)(value >> 8);
            bytes[3] = (byte)(value);
        }

        return bytes;
    }

    public static byte[] toShort(short value, Order order)
    {
        byte[] bytes = new byte[2];

        if (order == Order.LITTLE_ENDIAN)
        {
            bytes[0] = (byte)(value);
            bytes[1] = (byte)(value >> 8);
        }
        else
        {
            bytes[0] = (byte)(value >> 8);
            bytes[1] = (byte)(value);
        }

        return bytes;
    }

    public enum Order
    {
        LITTLE_ENDIAN,
        BIG_ENDIAN
    }
}
