using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TMPro;
using UnityEngine;
using static MapLevelManager;

public class EditorManager : MonoBehaviour
{
    public static EditorManager self;

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

    [Header("Undo/Redo")]
    public TMP_Text undoText;
    public TMP_Text redoText;

    public volatile List<byte[]> undosData = new List<byte[]>();
    public volatile List<UndoType> undosDataType = new List<UndoType>();
    public volatile List<byte[]> redosData = new List<byte[]>();
    public volatile List<UndoType> redosDataType = new List<UndoType>();

    void Start()
    {
        self = this;

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
            }
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            actionUndo();
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            actionRedo();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (GlobalList.gridParent.activeSelf)
                LevelData.gridManager.HideGrid();
            else
                LevelData.gridManager.ShowGrid();
        }
    }

    public void DestroyUnit(int gridX, int gridY)
    {
        Unit u = LevelData.gridManager.tiles[gridX, gridY].unit;
        LevelData.gridManager.RemoveUnit(gridX, gridY);
        u.destroyBoth();
    }

    public void DestroyMapObject(int gridX, int gridY)
    {
        MapObject m = LevelData.gridManager.tiles[gridX, gridY].mapObject;
        LevelData.gridManager.RemoveMapObject(gridX, gridY);
        LevelData.mapObjects.Remove(m);
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
                bird1.Play();
            else if (randomBirdSound < 0.666f)
                bird2.Play();
            else
                bird3.Play();
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

    private void UpdateUndoRedoUI()
    {
        undoText.text = "" + undosDataType.Count;
        redoText.text = "" + redosDataType.Count;
    }

    public void createUndoAction(UndoType uType, byte[] data, int arrayPos, bool terminateRedos = true)
    {
        if (terminateRedos)
        {
            redosData.Clear();
            redosDataType.Clear();
        }

        if (undosData.Count >= 10) {//Limit to 10 undos max
            int lastIndex = undosData.Count - 1;
            undosData.RemoveAt(lastIndex);
            undosDataType.RemoveAt(lastIndex);
        }
        undosData.Insert(arrayPos, data);
        undosDataType.Insert(arrayPos, uType);
        UpdateUndoRedoUI();
    }

    public void createRedoAction(UndoType rType, byte[] data)
    {
        if (redosData.Count >= 10) {//Limit to 10 redos max
            int lastIndex = redosData.Count - 1;
            redosData.RemoveAt(lastIndex);
            redosDataType.RemoveAt(lastIndex);
        }
        redosData.Insert(0, data);
        redosDataType.Insert(0, rType);
        UpdateUndoRedoUI();
    }

    public void actionUndo()
    {
        if (undosData.Count == 0) return;
        UndoType undoType = undosDataType[0];
        byte[] data = undosData[0];

        TerrainData tData = terrain.terrainData;

        switch (undoType)
        {
            case UndoType.TERRAIN_EDIT:
                //Generate redo option
                float[,] heights = tData.GetHeights(0, 0, tData.heightmapResolution, tData.heightmapResolution);
                byte[] newData = ArrayWorker.Float2DToByte1D(heights);
                createRedoAction(UndoType.TERRAIN_EDIT, newData);

                //Process undo
                int heightsmapRes = tData.heightmapResolution;
                heights = ArrayWorker.Byte1DToFloat2D(data, heightsmapRes, heightsmapRes);
                tData.SetHeights(0, 0, heights);
                break;
            case UndoType.TERRAIN_PAINT:
                //Generate redo option
                float[,,] textures = tData.GetAlphamaps(0, 0, tData.alphamapResolution, tData.alphamapResolution);
                newData = ArrayWorker.Float3DToByte1D(textures);
                createRedoAction(UndoType.TERRAIN_PAINT, newData);

                //Process undo
                int alphamapRes = tData.alphamapResolution;
                textures = ArrayWorker.Byte1DToFloat3D(data, alphamapRes, alphamapRes, tData.alphamapLayers);
                tData.SetAlphamaps(0, 0, textures);
                break;
            case UndoType.PLACE_UNIT:
                int gridX = BitConverter.ToInt32(data, 0);
                int gridY = BitConverter.ToInt32(data, 4);

                //Generate redo option
                Unit u = LevelData.gridManager.tiles[gridX, gridY].unit;
                UnitSerializable unitS = u.serializeUnit();
                newData = ArrayWorker.SerializableToBytes(unitS);
                createRedoAction(UndoType.PLACE_UNIT, newData);

                //Process undo - remove unit again
                DestroyUnit(gridX, gridY);
                break;
            case UndoType.REMOVE_UNIT:
                //Process undo - spawn unit back
                unitS = ArrayWorker.BytesToSerializable<UnitSerializable>(data);
                MapLevel.spawnUnit(unitS);

                //Generate redo option
                Vector3 unitPos = unitS.getV(UnitSerializable.KEY_BODY_POSITION);
                (gridX, gridY) = LevelData.gridManager.SamplePosition(unitPos.x, unitPos.z);
                
                newData = new byte[8];
                BitConverter.GetBytes(gridX).CopyTo(newData, 0); //GridX
                BitConverter.GetBytes(gridY).CopyTo(newData, 4); //GridY
                createRedoAction(UndoType.REMOVE_UNIT, newData);
                break;
        }

        undosData.RemoveAt(0);
        undosDataType.RemoveAt(0);
        UpdateUndoRedoUI();
    }

    public void actionRedo()
    {
        if (redosData.Count == 0) return;
        UndoType redoType = redosDataType[0];
        byte[] data = redosData[0];

        TerrainData tData = terrain.terrainData;

        switch (redoType)
        {
            case UndoType.TERRAIN_EDIT:
                //Generate undo option
                float[,] heights = tData.GetHeights(0, 0, tData.heightmapResolution, tData.heightmapResolution);
                byte[] newData = ArrayWorker.Float2DToByte1D(heights);
                createUndoAction(UndoType.TERRAIN_EDIT, newData, 0, false);

                //Process redo
                int heightsmapRes = tData.heightmapResolution;
                heights = ArrayWorker.Byte1DToFloat2D(data, heightsmapRes, heightsmapRes);
                tData.SetHeights(0, 0, heights);
                break;
            case UndoType.TERRAIN_PAINT:
                //Create undo option
                float[,,] textures = tData.GetAlphamaps(0, 0, tData.alphamapResolution, tData.alphamapResolution);
                newData = ArrayWorker.Float3DToByte1D(textures);
                createUndoAction(UndoType.TERRAIN_PAINT, newData, 0, false);

                //Process redo
                int alphamapRes = tData.alphamapResolution;
                textures = ArrayWorker.Byte1DToFloat3D(data, alphamapRes, alphamapRes, tData.alphamapLayers);
                tData.SetAlphamaps(0, 0, textures);
                break;
            case UndoType.PLACE_UNIT:
                //Process redo - new unit
                UnitSerializable unitS = ArrayWorker.BytesToSerializable<UnitSerializable>(data);
                MapLevel.spawnUnit(unitS);

                //Create undo option
                Vector3 unitPos = unitS.getV(UnitSerializable.KEY_BODY_POSITION);
                var (gridX, gridY) = LevelData.gridManager.SamplePosition(unitPos.x, unitPos.z);

                newData = new byte[8];
                BitConverter.GetBytes(gridX).CopyTo(newData, 0); //GridX
                BitConverter.GetBytes(gridY).CopyTo(newData, 4); //GridY
                createUndoAction(UndoType.PLACE_UNIT, newData, 0, false);
                break;
            case UndoType.REMOVE_UNIT:
                gridX = BitConverter.ToInt32(data, 0);
                gridY = BitConverter.ToInt32(data, 4);

                //Create undo option
                Unit u = LevelData.gridManager.tiles[gridX, gridY].unit;
                unitS = u.serializeUnit();
                newData = ArrayWorker.SerializableToBytes(unitS);
                createUndoAction(UndoType.REMOVE_UNIT, newData, 0, false);

                //Process redo - destroy unit again
                DestroyUnit(gridX, gridY);
                break;
        }

        redosData.RemoveAt(0);
        redosDataType.RemoveAt(0);
        UpdateUndoRedoUI();
    }

    public interface InactiveStartCaller
    {
        void InactiveStart();
    }

    public enum UndoType
    {
        TERRAIN_EDIT,
        TERRAIN_PAINT,
        PLACE_UNIT,
        REMOVE_UNIT
    }
}
