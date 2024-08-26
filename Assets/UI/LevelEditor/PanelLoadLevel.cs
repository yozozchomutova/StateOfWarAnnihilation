using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class PanelLoadLevel : MonoBehaviour
{
    public Terrain mainTerrain;
    public TerrainEdging terrainEdging;

    public Transform objectTrans;

    public LevelUI levelUIPrefab;
    public Transform scrollViewContentArea;

    public LevelUI[] listedLevels;

    public EditorManager editorManager;
    public PanelLevelInfo panelLevelInfo;
    public PanelNewLevel panelNewLevel;
    public BarMapObjects barMapObjects;
    public BarBuildings barBuildings;
    public InputField levelNameField;

    private void OnEnable()
    {
        //Load level list
        ReloadLevelList();
    }

    public void Load()
    {
        gameObject.SetActive(false);

        //Load level
    }

    public void OnFileExplorerOpen()
    {
        string savesFolder = Application.persistentDataPath + "/SavedLevels/";
        savesFolder = savesFolder.Replace(@"/", @"\");
        System.Diagnostics.Process.Start("explorer.exe", savesFolder);
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
    }

    private int levelUIOffsetY;
    public void ReloadLevelList()
    {
        //Reset
        levelUIOffsetY = -10;
        for (int i = 0; i < listedLevels.Length; i++)
        {
            Destroy(listedLevels[i].gameObject);
        }

        //Add new levels
        string[] levels = Directory.GetFiles(Application.persistentDataPath + "/SavedLevels/", "*.lvl");

        listedLevels = new LevelUI[levels.Length];

        for (int i = 0; i < levels.Length; i++)
        {
            LevelUI levelUI = Instantiate(levelUIPrefab, scrollViewContentArea);
            levelUI.SetValues(this, levels[i]);
            levelUI.gameObject.GetComponent<RectTransform>().localPosition = new Vector2(10, levelUIOffsetY);

            listedLevels[i] = levelUI;

            levelUIOffsetY -= 110;
        }

        //Expand Content transform (to be able to scroll)
        scrollViewContentArea.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, levels.Length * (levelUIPrefab.gameObject.GetComponent<RectTransform>().sizeDelta.y + 10));
    }

    public void LoadLevel(string filePath, LevelUI.Data lui)
    {
        //Create new level
        panelNewLevel.Create();

        FileStream stream = new FileStream(filePath, FileMode.Open);
        BinaryReader br = new BinaryReader(stream);

        stream.Position = lui.mapLevelData_offset;
        byte[] mapLevelBytes = LevelUI.readByteSequence(br, lui.mapLevelData_length);

        if (compareVersion(lui, 02, 12, 01) || compareVersion(lui, 02, 13, 01) || compareVersion(lui, 03, 01, 01) || compareVersion(lui, 03, 02, 01) || compareVersion(lui, 03, 03, 01) || compareVersion(lui, 03, 04, 01))
        {
            byte[] decompressedMapBytes = CompressionManager.Decompress(mapLevelBytes);

            ML_02_12 mapLevel = Deserialize<ML_02_12>(decompressedMapBytes);
            mapLevel.LoadData(mainTerrain, objectTrans, true);
        } else if (compareVersion(lui, 03, 05, 01) || compareVersion(lui, 03, 06, 01))
        {
            byte[] decompressedMapBytes = CompressionManager.Decompress(mapLevelBytes);

            ML_03_05 mapLevel = Deserialize<ML_03_05>(decompressedMapBytes);
            mapLevel.LoadData(mainTerrain, objectTrans, true);
        }
        else
        {
            Debug.Log("Load unsupported!");
        }

        LevelData.ResizeTerrain((int)mainTerrain.terrainData.size.x, terrainEdging, (int)mainTerrain.terrainData.size.x, false);
        editorManager.reloadTeamsConfiguration();

        //Deseralize all teams
        for (int i = 0; i < LevelData.teamStats.Length; i++)
        {
            TeamStats02_12 ts = LevelData.teamStats[i];
            ts.OnDeserialize();
        }

        //Move data from LevelUI to P. Level Info
        panelLevelInfo.description.text = lui.description;
        panelLevelInfo.updateLevelIcon(lui.imgData);

        levelNameField.text = lui.lvlName;

        gameObject.SetActive(false); // Hide panel

        br.Close();
        stream.Close();
    }

    public static T Deserialize<T>(byte[] data)
    {
        using (MemoryStream ms = new MemoryStream(data))
        {
            IFormatter br = new BinaryFormatter();
            return (T)br.Deserialize(ms);
        }
    }

    public static bool compareVersion(LevelUI.Data lui, int bigPatch2, int smallPatch2, int buildCode2)
    {
        return GameManager.compareVersion(lui.big_patch, lui.small_patch, lui.build_code, bigPatch2, smallPatch2, buildCode2);
    }
}
