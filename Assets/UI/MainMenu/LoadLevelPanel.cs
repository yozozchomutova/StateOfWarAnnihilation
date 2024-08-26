using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadLevelPanel : MonoBehaviour
{
    //Header
    public GameObject preparedLevelUIRoot;
    public RawImage levelIcon;
    public Text levelName, levelDesc;

    public LevelUI levelUIPrefab;
    public Transform scrollViewContentArea;

    public LevelUI[] listedLevels;

    [Header("Team settings strip - prefab")]
    public SingleplayerTeamSettings stsPrefab;
    public RectTransform stsScrollViewContent;

    private SingleplayerTeamSettings[] stsFullList;

    [Header("Level UI elemments (properties)")]
    public RectTransform levelListTrans;

    [Header("Loading screen panel")]
    public LoadingScreenPanel lsPanel;

    //CURRENT LOADING LEVEL PATH (These values are shared with GameLevelLoader.cs)
    public static int selectedTeamID = 1; //1 = blue, used for debugging 
    public static LevelUI.Data onGoingLoadingLvl;
    public static TeamPlaySettings[] teamPlaySettings;

    private void Start()
    {

    }

    private void OnEnable()
    {
        preparedLevelUIRoot.SetActive(false);

        ReloadLevelList();
    }

    public void OnFileExplorerOpen()
    {
        string savesFolder = Application.persistentDataPath + "/SavedLevels/";
        savesFolder = savesFolder.Replace(@"/", @"\");
        System.Diagnostics.Process.Start("explorer.exe", savesFolder);
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
            RectTransform lUI_rect = levelUI.gameObject.GetComponent<RectTransform>();
            lUI_rect.localPosition = new Vector2(10, levelUIOffsetY);
            lUI_rect.sizeDelta = new Vector2(levelListTrans.sizeDelta.x-20, lUI_rect.sizeDelta.y);

            listedLevels[i] = levelUI;

            levelUIOffsetY -= 110;

            //Disable LevelUI if not compatible
            if (!PanelLoadLevel.compareVersion(levelUI.d, 03, 01, 01) && !PanelLoadLevel.compareVersion(levelUI.d, 03, 02, 01) && !PanelLoadLevel.compareVersion(levelUI.d, 03, 03, 01) && !PanelLoadLevel.compareVersion(levelUI.d, 03, 04, 01) && !PanelLoadLevel.compareVersion(levelUI.d, 03, 05, 01) && !PanelLoadLevel.compareVersion(levelUI.d, 03, 06, 01))
            {
                levelUI.disablePlayBtn();
            }
        }

        //Expand Content transform (to be able to scroll)
        scrollViewContentArea.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, levels.Length * (levelUIPrefab.gameObject.GetComponent<RectTransform>().sizeDelta.y + 10));
    }

    public void prepareLevel(LevelUI lui)
    {
        onGoingLoadingLvl = lui.d;

        levelIcon.texture = lui.icon.texture;
        levelName.text = lui.levelName.text;
        levelDesc.text = "Size: " + lui.d.mapWidth + " |Difficulty: " + lui.d.difficulty + "%";

        //Clear STS list
        if (stsFullList != null)
            for (int i = 0; i < stsFullList.Length; i++)
                Destroy(stsFullList[i].gameObject);
        stsFullList = new SingleplayerTeamSettings[GlobalList.teams.Length];

        //Generate new STS list
        float offsetY = 0;
        for (int i = 0; i < GlobalList.teams.Length; i++)
        {
            SingleplayerTeamSettings sts = Instantiate(stsPrefab.gameObject, stsScrollViewContent).GetComponent<SingleplayerTeamSettings>();
            sts.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, offsetY);
            sts.init(i, i == 1 ? 1 : 2);
            stsFullList[i] = sts;
            offsetY -= 80;
        }
        stsScrollViewContent.sizeDelta = new Vector2(stsScrollViewContent.sizeDelta.x, -offsetY);

        preparedLevelUIRoot.SetActive(true);
    }

    public void loadLevel()
    {
        teamPlaySettings = new TeamPlaySettings[GlobalList.teams.Length];
        selectedTeamID = -1; //Spectate mode will happen, if none of team settings has player category
        for (int i = 0; i < teamPlaySettings.Length; i++)
        {
            teamPlaySettings[i] = new TeamPlaySettings(i, stsFullList[i].getSelectedDriverId());
            if (teamPlaySettings[i].teamDriverCategoryId == SingleplayerTeamSettings.DRIVER_CATEGORY_PLAYER)
            {
                selectedTeamID = i;
            }
        }

        lsPanel.EngageLoading(levelName.text, "Custom-made mission", "White Command Centers are obtainable...", startLoading(), 1f, 1f);
    }

    IEnumerator startLoading()
    {
        AsyncOperation lsg = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive); //Loading screen Gate

        while (!lsg.isDone)
        {
            yield return null;
        }

        lsPanel.transform.SetParent(GameObject.Find("LSG_mainCanvas").transform); //Retach from main scene to gate
        AsyncOperation mainMenuUnloader = SceneManager.UnloadSceneAsync(1); //Unload main menu

        while (!mainMenuUnloader.isDone)
        {
            yield return null;
        }

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(4, LoadSceneMode.Additive);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        lsPanel.transform.SetParent(GameObject.Find("PlayerCanvas").transform); //Retach from gate to scene
        lsg = SceneManager.UnloadSceneAsync(2); //Unload loading screen gate

        while (!lsg.isDone)
        {
            yield return null;
        }

        //Finish
        lsPanel.FinishLoading();
    }
}
