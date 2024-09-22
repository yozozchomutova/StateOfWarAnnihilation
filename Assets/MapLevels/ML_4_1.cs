#region [Libraries] All
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using UnityEditorInternal;
using UnityEngine;
using static MapLevel;
using static MapLevelManager.UnitSerializable;
#endregion

[System.Serializable]
public class ML_4_1 : Level
{
    #region [Variables] Serialized variables
    //Terrain
    public int mapWidth;
    public int mapHeight;

    public short[] terrainHeights;
    public byte[] alphamaps;

    //public byte[] alphamapGrass;
    //public byte[] alphamapRocks;
    //public byte[] alphamapMuds;
    //public byte[] alphamapSands;
    //public byte[] alphamapSandRocks;
    //public byte[] alphamapSnows;
    //public byte[] alphamapSnowRocks;
    //public byte[] alphamapGravel;

    public bool[] detailGrass;

    //Map objects
    public string[] mapObject_ids;
    public MapLevelManager.Transfornm3Ser[] mapObject_transform;

    //Units
    public MapLevelManager.UnitSerializable[] unitData;

    //Team stats
    public TeamStats02_12[] teamStats;

    //Weather
    public int timeClock;
    public bool timeStatic;
    public WorldEnvironment.Weather weatherDefault;
    #endregion

    #region [Functions] Saving/Writing
    protected override void WriteHeader_(BinaryWriter bw)
    {
        PanelSaveConfirm save = GameObject.FindObjectOfType<PanelSaveConfirm>();

        bw.Write((int)      save.panelLevelInfo.description.text.Length);                   //int32 -> Desc length
        bw.Write((byte[])   Encoding.UTF8.GetBytes(save.panelLevelInfo.description.text));  //byte[] -> Description
        bw.Write((int)      save.panelLevelInfo.imageBytes.Length);                         //int32 -> Img length
        bw.Write((byte[])   save.panelLevelInfo.imageBytes);                                //byte[] -> Img data

        bw.Write(PanelNewLevel.normalMapSize);  //int32 -> Map Width
        bw.Write(PanelNewLevel.normalMapSize);  //int32 -> Map Height

        bw.Write(PanelSaveConfirm.CalculateLevelDifficulty());  //float -> Difficulty
    }

    protected override void WriteMapData_()
    {
        TerrainData tData = Terrain.activeTerrain.terrainData;

        //1. Map properties
        //Map size
        mapWidth = (int)tData.size.x;
        mapHeight = (int)tData.size.z;

        //2. Terrain heights
        int heightmapRes = tData.heightmapResolution;
        float[,] fetchTerrainHeights = tData.GetHeights(0, 0, heightmapRes, heightmapRes);
        terrainHeights = new short[heightmapRes * heightmapRes];

        for (int x = 0; x < fetchTerrainHeights.GetLength(0); x++)
        {
            for (int y = 0; y < fetchTerrainHeights.GetLength(1); y++)
            {
                terrainHeights[x * heightmapRes + y] = HM_floatToShort(fetchTerrainHeights[y, x]);
            }
        }

        //3. Alphamaps
        int alphamapRes = tData.alphamapResolution;
        float[,,] fetchAlphamaps = tData.GetAlphamaps(0, 0, alphamapRes, alphamapRes);
        alphamaps = new byte[alphamapRes * alphamapRes * tData.alphamapLayers];

        for (int layer = 0; layer < fetchAlphamaps.GetLength(2); layer++)
        {
            for (int y = 0; y < fetchAlphamaps.GetLength(1); y++)
            {
                for (int x = 0; x < fetchAlphamaps.GetLength(0); x++)
                {
                    alphamaps[layer * alphamapRes * alphamapRes + y * alphamapRes + x] = AM_floatToByte(fetchAlphamaps[y, x, layer]);
                }
            }
        }

        //4. Map Objects
        List<MapObject> mos = LevelData.mapObjects;

        mapObject_ids = new string[mos.Count];
        mapObject_transform = new MapLevelManager.Transfornm3Ser[mos.Count];

        for (int j = 0; j < mos.Count; j++)
        {
            MapObject mo = mos[j];
            Vector3 p = mo.gameObject.transform.position;
            Vector3 r = mo.gameObject.transform.localRotation.eulerAngles;

            mapObject_ids[j] = mo.objectId;
            mapObject_transform[j] = new MapLevelManager.Transfornm3Ser(p.x, p.y, p.z, r.x, r.y, r.z);
        }

        //5. Grass/Details/Flowers
        int detailRes = tData.detailResolution;
        detailGrass = new bool[detailRes * detailRes];
        int[,] detailGrassInt = tData.GetDetailLayer(0, 0, detailRes, detailRes, 0);

        for (int x = 0; x < detailGrassInt.GetLength(0); x++)
            for (int y = 0; y < detailGrassInt.GetLength(1); y++)
                detailGrass[x * detailRes + y] = detailGrassInt[x, y] == 1;

        //6. Buildings, Towers, Units
        List<Unit> units = LevelData.units;
        unitData = new MapLevelManager.UnitSerializable[units.Count];
        for (int j = 0; j < units.Count; j++)
        {
            unitData[j] = units[j].serializeUnit();
        }

        //7. Team stats
        teamStats = LevelData.teamStats;
        for (int j = 0; j < teamStats.Length; j++)
            teamStats[j].OnSerialize();

        //8. Weather environment
        timeClock = LevelData.environment.time;
        timeStatic = LevelData.environment.timeStatic;
        weatherDefault = LevelData.environment.weatherDefault;
    }
    #endregion
    #region [Functions] Loading/Reading
    protected override void LoadHeader_(BinaryReader br, Header header)
    {
        int descLength = br.ReadInt32();                                                    //int32 -> Desc length
        header.description = Encoding.UTF8.GetString(br.ReadBytes(descLength));             //byte[] -> Description
        int imgLength = br.ReadInt32();                                                     //int32 -> Img length
        header.imgData = br.ReadBytes(imgLength);                                           //byte[] -> Img data

        header.mapWidth = br.ReadInt32();                                                   //int32 -> Map Width
        header.mapHeight = br.ReadInt32();                                                  //int32 -> Map Height

        header.difficulty = br.ReadInt32();                                                 //float -> Difficulty
    }

    public override void LoadMapData_()
    {
        bool editorMode = LevelData.scene == LevelData.Scene.EDITOR;
        //Transform parentTrans = GameObject.Find("Objects");
        TerrainData tData = Terrain.activeTerrain.terrainData;

        //1. Map size
        tData.size = new Vector3(mapWidth, tData.size.y, mapHeight);

        //2. Terrain heights
        int heightmapRes = tData.heightmapResolution;
        float[,] fetchTerrainHeights = tData.GetHeights(0, 0, heightmapRes, heightmapRes);

        for (int x = 0; x < heightmapRes ; x++)
        {
            for (int y = 0; y < heightmapRes; y++)
            {
                fetchTerrainHeights[y, x] = HM_shortToFloat(terrainHeights[x * heightmapRes + y]);
            }
        }

        //Addition: For game playing mode, add walls
        if (!editorMode)
        {
            int TH_height = fetchTerrainHeights.GetLength(0) - 1;
            int TH_width = fetchTerrainHeights.GetLength(1) - 1;

            for (int i = 0; i < heightmapRes; i++)
            {
                fetchTerrainHeights[0, i] = 0;
                fetchTerrainHeights[i, 0] = 0;
                fetchTerrainHeights[TH_height, i] = 0;
                fetchTerrainHeights[i, TH_width] = 0;
            }
        }

        //Finally apply
        tData.SetHeights(0, 0, fetchTerrainHeights);
        LevelData.gridManager.BuildGrid(tData);

        //3. Alphamaps
        int alphamapRes = tData.alphamapResolution;
        float[,,] fetchAlphamaps = tData.GetAlphamaps(0, 0, alphamapRes, alphamapRes);

        for (int layer = 0; layer < fetchAlphamaps.GetLength(2); layer++)
        {
            for (int y = 0; y < fetchAlphamaps.GetLength(1); y++)
            {
                for (int x = 0; x < fetchAlphamaps.GetLength(0); x++)
                {
                    fetchAlphamaps[y, x, layer] = AM_byteToFloat(alphamaps[layer * alphamapRes * alphamapRes + y * alphamapRes + x]);
                }
            }
        }

        tData.SetAlphamaps(0, 0, fetchAlphamaps);

        //4. Map Objects
        for (int i = 0; i < mapObject_ids.Length; i++)
        {
            string mo_id = mapObject_ids[i];
            MapLevelManager.Transfornm3Ser t = mapObject_transform[i];
            MapLevel.PlaceMapObject(GlobalList.mapObjects[mo_id], t.toVectorPosition(), t.toVectorRotation());
        }

        //5. Grass/Details/Flowers
        int detailRes = tData.detailResolution;
        int[,] detailGrassInt = new int[detailRes, detailRes];
        //int[,] detailSandGrassInt = new int[detailRes, detailRes];

        for (int x = 0; x < detailGrassInt.GetLength(0); x++)
        {
            for (int y = 0; y < detailGrassInt.GetLength(1); y++)
            {
                //if (fetchAlphamaps[x, y, 3] > 0.25f)
                //{
                //    detailSandGrassInt[x, y] = detailGrass[x, y] ? 1 : 0;
                //}
                //else
                    detailGrassInt[x, y] = detailGrass[x * detailRes + y] ? 1 : 0;
            }
        }
        tData.SetDetailLayer(0, 0, 0, detailGrassInt);
        //tData.SetDetailLayer(0, 0, 1, detailSandGrassInt);

        //6. Buildings, Towers, Units
        for (int j = 0; j < unitData.Length; j++)
        {
            MapLevel.spawnUnit(unitData[j]);
        }

        //7. Team stats
        LevelData.teamStats = teamStats;

        //8. Wearher environment
        LevelData.environment.time = timeClock;
        LevelData.environment.timeStatic = timeStatic;
        LevelData.environment.weatherDefault = weatherDefault;
        LevelData.environment.events.Clear();
        LevelData.environment.OnTimeUpdate();
    }

    //Final

    #endregion
}
