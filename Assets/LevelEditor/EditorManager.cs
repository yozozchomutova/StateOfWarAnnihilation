using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class EditorManager : MonoBehaviour
{
    [SerializeField, SerializeReference] public SelectUnitPanel[] inactiveStartCallers;

    public SelectUnitPanel unitsPanel;
    public SelectUnitPanel airForceSelectPanel;

    public LETeamConfiguration teamCfgPrefab;
    public Transform teamCfgFolder;

    public BarBuildings barBuildings;
    public PanelNewLevel panelNewLevel;

    [HideInInspector] private LETeamConfiguration[] teamCfgs;

    //For LevelData
    public Terrain terrain;

    [Header("Audio")]
    public AudioSource bird1;
    public AudioSource bird2;
    public AudioSource bird3;

    public float minBirdSoundDistance;
    public float maxBirdSoundDistance;
    private float curBirdSoundDistance;

    //Saving level and level info
    [Header("Saving level and level info")]
    public PanelSaveConfirm panelSaveLevel;
    [HideInInspector] public string levelName = "";

    public BarMapObjects barMapObjects;
    public BarNavigations barNavigations;
    public PanelLevelInfo panelLevelInfo;

    [Header("Ctrl tooltips")]
    public GameObject ctrlTipsPanel;

    [Header("Reference")]
    public BarTerrainEdit barTerrainEdit;

    //Undo/Redo system
    public volatile List<UndoType> undoTypes = new List<UndoType>();
    public volatile List<byte[][]> undoTypesData = new List<byte[][]>();

    public volatile List<UndoType> redoTypes = new List<UndoType>();
    public volatile List<byte[][]> redoTypesData = new List<byte[][]>();

    void Start()
    {
        //Connect with LevelData
        LevelData.mainTerrain = terrain;

        LevelData.Init();

        //Set itself as parent of all Units
        MapLevel.setStaticParentTrans(transform);

        //Call all inactive starts
        for (int i = 0; i < inactiveStartCallers.Length; i++)
        {
            ((InactiveStartCaller)inactiveStartCallers[i]).InactiveStart();
        }

        //Teams
        Team[] teams = GlobalList.teams;
        TeamStats02_12[] teamStats = LevelData.teamStats;
        teamCfgs = new LETeamConfiguration[teams.Length];

        for (int i = 0; i < teams.Length; i++)
        {
            LETeamConfiguration teamCfg = Instantiate(teamCfgPrefab, teamCfgFolder);

            teamCfg.bindTeamStats(teamStats[i]);

            teamCfg.gameObject.SetActive(false);
            teamCfgs[i] = teamCfg;
        }

        //Create new level
        panelNewLevel.Create();

        //Change discord RPC
        DiscordInitializer.activity.Details = "In Level Editor";
        DiscordInitializer.activity.Timestamps.Start = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        DiscordInitializer.updateActivity();

        //Bird audio
        curBirdSoundDistance = UnityEngine.Random.Range(minBirdSoundDistance, maxBirdSoundDistance);
    }

    private void Update()
    {
        //Rotate skybox
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * 0.2f);

        handleBirdAudio();

        //Handle shortcuts
        ctrlTipsPanel.SetActive(Input.GetKey(KeyCode.LeftControl));
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                //TODO - If level wasnt never saved before, open panel... Otherwise overwrite level...
                /*if (string.IsNullOrEmpty(levelName))
                {
                    panelSaveLevel.show();
                } else
                {

                }*/
                panelSaveLevel.gameObject.SetActive(true);
            } else if (Input.GetKeyDown(KeyCode.J))
            {
                actionUndo();
            } else if (Input.GetKeyDown(KeyCode.H))
            {
                actionRedo();
            }
        }
    }

    public void showPlayerSettingsTabs()
    {
        int curY = 0;

        for (int i = 0; i < LevelData.units.Count; i++)
        {
            Unit unit = LevelData.units[i];

            if (unit.id != "0_commandCenter1")
                continue;

            int teamID = unit.team.id;
            if (!teamCfgs[teamID].gameObject.activeSelf)
            {
                teamCfgs[teamID].loadFrom();
                teamCfgs[teamID].gameObject.GetComponent<RectTransform>().localPosition = new Vector2(0, curY);
                teamCfgs[teamID].gameObject.SetActive(true);
                curY -= 51;
            }
        }
    }

    public void ResetGame()
    {
        LevelData.ResetGame();
        reloadTeamsConfiguration();
        levelName = "";
    }

    public void hidePlayerSettingsTabs()
    {
        for (int i = 0; i < teamCfgs.Length; i++)
        {
            teamCfgs[i].saveTo();
            teamCfgs[i].gameObject.SetActive(false);
        }
    }

    public void reloadTeamsConfiguration()
    {
        for (int i = 0; i < teamCfgs.Length; i++)
        {
            teamCfgs[i].loadFrom();
        }
    }

    private void handleBirdAudio()
    {
        curBirdSoundDistance -= Time.deltaTime;
        if (curBirdSoundDistance < 0)
        {
            curBirdSoundDistance = UnityEngine.Random.Range(minBirdSoundDistance, maxBirdSoundDistance);

            //Random bird sound
            float randomBirdSound = UnityEngine.Random.Range(0f, 1f);
            if (randomBirdSound < 0.333f)
            {
                bird1.Play();
            } else if (randomBirdSound < 0.666f)
            {
                bird2.Play();
            } else
            {
                bird3.Play();
            }
        }
    }

    public void saveLevel(string levelName)
    {
        //Create SavedLevels folder, if not created already
        Directory.CreateDirectory(Application.persistentDataPath + "/SavedLevels/");

        //Setup
        string path = Application.persistentDataPath + "/SavedLevels/" + levelName + ".lvl";

        FileStream stream = new FileStream(path, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(stream);

        //Write main informations
        bw.Write((byte)int.Parse(GameManager.getBigPatchVer())); //Big patch
        bw.Write((byte)int.Parse(GameManager.getSmallPatchVer())); //Small patch
        bw.Write((byte)GameManager.getTagVersionId()); //Build code
        bw.Write(calculateDifficulty()); //Difficulty
        bw.Write((int)PanelNewLevel.normalMapSize); //Map width
        bw.Write((int)PanelNewLevel.normalMapSize); //Map height
        bw.Write((int)panelLevelInfo.description.text.Length); //Desc length
        bw.Write((int)panelLevelInfo.imageBytes.Length); //Img length

        ML_03_05 mapLevel = new ML_03_05(LevelData.mainTerrain, barMapObjects, barBuildings, barNavigations);
        byte[] mapLevelData = CompressionManager.Compress(Serialize(mapLevel));

        bw.Write((int)mapLevelData.Length); //Map level data length

        bw.Write((byte[])Encoding.UTF8.GetBytes(panelLevelInfo.description.text)); //Desc
        bw.Write((byte[])panelLevelInfo.imageBytes); //Img data
        bw.Write(mapLevelData);

        bw.Close();
        stream.Close();
    }

    public static byte[] Serialize(object obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public int calculateDifficulty()
    {
        int blueUnitCount = 0, allUnitCount = 0; //allUnitCount = all map units except neutral/white

        for (int i = 0; i < LevelData.units.Count; i++)
        {
            int unitTeam = LevelData.units[i].team.id;
            if (unitTeam != Team.WHITE)
            {
                allUnitCount++;
            }

            if (unitTeam == Team.BLUE)
            {
                blueUnitCount++;
            }
        }

        return (int)((float)blueUnitCount / allUnitCount * 100f);
    }

    public void createUndoAction(UndoType undoType, byte[][] data)
    {
        if (undoTypes.Count > 4) //Limit to 5 undo max
        {
            undoTypes.RemoveAt(0);
            undoTypesData.RemoveAt(0);
        }

        undoTypes.Add(undoType);
        undoTypesData.Add(data);
    }

    public void createRedoAction(UndoType redoType, byte[][] data)
    {
        if (redoTypes.Count > 4) //Limit to 5 redo max
        {
            redoTypes.RemoveAt(0);
            redoTypesData.RemoveAt(0);
        }

        redoTypes.Add(redoType);
        redoTypesData.Add(data);
    }

    public void actionUndo()
    {
        if (undoTypes.Count == 0)
            return;

        UndoType undoType = undoTypes[undoTypes.Count - 1];
        byte[][] data = undoTypesData[undoTypesData.Count - 1];

        if (undoType == UndoType.TERRAIN_PAINT)
        {
            TerrainData tData = terrain.terrainData;

            int alphamapRes = tData.alphamapResolution;
            float[,,] fetchAlphamaps = tData.GetAlphamaps(0, 0, alphamapRes, alphamapRes);

            for (int i = 0; i < fetchAlphamaps.GetLength(2); i++)
            {
                for (int x = 0; x < fetchAlphamaps.GetLength(0); x++)
                {
                    for (int y = 0; y < fetchAlphamaps.GetLength(1); y++)
                    {
                        fetchAlphamaps[x, y, i] = BitConverter.ToSingle(data[x * alphamapRes + y + (alphamapRes * alphamapRes * i)]);
                    }
                }
            }

            terrain.terrainData.SetAlphamaps(0, 0, fetchAlphamaps);
        }

        undoTypes.RemoveAt(undoTypes.Count - 1);
        undoTypesData.RemoveAt(undoTypesData.Count - 1);
    }

    public void actionRedo()
    {
        if (redoTypes.Count == 0)
            return;

        UndoType redoType = redoTypes[redoTypes.Count - 1];
        byte[][] data = redoTypesData[redoTypesData.Count - 1];

        redoTypes.RemoveAt(redoTypes.Count - 1);
        redoTypesData.RemoveAt(redoTypesData.Count - 1);
    }

    public interface InactiveStartCaller
    {
        void InactiveStart();
    }

    public enum UndoType
    {
        TERRAIN_EDIT,
        TERRAIN_PAINT,
        WATER_LEVEL,
        PLACE_UNIT,
        REMOVE_UNIT
    }
}
