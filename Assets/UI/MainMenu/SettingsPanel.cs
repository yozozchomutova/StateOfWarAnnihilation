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

    [Header("Discord")]
    public Slider discordRPC;

    [Header("Main information")]
    public GameObject stgFullPanel;
    public Transform panelBcg;

    public GameObject[] allPanels;

    [Header("Window: Game")]
    public InputField nicknameIF;
    public Dropdown airForcesPositionD;
    public Dropdown gameCameraTypeD;

    [Header("Window: Editor")]

    [Header("Window: Graphics")]
    public Dropdown resolutionD;
    public Dropdown graphicsD;

    [Header("Window: Audio")]
    public Slider masterAudioS;

    [Header("Window: Services")]
    public Slider discordRpcS;

    private void Start()
    {
        //Fetch GameObject
        //dp = GameObject.Find("DiscordProfile").GetComponent<DiscordProfile>();
        di = GameObject.Find("DiscordManager").GetComponent<DiscordInitializer>();

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

        //=Game
        nicknameIF.text = PlayerPrefs.GetString("stg_setNickname", "Guest");
        airForcesPositionD.value = PlayerPrefs.GetInt("stg_airForceUILayout", 1);
        gameCameraTypeD.value = PlayerPrefs.GetInt("stg_gameCameraType", 1);

        //=Editor

        //=Graphics
        resolutionD.options = resolutions;
        resolutionD.value = PlayerPrefs.GetInt("stg_resolutionId", 0);

        graphicsD.options = qualities;
        graphicsD.value = PlayerPrefs.GetInt("stg_qualityPreset", 0);

        //=Audio
        masterAudioS.value = PlayerPrefs.GetFloat("stg_masterAudio", 1f);

        //=Services
        //discord RPC
        discordRPC.value = PlayerPrefs.GetInt("service_discordRP", 1);

        //Start with default
        switchPanel(0);

        applySettings();
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
        //=Game
        PlayerPrefs.SetString("stg_setNickname", nicknameIF.text);
        PlayerPrefs.SetInt("stg_airForceUILayout", airForcesPositionD.value);
        PlayerPrefs.SetInt("stg_gameCameraType", gameCameraTypeD.value);

        //=Editor

        //=Graphics
        QualitySettings.SetQualityLevel(graphicsD.value);
        PlayerPrefs.SetInt("stg_qualityPreset", graphicsD.value);

        int resId = resolutionD.value;
        int resIdOffset = Screen.resolutions.Length - 1;
        Screen.SetResolution(Screen.resolutions[resIdOffset - resId].width, Screen.resolutions[resIdOffset - resId].height, true);
        PlayerPrefs.SetInt("stg_resolutionId", resolutionD.value);

        //=Audio
        AudioListener.volume = masterAudioS.value;
        PlayerPrefs.SetFloat("stg_masterAudio", masterAudioS.value);

        //=Services
        PlayerPrefs.SetInt("service_discordRP", (int) discordRPC.value);

        //Afterjob
        PlayerPrefs.Save();
        makeChanges();
    }

    private void makeChanges()
    {
        //=Game
        if (LevelData.scene == LevelData.Scene.GAME) //Reload UI config
            FindObjectOfType<GameLevelUIController>().reloadUIlayout();

        //=Services
        DiscordManager.current.UpdateDiscordService();
    }

    public void closePanel()
    {
        stgFullPanel.GetComponent<MainMenuPanel>().OnClose();
    }
}
