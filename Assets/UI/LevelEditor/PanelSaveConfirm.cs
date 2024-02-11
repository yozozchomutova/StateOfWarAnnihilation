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

    public Terrain mainTerrain;
    public Transform water;

    [Header("References")]
    public BarMapObjects barMapObjects;
    public BarBuildings barBuildings;
    public BarNavigations barNavigations;
    public PanelLevelInfo panelLevelInfo;

    public void Yes()
    {
        //Create SavedLevels folder, if not created already
        Directory.CreateDirectory(Application.persistentDataPath + "/SavedLevels/");

        //Setup
        string path = Application.persistentDataPath + "/SavedLevels/" + levelName.text + ".lvl";

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

        ML_03_05 mapLevel = new ML_03_05(mainTerrain, water.position.y, barMapObjects, barBuildings, barNavigations);
        byte[] mapLevelData = CompressionManager.Compress(Serialize(mapLevel));

        bw.Write((int)mapLevelData.Length); //Map level data length

        bw.Write((byte[])Encoding.UTF8.GetBytes(panelLevelInfo.description.text)); //Desc
        bw.Write((byte[])panelLevelInfo.imageBytes); //Img data
        bw.Write(mapLevelData);

        bw.Close();
        stream.Close();

        gameObject.SetActive(false); //Close panel
    }

    public void No()
    {
        gameObject.SetActive(false);
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
}
