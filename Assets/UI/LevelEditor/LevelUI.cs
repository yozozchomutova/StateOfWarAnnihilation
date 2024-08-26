using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    private LoadLevelPanel loadLevelPanel; //FOR MAIN MENU / SINGLEPLAYER
    private PanelLoadLevel panelLoadLevel; //FOR EDITOR

    public Text levelName;
    public Text levelInfo;
    public RawImage icon;

    public Button playBtn;
    public Text playBtnText;

    public Data d;

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void SetValues(LoadLevelPanel loadLevelPanel, string fileLvlPath)
    {
        this.loadLevelPanel = loadLevelPanel;
        Init(fileLvlPath);
    }

    public void SetValues(PanelLoadLevel panelLoadLevel, string fileLvlPath)
    {
        this.panelLoadLevel = panelLoadLevel;
        Init(fileLvlPath);
    }

    public void Init()
    {
        //TEMPORARY FILE PATH
        Init("C:\\Users\\danek\\AppData\\LocalLow\\YZCH\\StateOfWarAnnihilation\\SavedLevels\\LEVEL01.lvl");
    }

    private void Init(string fileLvlPath)
    {
        d = new Data();
        d.lvlName = Path.GetFileNameWithoutExtension(fileLvlPath);
        d.fileLvlPath = fileLvlPath;

        //Load main info
        FileStream stream = new FileStream(fileLvlPath, FileMode.Open);
        BinaryReader br = new BinaryReader(stream);

        d.big_patch = br.ReadByte();
        d.small_patch = br.ReadByte();

        //DEPRECATED / OBSOLETE - Remove in the future
        if (d.big_patch == 1 && (d.small_patch == 1 || d.small_patch == 2))
        {
            LoadVer_01_02___AV(stream, br);
        }
        else
        {
            d.build_code = br.ReadByte();

            if ((d.big_patch == 1) && (d.small_patch == 3 || d.small_patch == 4) && d.build_code == 1)
            {
                LoadVer_01_03___AV(stream, br);
            } else if ((d.big_patch == 2) && (d.small_patch == 1 || d.small_patch == 2 || d.small_patch == 3 || d.small_patch == 4 || d.small_patch == 5 || d.small_patch == 6 || d.small_patch == 7 || d.small_patch == 8 || d.small_patch == 9 || d.small_patch == 10 || d.small_patch == 11 || d.small_patch == 12 || d.small_patch == 13) && d.build_code == 1)
            {
                LoadVer_01_04___AV(stream, br);
            } else if ((d.big_patch == 3) && (d.small_patch == 1 || d.small_patch == 2 || d.small_patch == 3 || d.small_patch == 4 || d.small_patch == 5 || d.small_patch == 6))
            {
                LoadVer_01_04___AV(stream, br);
            }
        }

        stream.Close();
    }

    public void OnInfo()
    {

    }

    public void OnLoad()
    {
        try
        {
            if (panelLoadLevel != null)
                panelLoadLevel.LoadLevel(d.fileLvlPath, d); //FOR EDITOR
            else
                loadLevelPanel.prepareLevel(this); //FOR MAIN MENU / SINGLEPLAYER
        } catch(Exception e)
        {
            Debug.Log("Catched!");
            Debug.LogException(e);
            Resources.FindObjectsOfTypeAll<MessageBox>()[0].show("Error while loading level", e.Message + "\n" + e.StackTrace);
        }
    }

    public void disablePlayBtn()
    {
        playBtn.interactable = false;
        playBtnText.text = "Unsupported";
    }

    public static byte[] readByteSequence(BinaryReader br, int length)
    {
        return br.ReadBytes(length);
    }

    //LOADING FOR DIFFERENT VERSIONS
    public void LoadVer_01_02___AV(FileStream stream, BinaryReader br)
    {
        d.difficulty = (int) br.ReadSingle()*10;
        d.mapWidth = br.ReadInt32();
        d.mapHeight = br.ReadInt32();
        int descLength = br.ReadInt32();
        int imgLength = br.ReadInt32();
        d.mapLevelData_length = br.ReadInt32();

        d.mapLevelData_offset = (int)(stream.Position + descLength + imgLength); //Prepare level data

        //Write information to local memory
        levelName.text = Path.GetFileName(d.fileLvlPath);

        d.description = Encoding.UTF8.GetString(readByteSequence(br, descLength));

        d.imgData = readByteSequence(br, imgLength);

        //Icon
        Texture2D imgTex = new Texture2D(2, 2);
        ImageConversion.LoadImage(imgTex, d.imgData);
        icon.texture = imgTex;

        //Map info:
        levelInfo.text = "Version: " + d.big_patch + "-" + d.small_patch + "\nSize: " + d.mapWidth + "x" + d.mapHeight + "\nDifficulty: " + d.difficulty + "%";
        levelInfo.color = new Color(1f, 0, 0, 1f);
    }

    public void LoadVer_01_03___AV(FileStream stream, BinaryReader br)
    {
        d.difficulty = br.ReadInt32();
        d.mapWidth = br.ReadInt32();
        d.mapHeight = br.ReadInt32();
        int descLength = br.ReadInt32();
        int imgLength = br.ReadInt32();
        d.mapLevelData_length = br.ReadInt32();

        d.mapLevelData_offset = (int)(stream.Position + descLength + imgLength); //Prepare level data

        //Write information to local memory
        levelName.text = Path.GetFileName(d.fileLvlPath);

        d.description = Encoding.UTF8.GetString(readByteSequence(br, descLength));

        d.imgData = readByteSequence(br, imgLength);

        //Icon
        Texture2D imgTex = new Texture2D(2, 2);
        ImageConversion.LoadImage(imgTex, d.imgData);
        icon.texture = imgTex;

        //Map info:
        levelInfo.text = "Version: " + d.big_patch + "-" + d.small_patch + "-" + d.build_code + "\nSize: " + d.mapWidth + "x" + d.mapHeight + "\nDifficulty: " + d.difficulty + "%";
    }

    public void LoadVer_01_04___AV(FileStream stream, BinaryReader br)
    {
        //Small + Big patch bytes offset
        d.difficulty = br.ReadInt32();
        d.mapWidth = br.ReadInt32();
        d.mapHeight = br.ReadInt32();
        int descLength = br.ReadInt32();
        int imgLength = br.ReadInt32();
        d.mapLevelData_length = br.ReadInt32();

        d.mapLevelData_offset = (int)(stream.Position + descLength + imgLength); //Prepare level data

        //Write information to local memory
        if (levelName != null)
            levelName.text = Path.GetFileName(d.fileLvlPath);

        d.description = Encoding.UTF8.GetString(readByteSequence(br, descLength));

        d.imgData = readByteSequence(br, imgLength);

        if (levelInfo != null)
        {
            //Icon
            Texture2D imgTex = new Texture2D(2, 2);
            ImageConversion.LoadImage(imgTex, d.imgData);
            icon.texture = imgTex;

            //Map info:
            levelInfo.text = "Version: " + d.big_patch + "-" + d.small_patch + "-" + d.build_code + "\nSize: " + d.mapWidth + "x" + d.mapHeight + "\nDifficulty: " + d.difficulty + "%";
        }
    }

    public class Data
    {
        public string lvlName;
        public string fileLvlPath;

        //Data
        public int big_patch, small_patch, build_code;
        public int difficulty;
        public int mapWidth, mapHeight;
        public string description;

        //If player decides to load level
        public int mapLevelData_offset, mapLevelData_length;
        public byte[] imgData;
    }
}
