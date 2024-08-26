using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MapLevel;
using static MapLevelManager.UnitSerializable;

[System.Serializable]
public class ML_03_05
{
    #region Variables

    //Map stats
    //Width & height are saved without borders in level editor (-200 points of size)
    public int mapWidth;
    public int mapHeight;

    public int navigations_agentTypeID;

    public float waterLevel;

    public short[] terrainHeights;

    public byte[] alphamapGrass;
    public byte[] alphamapRocks;
    public byte[] alphamapMuds;
    public byte[] alphamapSands;
    public byte[] alphamapSandRocks;
    public byte[] alphamapSnows;
    public byte[] alphamapSnowRocks;
    public byte[] alphamapGravel;

    //details
    public bool[,] detailGrass;

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
    public WorldEnvironment.Event[] weatherEvents;
    #endregion

    #region Saving level
    public ML_03_05(Terrain terrain, BarMapObjects barMapObjects, BarBuildings barBuildings, BarNavigations barNavigations)
    {
        TerrainData tData = terrain.terrainData;

        //1. Map properties
        //Map size
        mapWidth = (int)tData.size.x;
        mapHeight = (int)tData.size.z;

        navigations_agentTypeID = LevelData.navigations_agentTypeID;

        //2. Terrain heights
        int heightmapRes = tData.heightmapResolution;
        heightmapRes = Mathf.CeilToInt(heightmapRes / 2f);
        float[,] fetchTerrainHeights = tData.GetHeights(Mathf.CeilToInt(heightmapRes / 2f), Mathf.CeilToInt(heightmapRes / 2f), heightmapRes, heightmapRes);
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
        alphamapRes = Mathf.CeilToInt(alphamapRes / 2f);
        float[,,] fetchAlphamaps = tData.GetAlphamaps(Mathf.CeilToInt(alphamapRes / 2f), Mathf.CeilToInt(alphamapRes / 2f), alphamapRes, alphamapRes);

        alphamapGrass = new byte[alphamapRes * alphamapRes];
        alphamapRocks = new byte[alphamapRes * alphamapRes];
        alphamapMuds = new byte[alphamapRes * alphamapRes];
        alphamapSands = new byte[alphamapRes * alphamapRes];
        alphamapSandRocks = new byte[alphamapRes * alphamapRes];
        alphamapSnows = new byte[alphamapRes * alphamapRes];
        alphamapSnowRocks = new byte[alphamapRes * alphamapRes];
        alphamapGravel = new byte[alphamapRes * alphamapRes];

        for (int x = 0; x < fetchAlphamaps.GetLength(0); x++)
        {
            for (int y = 0; y < fetchAlphamaps.GetLength(1); y++)
            {
                alphamapGrass[x * alphamapRes + y] = AM_floatToByte(fetchAlphamaps[x, y, 0]);
                alphamapRocks[x * alphamapRes + y] = AM_floatToByte(fetchAlphamaps[x, y, 1]);
                alphamapMuds[x * alphamapRes + y] = AM_floatToByte(fetchAlphamaps[x, y, 2]);
                alphamapSands[x * alphamapRes + y] = AM_floatToByte(fetchAlphamaps[x, y, 3]);
                alphamapSandRocks[x * alphamapRes + y] = AM_floatToByte(fetchAlphamaps[x, y, 4]);
                alphamapSnows[x * alphamapRes + y] = AM_floatToByte(fetchAlphamaps[x, y, 5]);
                alphamapSnowRocks[x * alphamapRes + y] = AM_floatToByte(fetchAlphamaps[x, y, 6]);
                alphamapGravel[x * alphamapRes + y] = AM_floatToByte(fetchAlphamaps[x, y, 7]);
            }
        }

        //4. Water level
        this.waterLevel = waterLevel;

        //5. Map Objects
        List<MapObject> nonFilteredMO = LevelData.mapObjects;
        List<MapObject> filteredMO = new List<MapObject>();

        for (int j = 0; j < nonFilteredMO.Count; j++)
        {
            if (checkPlayableAreaBounds(mapWidth, nonFilteredMO[j].transform.position))
                filteredMO.Add(nonFilteredMO[j]);
        }

        mapObject_ids = new string[filteredMO.Count];
        mapObject_transform = new MapLevelManager.Transfornm3Ser[filteredMO.Count];

        for (int j = 0; j < filteredMO.Count; j++)
        {
            MapObject mo = filteredMO[j];
            Vector3 p = mo.gameObject.transform.position;
            Vector3 r = mo.gameObject.transform.localRotation.eulerAngles;

            mapObject_ids[j] = mo.objectId;
            mapObject_transform[j] = new MapLevelManager.Transfornm3Ser(p.x, p.y, p.z, r.x, r.y, r.z);
        }

        //6. Grass/Details/Flowers
        int detailRes = tData.detailResolution;
        detailRes = Mathf.CeilToInt(detailRes / 2f);

        detailGrass = new bool[detailRes, detailRes];
        int[,] detailGrassInt = tData.GetDetailLayer(Mathf.CeilToInt(detailRes / 2f), Mathf.CeilToInt(detailRes / 2f), detailRes, detailRes, 0);

        for (int x = 0; x < detailGrassInt.GetLength(0); x++)
        {
            for (int y = 0; y < detailGrassInt.GetLength(1); y++)
            {
                detailGrass[x, y] = detailGrassInt[x, y] == 1;
            }
        }

        //7. Buildings, Towers, Units
        List<Unit> unitsNonFiltered = LevelData.units;
        List<Unit> units = new List<Unit>();

        //Filter units that are in playable area
        for (int j = 0; j < unitsNonFiltered.Count; j++)
        {
            if (checkPlayableAreaBounds(mapWidth, unitsNonFiltered[j].gameObject.transform.position))
            {
                units.Add(unitsNonFiltered[j]);
            }
        }

        unitData = new MapLevelManager.UnitSerializable[units.Count];
        int i = 0;

        for (int j = 0; j < units.Count; j++)
        {
            if (!checkPlayableAreaBounds(mapWidth, units[j].gameObject.transform.position))
                continue;

            unitData[i] = units[j].serializeUnit();
            i++;
        }

        //8. Team stats
        teamStats = LevelData.teamStats;
        for (int j = 0; j < teamStats.Length; j++)
        {
            teamStats[j].OnSerialize();
        }

        //9. Weather environment
        timeClock = LevelData.environment.time;
        timeStatic = LevelData.environment.timeStatic;
        weatherDefault = LevelData.environment.weatherDefault;
        weatherEvents = new WorldEnvironment.Event[LevelData.environment.events.Count];
        for (int j = 0; j < LevelData.environment.events.Count; j++)
        {
            weatherEvents[j] = LevelData.environment.events[j];
        }
    }
    #endregion

    #region Creating level
    public ML_03_05() { }
    #endregion

    #region Load level
    public void LoadData(Terrain terrain, Transform parentTrans, bool editorMode)
    {
        TerrainData tData = terrain.terrainData;

        //1. Map size
        if (editorMode)
        {
            tData.size = new Vector3(mapWidth, tData.size.y, mapHeight);
        }
        else
        {
            mapWidth = (int)(mapWidth / 2f);
            mapHeight = (int)(mapHeight / 2f);
            tData.size = new Vector3(mapWidth * 2, tData.size.y, mapHeight * 2);
        }

        //Navigation
        LevelData.navigations_agentTypeID = navigations_agentTypeID;

        //2. Terrain heights
        int heightmapRes = tData.heightmapResolution;
        if (editorMode)
            heightmapRes = Mathf.CeilToInt(heightmapRes / 2f);

        float[,] fetchTerrainHeights;

        if (editorMode)
            fetchTerrainHeights = tData.GetHeights(Mathf.CeilToInt(heightmapRes / 2f), Mathf.CeilToInt(heightmapRes / 2f), heightmapRes, heightmapRes);
        else
            fetchTerrainHeights = tData.GetHeights(0, 0, heightmapRes, heightmapRes);

        for (int x = 0; x < fetchTerrainHeights.GetLength(0); x++)
        {
            for (int y = 0; y < fetchTerrainHeights.GetLength(1); y++)
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
                //Format: [Y, X]
                fetchTerrainHeights[0, i] = 0;
                fetchTerrainHeights[i, 0] = 0;
                fetchTerrainHeights[TH_height, i] = 0;
                fetchTerrainHeights[i, TH_width] = 0;
            }
        }

        //Finally apply
        if (editorMode)
            tData.SetHeights(Mathf.CeilToInt(heightmapRes / 2f), Mathf.CeilToInt(heightmapRes / 2f), fetchTerrainHeights);
        else
            tData.SetHeights(0, 0, fetchTerrainHeights);

        //3. Alphamaps
        int alphamapRes = tData.alphamapResolution;
        if (editorMode)
            alphamapRes = Mathf.CeilToInt(alphamapRes / 2f);

        float[,,] fetchAlphamaps;

        if (editorMode)
            fetchAlphamaps = tData.GetAlphamaps(Mathf.CeilToInt(alphamapRes / 2f), Mathf.CeilToInt(alphamapRes / 2f), alphamapRes, alphamapRes);
        else
            fetchAlphamaps = tData.GetAlphamaps(0, 0, alphamapRes, alphamapRes);

        for (int x = 0; x < fetchAlphamaps.GetLength(0); x++)
        {
            for (int y = 0; y < fetchAlphamaps.GetLength(1); y++)
            {
                fetchAlphamaps[x, y, 0] = AM_byteToFloat(alphamapGrass[x * alphamapRes + y]);
                fetchAlphamaps[x, y, 1] = AM_byteToFloat(alphamapRocks[x * alphamapRes + y]);
                fetchAlphamaps[x, y, 2] = AM_byteToFloat(alphamapMuds[x * alphamapRes + y]);
                fetchAlphamaps[x, y, 3] = AM_byteToFloat(alphamapSands[x * alphamapRes + y]);
                fetchAlphamaps[x, y, 4] = AM_byteToFloat(alphamapSandRocks[x * alphamapRes + y]);
                fetchAlphamaps[x, y, 5] = AM_byteToFloat(alphamapSnows[x * alphamapRes + y]);
                fetchAlphamaps[x, y, 6] = AM_byteToFloat(alphamapSnowRocks[x * alphamapRes + y]);
                fetchAlphamaps[x, y, 7] = AM_byteToFloat(alphamapGravel[x * alphamapRes + y]);
            }
        }

        if (editorMode)
            tData.SetAlphamaps(Mathf.CeilToInt(alphamapRes / 2f), Mathf.CeilToInt(alphamapRes / 2f), fetchAlphamaps);
        else
            tData.SetAlphamaps(0, 0, fetchAlphamaps);

        //4.Water

        //5. Map Objects
        for (int i = 0; i < mapObject_ids.Length; i++)
        {
            string mo_id = mapObject_ids[i];
            MapLevelManager.Transfornm3Ser t = mapObject_transform[i];

            if (!editorMode)
            {
                t.posX -= mapWidth;
                t.posZ -= mapHeight;
            }

            GameObject mo_gm = GameObject.Instantiate(GlobalList.mapObjects[mo_id].gameObject, t.toVectorPosition(), Quaternion.identity, parentTrans);
            mo_gm.transform.localRotation = Quaternion.Euler(t.rotX, t.rotY, t.rotZ);
            LevelData.mapObjects.Add(mo_gm.GetComponent<MapObject>());
        }

        //6. Grass/Details/Flowers
        int detailRes = tData.detailResolution;
        if (editorMode)
            detailRes = Mathf.CeilToInt(detailRes / 2f);

        int[,] detailGrassInt = new int[detailRes, detailRes];
        int[,] detailSandGrassInt = new int[detailRes, detailRes];

        for (int x = 0; x < detailGrassInt.GetLength(0); x++)
        {
            for (int y = 0; y < detailGrassInt.GetLength(1); y++)
            {
                if (fetchAlphamaps[x, y, 3] > 0.25f)
                {
                    detailSandGrassInt[x, y] = detailGrass[x, y] ? 1 : 0;
                }
                else
                    detailGrassInt[x, y] = detailGrass[x, y] ? 1 : 0;
            }
        }

        if (editorMode)
        {
            tData.SetDetailLayer(Mathf.CeilToInt(detailRes / 2f), Mathf.CeilToInt(detailRes / 2f), 0, detailGrassInt);
            tData.SetDetailLayer(Mathf.CeilToInt(detailRes / 2f), Mathf.CeilToInt(detailRes / 2f), 1, detailSandGrassInt);
        }
        else
        {
            tData.SetDetailLayer(0, 0, 0, detailGrassInt);
            tData.SetDetailLayer(0, 0, 1, detailSandGrassInt);
        }

        //Generate navigation
        LevelData.mainTerrain = terrain;
        LevelData.buildGameNavigationMesh();

        //7. Buildings, Towers, Units
        for (int j = 0; j < unitData.Length; j++)
        {
            MapLevelManager.UnitSerializable data = unitData[j];

            if (!editorMode)
            {
                Vector3 position = data.getV(KEY_BODY_POSITION);
                position.x -= mapWidth;
                position.z -= mapHeight;
                data.setV(KEY_BODY_POSITION, position);
            }

            spawnUnit(data);
        }

        //8. Team stats
        LevelData.teamStats = teamStats;

        //9. Wearher environment
        LevelData.environment.time = timeClock;
        LevelData.environment.timeStatic = timeStatic;
        LevelData.environment.weatherDefault = weatherDefault;
        LevelData.environment.events.Clear();
        for (int j = 0; j < weatherEvents.Length; j++)
        {
            LevelData.environment.events.Add(weatherEvents[j]);
        }
    }
    #endregion

    #region Tool functions
    private static short HM_floatToShort(float value)
    {
        return (short)(value * short.MaxValue);
    }

    private static float HM_shortToFloat(short value)
    {
        return (float)value / short.MaxValue;
    }

    private static byte AM_floatToByte(float value)
    {
        return (byte)(value * 255);
    }

    private static float AM_byteToFloat(byte value)
    {
        return (float)value / 255f;
    }

    public static bool checkPlayableAreaBounds(float mapWidth, Vector3 pos)
    {
        if (mapWidth * 0.25f < pos.x && mapWidth * 0.75f > pos.x)
        {
            if (mapWidth * 0.25f < pos.z && mapWidth * 0.75f > pos.z)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}
