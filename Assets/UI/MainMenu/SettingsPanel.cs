using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsPanel : MonoBehaviour
{
    [HideInInspector] public DiscordInitializer di;
    [HideInInspector] public DiscordProfile dp;

    //Fonts
    private Font sgrFont;

    [Header("Main information")]
    public GameObject stgFullPanel;
    public Transform panelBcg;

    private List<GameObject> allPanels = new List<GameObject>();

    //UI Generating
    private float uiGeneratingY;

    [Header("Prefabs")]
    public GameObject prefabUIInputField;
    public GameObject prefabUISlider;
    public GameObject prefabUIDropdown;

    private Dictionary<string, UISettingClass> settingsList = new Dictionary<string, UISettingClass>();

    private void Start()
    {
        //Fonts
        sgrFont = Resources.Load<Font>("Fonts/sgr");

        //Fetch GameObject
        //dp = GameObject.Find("DiscordProfile").GetComponent<DiscordProfile>();
        //di = GameObject.Find("DiscordManager").GetComponent<DiscordInitializer>();

        //Game settings
        //airForceUILayout.value = PlayerPrefs.GetInt("stg_airForceUILayout", 0);
        //gameCameraType.value = PlayerPrefs.GetInt("stg_gameCameraType", 0);
        //setNickname.text = PlayerPrefs.GetString("stg_setNickname", "Guest");

        //Audio update
        //masterAudio.value = PlayerPrefs.GetInt("stg_masterAudio", 100);

        //Resolution init
        List<Dropdown.OptionData> resolutions = new List<Dropdown.OptionData>();
        for (int i = Screen.resolutions.Length-1; i >= 0; i--)
        {
            resolutions.Add(new Dropdown.OptionData(Screen.resolutions[i].width + "x" + Screen.resolutions[i].height));
        }
        List<Dropdown.OptionData> qualities = new List<Dropdown.OptionData>();
        for (int i = QualitySettings.names.Length - 1; i >= 0; i--)
        {
            qualities.Add(new Dropdown.OptionData(QualitySettings.names[i]));
        }
        List<Dropdown.OptionData> airforceUIlayout = new List<Dropdown.OptionData>();
        airforceUIlayout.Add(new Dropdown.OptionData("Top-down"));
        airforceUIlayout.Add(new Dropdown.OptionData("Top-left"));
        List<Dropdown.OptionData> cameraTypes = new List<Dropdown.OptionData>();
        cameraTypes.Add(new Dropdown.OptionData("Perspective"));
        cameraTypes.Add(new Dropdown.OptionData("Ortographic"));

        //resolutionD.options = optionList;
        //resolutionD.value = PlayerPrefs.GetInt("stg_resolutionId", optionList.Count-1);

        //discord RPC
        //discordRPC.value = PlayerPrefs.GetInt("stg_discordRpc", 0);

        //Generate UI
        Transform rootGameStgs = AddUI_Root("game settings").transform;
        AddUI_InputField(rootGameStgs, "Nickname", "stg_setNickname");
        AddUI_Dropdown(rootGameStgs, "Camera type", "stg_airForceUILayout", airforceUIlayout);
        AddUI_Dropdown(rootGameStgs, "Camera type", "stg_gameCameraType", cameraTypes);

        Transform rootGraphicsStgs = AddUI_Root("graphics settings").transform;
        AddUI_Dropdown(rootGraphicsStgs, "Resolution", "stg_resolutionId", resolutions);
        AddUI_Dropdown(rootGraphicsStgs, "Quality", "stg_qualityPreset", qualities);

        Transform rootAudioStgs = AddUI_Root("audio settings").transform;
        AddUI_Slider(rootAudioStgs, "Master audio", "stg_masterAudio");

        Transform rootControllsStgs = AddUI_Root("controlls settings").transform;


        //Load default configuration for each one
        foreach (UISettingClass s in settingsList.Values)
        {
            s.loadSetting<System.Object>();
        }

        //Start with default
        switchPanel(0);

        applySettings();
    }

    public void hideGameSettingsPanel()
    {

        //Apply settings
        /*PlayerPrefs.SetInt("stg_airForceUILayout", airForceUILayout.value);
        PlayerPrefs.SetInt("stg_gameCameraType", gameCameraType.value);

        setNickname.text = string.IsNullOrWhiteSpace(setNickname.text) ? "Guest" : setNickname.text;
        PlayerPrefs.SetString("stg_setNickname", setNickname.text);
        PlayerPrefs.Save();*/
    }

    public void hideAudioSettings()
    {
        PlayerPrefs.Save();
    }

    public void hideGraphicSettings()
    {
        PlayerPrefs.Save();
    }

    public void hidePrivacyPanel()
    {

        //Apply settings
        /*PlayerPrefs.SetInt("stg_discordRpc", (int)discordRPC.value);
        PlayerPrefs.Save();

        if ((int)discordRPC.value == 1)
        {
            di.StartDiscord(dp);
        } else
        {
            di.StopDiscord(dp, "Forbiden");
        }*/
    }

    public void switchPanel(int id)
    {
        foreach (GameObject g in allPanels)
        {
            g.SetActive(false);
        }

        allPanels[id].SetActive(true);
    }

    public void applySettings()
    {
        foreach (UISettingClass s in settingsList.Values)
        {
            s.saveSetting();
        }

        print("Volume applied!");
        AudioListener.volume = settingsList["stg_masterAudio"].loadSetting<float>();

        print("QID: " + settingsList["stg_qualityPreset"].loadSetting<int>());
        QualitySettings.SetQualityLevel(settingsList["stg_qualityPreset"].loadSetting<int>());

        int resId = settingsList["stg_resolutionId"].loadSetting<int>();
        int resIdOffset = Screen.resolutions.Length - 1;
        Screen.SetResolution(Screen.resolutions[resIdOffset - resId].width, Screen.resolutions[resIdOffset - resId].height, true);

        //If in game level, reload UI config
        GameLevelUIController levelUIcontroller = FindObjectOfType<GameLevelUIController>();
        if (levelUIcontroller != null)
        {
            levelUIcontroller.reloadUIlayout();
        }
    }

    public void closePanel()
    {
        stgFullPanel.GetComponent<MainMenuPanel>().OnClose();
    }

    #region Add UI functions
    public GameObject AddUI_Root(string title)
    {
        uiGeneratingY = -75;

        GameObject root = new GameObject(title);
        RectTransform rt = root.AddComponent<RectTransform>();
        root.transform.SetParent(panelBcg);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        allPanels.Add(root);
        return root;
    }

    public GameObject AddUI_TextGO(Transform root, string titleName, string text)
    {
        GameObject go = new GameObject(titleName);
        go.transform.parent = root;
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.pivot = new Vector2(0, 1);
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.sizeDelta = new Vector2(800, 30);
        go.gameObject.AddComponent<MeshRenderer>();
        Text titleTxt = go.gameObject.AddComponent<Text>();
        titleTxt.raycastTarget = false;
        titleTxt.text = text;
        titleTxt.color = Color.white;
        titleTxt.fontSize = 20;
        titleTxt.font = sgrFont;

        rt.anchoredPosition = new Vector2(20, uiGeneratingY);
        return go;
    }

    public void AddUI_InputField(Transform root, string title, string key)
    {
        GameObject textGo = AddUI_TextGO(root, title, title);
        RectTransform ifRt = Instantiate(prefabUIInputField, textGo.transform).GetComponent<RectTransform>();
        ifRt.anchoredPosition = new Vector2(0, -30);

        uiGeneratingY -= 80;
        settingsList.Add(key, new UISettingClass(key, ifRt.gameObject, UISettingClass.DataType.INPUT_FIELD));
    }

    public void AddUI_Dropdown(Transform root, string title, string key, List<Dropdown.OptionData> data)
    {
        GameObject textGo = AddUI_TextGO(root, title, title);
        RectTransform dpRt = Instantiate(prefabUIDropdown, textGo.transform).GetComponent<RectTransform>();
        dpRt.anchoredPosition = new Vector2(0, -30);
        dpRt.GetComponent<Dropdown>().options = data;

        uiGeneratingY -= 80;

        settingsList.Add(key, new UISettingClass(key, dpRt.gameObject, UISettingClass.DataType.DROPDOWN));
    }

    public void AddUI_Slider(Transform root, string title, string key)
    {
        GameObject textGo = AddUI_TextGO(root, title, title);
        RectTransform slRt = Instantiate(prefabUISlider, textGo.transform).GetComponent<RectTransform>();
        slRt.anchoredPosition = new Vector2(0, -30);

        uiGeneratingY -= 80;

        settingsList.Add(key, new UISettingClass(key, slRt.gameObject, UISettingClass.DataType.SLIDER));
    }
    #endregion

    public class UISettingClass
    {
        public string key;
        public GameObject ui;
        public DataType dataType;

        public UISettingClass(string key, GameObject ui, DataType dataType)
        {
            this.key = key;
            this.ui = ui;
            this.dataType = dataType;
        }

        public enum DataType
        {
            SLIDER,
            INPUT_FIELD,
            DROPDOWN,
        }

        public void saveSetting()
        {
            switch (dataType)
            {
                case DataType.SLIDER:
                    PlayerPrefs.SetFloat(key, ui.GetComponent<Slider>().value);
                    break;
                case DataType.INPUT_FIELD:
                    PlayerPrefs.SetString(key, ui.GetComponent<InputField>().text);
                    break;
                case DataType.DROPDOWN:
                    PlayerPrefs.SetInt(key, ui.GetComponent<Dropdown>().value);
                    break;
            }
        }

        public T loadSetting<T>()
        {
            switch (dataType)
            {
                case DataType.SLIDER:
                    Slider s = ui.GetComponent<Slider>();
                    s.value = PlayerPrefs.GetFloat(key, s.value);
                    return (T) Convert.ChangeType(s.value , typeof(T));
                case DataType.INPUT_FIELD:
                    InputField i = ui.GetComponent<InputField>();
                    i.text = PlayerPrefs.GetString(key, i.text);
                    return (T)Convert.ChangeType( i.text , typeof(T));
                case DataType.DROPDOWN:
                    Dropdown d = ui.GetComponent<Dropdown>();
                    d.value = PlayerPrefs.GetInt(key, d.value);
                    return (T)Convert.ChangeType(d.value, typeof(T));
                default:
                    throw new Exception("Shouldn't happen that");
            }
        }
    }
}
