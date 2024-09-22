using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;

public class LevelData
{
    //Scene
    public static Scene scene = Scene.NONE;

    //Map data
    public static GridManager gridManager = new GridManager();
    public static List<Unit> units = new List<Unit>();
    public static List<MapObject> mapObjects = new List<MapObject>();

    //Player stats
    public static TeamStats02_12[] teamStats;
    public static TeamStats02_12 ts;  //Current player

    //Environment
    public static WorldEnvironment environment;

    public static void Init()
    {
        //Team Stats
        Team[] teams = GlobalList.teams;
        teamStats = new TeamStats02_12[teams.Length];
        for (int i = 0; i < teamStats.Length; i++)
        {
            teamStats[i] = new TeamStats02_12();
            teamStats[i].teamId = teams[i].id;
        }

        //Reset game for start
        ResetGame();
    }

    public static void ResizeTerrain(float mapSize, TerrainEdging te, int sizeForTE, bool scaleMapObjects)
    {
        //Adapt all map objects position to terrain size
        if (scaleMapObjects)
        {
            float quarterMapSize = (int)(Terrain.activeTerrain.terrainData.size.x / 4f);
            float halfMapSize = (int)(Terrain.activeTerrain.terrainData.size.x / 2f);

            float newQuarterMapSize = (int)(mapSize / 4f);
            float newHalfMapSize = (int)(mapSize / 2f);
            for (int i = LevelData.units.Count - 1; i >= 0; i--)
            {
                Unit u = LevelData.units[i];
                Vector3 oldPos = u.gameObject.transform.position;
                Vector3 relativePos = new Vector3((oldPos.x - quarterMapSize) / halfMapSize, oldPos.y, (oldPos.z - quarterMapSize) / halfMapSize);
                if (relativePos.x < 0 || relativePos.x > 1 || relativePos.z < 0 || relativePos.z > 1)
                {
                    u.destroyBoth();
                    continue;
                }
                relativePos = new Vector3(relativePos.x * newHalfMapSize + newQuarterMapSize, relativePos.y, relativePos.z * newHalfMapSize + newQuarterMapSize);

                if (u.body != null)
                    u.body.transform.position = relativePos;
                else
                    u.transform.position = relativePos;
            }

            for (int i = mapObjects.Count - 1; i >= 0; i--)
            {
                MapObject m = mapObjects[i];
                Vector3 oldPos = m.gameObject.transform.position;
                Vector3 relativePos = new Vector3((oldPos.x - quarterMapSize) / halfMapSize, oldPos.y, (oldPos.z - quarterMapSize) / halfMapSize);
                if (relativePos.x < 0 || relativePos.x > 1 || relativePos.z < 0 || relativePos.z > 1)
                {
                    mapObjects.Remove(m);
                    GameObject.Destroy(m.gameObject);
                    continue;
                }

                m.transform.position = new Vector3(relativePos.x * newHalfMapSize + newQuarterMapSize, relativePos.y, relativePos.z * newHalfMapSize + newQuarterMapSize);
            }
        }

        //Create new level:
        TerrainData tData = Terrain.activeTerrain.terrainData;

        //Size
        tData.size = new Vector3(mapSize, tData.size.y, mapSize);

        //Grid
        gridManager.NewGrid(tData, mapSize, mapSize);

        //Terrain edging update
        if (te != null)
        {
            te.updateEdges(sizeForTE);
        }
    }

    public static void clearTerrain()
    {
        TerrainData tData = Terrain.activeTerrain.terrainData;

        //Flatten terrain
        int heightMapRes = tData.heightmapResolution;
        float[,] mapHeights = tData.GetHeights(0, 0, heightMapRes, heightMapRes);

        for (int x = 0; x < mapHeights.GetLength(0); x++)
        {
            for (int y = 0; y < mapHeights.GetLength(1); y++)
            {
                mapHeights[x, y] = PanelNewLevel.terrainHeightLvl1;
            }
        }

        tData.SetHeights(0, 0, mapHeights);

        //Clear textures
        int alphamapRes = tData.alphamapResolution;
        float[,,] mapAlphamaps = tData.GetAlphamaps(0, 0, alphamapRes, alphamapRes);
        for (int alphamapIndex = 0; alphamapIndex < mapAlphamaps.GetLength(2); alphamapIndex++)
        {
            for (int x = 0; x < alphamapRes; x++)
            {
                for (int y = 0; y < alphamapRes; y++)
                {
                    mapAlphamaps[x, y, alphamapIndex] = alphamapIndex == 0 ? 1f : 0f;
                }
            }
        }

        tData.SetAlphamaps(0, 0, mapAlphamaps);

        //Clear all details
        int detailRes = tData.detailResolution;
        tData.SetDetailLayer(0, 0, 0, new int[detailRes, detailRes]);

        //Clear map objects
        clearAllMapObjects();
    }

    public static void clearAllMapObjects()
    {
        foreach (MapObject mapObject in mapObjects)
        {
            if (mapObject != null)
                GameObject.Destroy(mapObject.gameObject);
        }

        mapObjects.Clear();
    }

    public static void clearUnits()
    {
        for (int i = units.Count-1; i >= 0; i--)
            if (units[i] != null)
                units[i].destroyBoth();

        units.Clear();
    }

    public static void ResetGame()
    {
        ResetGame(PanelNewLevel.normalMapSize);
    }

    public static void ResetGame(int mapSize)
    {
        LevelData.gridManager.Nullate();

        //Clear terrain
        ResizeTerrain(mapSize, Terrain.activeTerrain.GetComponent<TerrainEdging>(), mapSize, false);
        ClearGame();

        //(re)Start environemnt
        environment = new WorldEnvironment();
        LevelData.environment.init(
            GameObject.FindObjectOfType<ReflectionProbe>(),
            GameObject.Find("Sun").GetComponent<Light>());
    }

    public static void ClearGame()
    {
        clearTerrain();
        clearUnits();
        clearTeamStats();
    }

    public static void clearTeamStats()
    {
        for (int i = 0; i < teamStats.Length; i++)
            teamStats[i].clear();
    }

    //Compares teamStats/Ids
    public static bool tsCmp(int teamId)
    {
        if (ts == null)
            return false;

        return teamId == ts.teamId;
    }

    #region [Structs] Scene
    public enum Scene
    {
        NONE,
        GAME,
        EDITOR
    }
    #endregion
}
