using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PanelSaveConfirm : MonoBehaviour
{
    public EditorManager editorManager;

    public InputField levelName;
    public GameObject saveOverwriteDialog;

    public Terrain mainTerrain;

    [Header("Export SOWA editor")]
    public Camera photoshotCam;
    public Text sowcMapSize;
    public Slider sowcMapSlider;
    public InputField sowcMapName;

    [Header("References")]
    public BarMapObjects barMapObjects;
    public BarBuildings barBuildings;
    public BarNavigations barNavigations;
    public PanelLevelInfo panelLevelInfo;

    /// <summary>How is level saved </summary>
    public static string savedLevelName;

    private void OnEnable()
    {
        levelName.text = savedLevelName;
    }

    /// <summary>Check, if level already exists with that name</summary>
    public void Save()
    {
        string newLevelName = levelName.text;
        if (!string.IsNullOrWhiteSpace(newLevelName))
        {
            if (savedLevelName != newLevelName) //Check for overwriting
            {
                if (File.Exists(Application.persistentDataPath + "/SavedLevels/" + newLevelName + ".lvl")) //Ask
                {
                    saveOverwriteDialog.SetActive(true);
                } else //Save
                {
                    Yes();
                }
            } else //Autosave
            {
                Yes();
            }
        }
    }

    /// <summary>If level wasnt never saved before, open panel... Otherwise overwrite level...</summary>
    public void QuickSave()
    {
        if (string.IsNullOrWhiteSpace(savedLevelName))
        {
            gameObject.SetActive(true);
        } else
        {
            Yes();
        }
    }

    public void Yes()
    {
        savedLevelName = levelName.text;

        //Create SavedLevels folder, if not created already
        Directory.CreateDirectory(Application.persistentDataPath + "/SavedLevels/");

        //Setup
        string path = Application.persistentDataPath + "/SavedLevels/" + levelName.text + ".lvl";

        using (BinaryWriter bw = Level.OpenFileWriter(path))
        {
            Level l = new ML_4_1();
            l.WriteHeader(bw);
            l.WriteMapData(bw);
        }

        //Write main informations
        /*
        bw.Write((byte)int.Parse(GameManager.getBigPatchVer())); //Big patch
        bw.Write((byte)int.Parse(GameManager.getSmallPatchVer())); //Small patch
        bw.Write((byte)GameManager.getTagVersionId()); //Build code
        bw.Write(calculateDifficulty()); //Difficulty
        bw.Write((int)PanelNewLevel.normalMapSize); //Map width
        bw.Write((int)PanelNewLevel.normalMapSize); //Map height
        bw.Write((int)panelLevelInfo.description.text.Length); //Desc length
        bw.Write((int)panelLevelInfo.imageBytes.Length); //Img length

        ML_03_05 mapLevel = new ML_03_05(mainTerrain, barMapObjects, barBuildings, barNavigations);
        byte[] mapLevelData = CompressionManager.Compress(Serialize(mapLevel));

        bw.Write((int)mapLevelData.Length); //Map level data length

        bw.Write((byte[])Encoding.UTF8.GetBytes(panelLevelInfo.description.text)); //Desc
        bw.Write((byte[])panelLevelInfo.imageBytes); //Img data
        bw.Write(mapLevelData);

        bw.Close();
        stream.Close();*/

        gameObject.SetActive(false); //Close panel
    }

    public void No()
    {
        gameObject.SetActive(false);
    }

    /// <summary>Calculates difficulty on current level</summary>
    /// <returns>Difficulty between 0-10 float</returns>
    public static float CalculateLevelDifficulty()
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

        return ((float)blueUnitCount / (float) allUnitCount * 10);
    }

    public static int[] GetPlayingTeams()
    {
        List<int> teams = new List<int>();

        for (int i = 0; i < LevelData.units.Count; i++)
        {
            int unitTeamId = LevelData.units[i].team.id;
            if (unitTeamId != Team.WHITE)
                if (!teams.Contains(unitTeamId))
                    teams.Add(unitTeamId);
        }

        return teams.ToArray();
    }

    public void SowCSliderChange()
    {
        sowcMapSize.text = "Map Size (" + (int) sowcMapSlider.value + ")";
    }

    public void ExportSowClassicLevel()
    {
        string folder = Application.persistentDataPath;
        ClassicSOWExporter.exportAll(photoshotCam, folder + "/M" + sowcMapName.text, (int)sowcMapSlider.value);
    }
}
