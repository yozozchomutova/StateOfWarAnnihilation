using System.Collections.Generic;
using UnityEngine;
using static MapLevel;
using static MapLevelManager;
using static MapLevelManager.UnitSerializable;

[System.Serializable]
public class ML_02_12
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
    public string[] unitIds;
    public UnitLE.UnitLESer[] unitData;

    //Team stats
    public TeamStats02_12[] teamStats;

    #endregion

    #region Load level
    public void LoadData(Terrain terrain, Transform water, Transform parentTrans, bool editorMode)
    {
        TerrainData tData = terrain.terrainData;

        //1. Map size
        if (editorMode) {
            tData.size = new Vector3(mapWidth, tData.size.y, mapHeight);
        } else {
            mapWidth = (int)(mapWidth / 2f);
            mapHeight = (int)(mapHeight / 2f);
            tData.size = new Vector3(mapWidth*2, tData.size.y, mapHeight*2);
        }

        //Navigation
        LevelData.navigations_agentTypeID = navigations_agentTypeID;

        //2. Terrain heights
        int heightmapRes = tData.heightmapResolution;
        if (editorMode)
            heightmapRes = Mathf.CeilToInt(heightmapRes / 2f);

        float[,] fetchTerrainHeights;

        if (editorMode)
            fetchTerrainHeights = tData.GetHeights(Mathf.CeilToInt(heightmapRes/2f), Mathf.CeilToInt(heightmapRes / 2f), heightmapRes, heightmapRes);
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
        water.position = new Vector3(mapWidth, waterLevel, mapHeight);

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

        for (int x = 0; x < detailGrassInt.GetLength(0); x++) {
            for (int y = 0; y < detailGrassInt.GetLength(1); y++) {
                if (fetchAlphamaps[x, y, 3] > 0.25f)
                {
                    detailSandGrassInt[x, y] = detailGrass[x, y] ? 1 : 0;
                } else
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
        for (int j = 0; j < unitIds.Length; j++)
        {
            UnitLE.UnitLESer data = unitData[j];

            if (!editorMode)
            {
                data.position.x -= mapWidth;
                data.position.z -= mapHeight;
            }

            //Convert Old UnitLE.UnitLESer to new UnitSerialization
            Dictionary<string, string> dictionaryData = new Dictionary<string, string>();

            dictionaryData.Add(KEY_UNIT_ID, unitIds[j]);
            dictionaryData.Add(KEY_UNIT_HP, "" + Mathf.Min(1f, data.health / GlobalList.units[unitIds[j]].hpMax));
            dictionaryData.Add(KEY_UNIT_TEAMID, "" + data.unitTeamID);
            dictionaryData.Add(KEY_BODY_POSITION, "" + data.position.parseToString());

            Vector3Ser unitRotation = new Vector3Ser(new Vector3(GlobalList.units[unitIds[j]].transform.localEulerAngles.x, data.unitRotation.y, data.unitRotation.z));
            dictionaryData.Add(KEY_UNIT_ROTATION, "" + unitRotation.parseToString());
            dictionaryData.Add(KEY_BODY_ROTATION, "" + unitRotation.parseToString());

            switch (unitIds[j])
            {
                case "0_windTurbine1":
                    dictionaryData.Add("producingUnits", "0_energy1;0_energy1;0_energy1");
                    break;
                case "0_goldMachine1":
                    dictionaryData.Add("producingUnits", "0_goldbrick1;0_goldbrick1;0_goldbrick1");
                    break;
                case "0_researchStation1":
                    dictionaryData.Add("producingUnits", "0_research1;0_research1;0_research1");
                    break;
            }

            //Now custom data to dictionary
            for (int i = 0; i < data.additionalData.values.Length; i++)
            {
                if (!dictionaryData.ContainsKey(data.additionalData.keys[i]))
                dictionaryData.Add(data.additionalData.keys[i], data.additionalData.values[i]);
            }

            UnitSerializable newSer = new UnitSerializable(dictionaryData);
            spawnUnit(newSer);
        }

        //8. Team stats
        LevelData.teamStats = teamStats;
    }
    #endregion

    #region Tool functions
    private static short HM_floatToShort(float value)
    {
        return (short) (value * short.MaxValue);
    }

    private static float HM_shortToFloat(short value)
    {
        return (float)value / short.MaxValue;
    }

    private static byte AM_floatToByte(float value)
    {
        return (byte) (value * 255);
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
