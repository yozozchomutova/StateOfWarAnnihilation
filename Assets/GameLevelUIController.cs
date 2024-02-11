using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameLevelUIController : MonoBehaviour
{
    public LevelManager levelManager;
    public GameLevelLoader gameLevelLoader;

    public MainMenuPanel gameLevelMenuPanel;

    public Camera playerCamera;

    public bool writingToChat = false;

    //UI layouts
    [HideInInspector] public int SAF_cfgId = -1;
    public UI_GameAirForces[] sendAirForce_cfgs;

    //UI unit selection system
    public RectTransform unitSelectionRoot;
    public float unitSelectionRootY;
    public SelectedGameUnitUI unitSelectionItemPrefab;
    //public List<SelectedGameUnitUI> unitSelectionItems = new List<SelectedGameUnitUI>();
    public Dictionary<string, SelectedGameUnitUI> unitSelectionItems = new Dictionary<string, SelectedGameUnitUI>();

    [Header("Minimap")]
    public RawImage minimapOverlay;
    private Texture2D minimapOTexture;
    private Color[] minimapBcg;

    //UI Chat
    [Header("Chatting")]
    public RectTransform chatRoot;
    public RectTransform chatContent;
    public static RectTransform staticChatContent;
    public TMP_Text chatMsgPrefab;
    public static TMP_Text staticChatMsgPrefab;
    public Text chatLog;
    public static Text staticChatLog; //Initialized in: Start()
    public InputField chatInput;
    public GameObject chatEnterToWriteMsg;
    public AudioSource chatAlertSound;
    public static Vector3 staticMsgPosition;

    [Header("Debugging")]
    public TMP_Text fpsCounter;
    public float fpsUpdateTime;
    private float curFpsUpdateTime;
    private int fpsSample = 0;
    private int[] fpsSamples = new int[10];

    [Header("Level objectives")]
    public MainMenuPanel levelObjectivesRoot;

    [Header("Player list tab")]
    public MainMenuPanel playerListTab;

    public static bool lockCameraMovement = false;

    void Start()
    {
        reloadUIlayout();
    }

    public void reloadUIlayout()
    {
        //Make chat log static
        staticChatContent = chatContent;
        staticChatLog = chatLog;
        staticChatMsgPrefab = chatMsgPrefab;

        //Send air force UI layout configuration
        SAF_cfgId = PlayerPrefs.GetInt("stg_airForceUILayout", 0);

        hideAll_sendAirForce_cfg();
        if (SAF_cfgId == 0)
        {
            sendAirForce_cfgs[0].gameObject.SetActive(true);
            chatRoot.anchoredPosition = new Vector2(0, 100);
        }
        else if (SAF_cfgId == 1)
        {
            sendAirForce_cfgs[1].gameObject.SetActive(true);
            chatRoot.anchoredPosition = new Vector2(0, 0);
        }
        configure_sendAirForce_cfg();

        //Decide if use Perspective or Ortographic camera type
        int cameraType = PlayerPrefs.GetInt("stg_gameCameraType", 0);
        if (cameraType == 0)
        {
            playerCamera.orthographic = false;
        }
        else if (cameraType == 1)
        {
            playerCamera.orthographic = true;
        }

        //Minimap
        minimapOTexture = new Texture2D(221, 221, TextureFormat.RGBA4444, false);
        minimapBcg = new Color[221 * 221];
        for (int i = 0; i < minimapBcg.Length; i++)
            minimapBcg[i] = Color.clear;
        InvokeRepeating("UpdateMinimapOverlay", 0f, 1f);
    }

    void Update()
    {
        curFpsUpdateTime -= Time.deltaTime;
        if (curFpsUpdateTime <= 0)
        {
            fpsSamples[fpsSample] = (int)(1f / Time.unscaledDeltaTime);
            fpsSample++;

            if (fpsSample >= 10)
            {
                fpsSample = 0;
                float average = 0;
                for (int i = 0; i < fpsSamples.Length; i++)
                    average += fpsSamples[i];
                average /= (float)fpsSamples.Length;

                fpsCounter.text = "FPS: " + (int)(average);
            }

            curFpsUpdateTime = fpsUpdateTime;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (writingToChat) { //Exit chat
                closeChatWriteMode();
            } else if (levelManager.buildMode) { //Exit build mode
                levelManager.cancelBuild();
            } else if (levelManager.sendAirforceMode) { //Exit air force mode
                levelManager.cancelAirForce();
            } else {
                gameLevelMenuPanel.gameObject.SetActive(true);
                gameLevelMenuPanel.alphaRising = !gameLevelMenuPanel.alphaRising;
            }
        } else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (writingToChat) //Send message
            {
                sendChatEnteredMsg();
                closeChatWriteMode();
            } else
            {
                openChatWriteMode();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            showLevelObjectives();
        }

        //Turret building keys
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.F))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) levelManager.prepareAirForce(GlobalList.AirForce.JET);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) levelManager.prepareAirForce(GlobalList.AirForce.DESTROYER);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) levelManager.prepareAirForce(GlobalList.AirForce.CYCLONE);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) levelManager.prepareAirForce(GlobalList.AirForce.CARRYBUS);
            else if (Input.GetKeyDown(KeyCode.Alpha5)) levelManager.prepareAirForce(GlobalList.AirForce.DEBRIS);
        } else {
            if (Input.GetKeyDown(KeyCode.Alpha1)) build_ant();
            else if (Input.GetKeyDown(KeyCode.Alpha2)) build_antiAircraft();
            else if (Input.GetKeyDown(KeyCode.Alpha3)) build_plasmatic();
            else if (Input.GetKeyDown(KeyCode.Alpha4)) build_machineGun();
            else if (Input.GetKeyDown(KeyCode.Alpha5)) build_granader();
        }

        //Handle selected units UI
        //Blink/Reset tiles
        for (int i = unitSelectionItems.Count-1; i >= 0; i--)
        {
            SelectedGameUnitUI s = unitSelectionItems.Values.ElementAt(i);

            if (!s.isStillUsable())
            {
                Destroy(unitSelectionItems.Values.ElementAt(i).gameObject);
                unitSelectionItems.Remove(unitSelectionItems.Keys.ElementAt(i));
                continue;
            } else
            {
                s.updateX(10 + 120 * i);
                s.pushStats();
            }

            s.clear();
        }

        //Create new tiles
        if (levelManager.selectedUnits.Count > 0) {
            unitSelectionRootY += (0 - unitSelectionRootY) * Time.deltaTime * 8f; //Show
            for (int i = 0; i < levelManager.selectedUnits.Count; i++)
            {
                Unit u = levelManager.selectedUnits[i];

                if (!unitSelectionItems.ContainsKey(u.id))
                {
                    SelectedGameUnitUI s = Instantiate(unitSelectionItemPrefab, unitSelectionRoot);
                    s.initStats(0, levelManager.selectedUnits[i].team, levelManager.selectedUnits[i].icon);
                    s.addUnit(u.getHpNormalized());
                    unitSelectionItems.Add(u.id, s);
                } else
                {
                    SelectedGameUnitUI s = unitSelectionItems[u.id];
                    s.addUnit(u.getHpNormalized());
                }
            }
        } else {
            unitSelectionRootY += (-159 - unitSelectionRootY) * Time.deltaTime * 8f; //Hide
        }

        unitSelectionRoot.anchoredPosition = new Vector2(0, unitSelectionRootY);

        //Handle click from Minimap - Moving on map
        if (Input.GetMouseButton(0))
        {
            //Is mouse over Rect?
            Vector2 mousePos = Input.mousePosition;
            RectTransform rectTransform = minimapOverlay.rectTransform;
            bool isMouseOverRawImage = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos, null);

            if (isMouseOverRawImage)
            {
                //Get position
                Vector2 localMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, null, out localMousePos);
                localMousePos += new Vector2(rectTransform.rect.width/2f, rectTransform.rect.height/2f);

                float normalizedX = Mathf.InverseLerp(0, rectTransform.rect.width, localMousePos.x);
                float normalizedY = Mathf.InverseLerp(0, rectTransform.rect.height, localMousePos.y);

                //Apply to camera
                Vector3 tSize = LevelData.mainTerrain.terrainData.size;
                Vector3 camPivot = playerCamera.transform.parent.position;

                camPivot = new Vector3(normalizedX * tSize.x, camPivot.y, normalizedY * tSize.z);

                playerCamera.transform.parent.position = camPivot;
            }
        }
    }

    public void UpdateMinimapOverlay()
    {
        Vector3 tSize = LevelData.mainTerrain.terrainData.size;
        int mSize = 221;

        minimapOTexture.SetPixels(minimapBcg);

        //Units
        for (int i = 0; i < LevelData.units.Count; i++)
        {
            Unit u = LevelData.units[i];

            Vector3 p = u.gameObject.transform.position;
            Vector2 pixelPositions = new Vector2(p.x / tSize.x * mSize, p.z / tSize.z * mSize);

            if (pixelPositions.x > 0 && pixelPositions.x < 217 && pixelPositions.y > 0 && pixelPositions.y < 217)
                minimapOTexture.SetPixels((int) pixelPositions.x, (int) pixelPositions.y, 4, 4, u.team.minimapColorSequence);
        }

        //Camera
        //Vector2 pixelPositions = new Vector2(p.x / tSize.x * mSize, p.z / tSize.z * mSize);

        minimapOTexture.Apply();
        minimapOverlay.texture = minimapOTexture;
    }

    public void build_ant()
    {
        levelManager.deselectUnits();
        if (LevelData.ts.ant)
            levelManager.enterBuildMode("0_antT1");
    }

    public void build_antiAircraft()
    {
        levelManager.deselectUnits();
        if (LevelData.ts.antiAircraft)
            levelManager.enterBuildMode("0_antiAircraftT1");
    }

    public void build_plasmatic()
    {
        levelManager.deselectUnits();
        if (LevelData.ts.plasmatic)
            levelManager.enterBuildMode("0_plasmaticT1");
    }

    public void build_machineGun()
    {
        levelManager.deselectUnits();
        if (LevelData.ts.machineGun)
            levelManager.enterBuildMode("0_machineGunT1");
    }

    public void build_granader()
    {
        levelManager.deselectUnits();
        if (LevelData.ts.granader)
            levelManager.enterBuildMode("0_granaderT1");
    }

    private void hideAll_sendAirForce_cfg()
    {
        foreach (UI_GameAirForces ui_gaf in sendAirForce_cfgs)
            ui_gaf.gameObject.SetActive(false);
    }

    private void configure_sendAirForce_cfg()
    {
        foreach (UI_GameAirForces ui_gaf in sendAirForce_cfgs)
        {
            if (ui_gaf.gameObject.activeSelf)
                ui_gaf.SetupUI();
        }
    }

    public void color_sendAirForce_cfg(bool disabled)
    {
        sendAirForce_cfgs[SAF_cfgId].ShowColor(disabled);
    }

    private void openChatWriteMode()
    {
        writingToChat = true;
        lockCameraMovement = true;
        chatEnterToWriteMsg.SetActive(false);
        chatInput.gameObject.SetActive(true);
        chatInput.Select();
        chatInput.ActivateInputField();
    }

    private void sendChatEnteredMsg()
    {
        if (!string.IsNullOrWhiteSpace(chatInput.text)) {
            sendChatMsg(chatInput.text);
        }

        chatInput.text = "";
    }

    private void sendChatMsg(string msg)
    {
        //Is it command?
        if (msg.StartsWith("/")) {
            try {
                //TODO CHEATS - DISABLE IN FUTURE
                if (msg.StartsWith("/m"))
                {
                    LevelData.ts.money += int.Parse(msg.Substring(3));
                }
                else if (msg.StartsWith("/r"))
                {
                    LevelData.ts.research += int.Parse(msg.Substring(3));
                }
                else if (msg.StartsWith("/cb"))
                {
                    if (msg.Length < 4)
                    {
                        AirForce.TEMPORARY_CARRYBUS_UNIT = "";
                        logMessage("CB: Unit cleared!", false, Vector3.zero);
                    } else if (GlobalList.units.ContainsKey(msg.Substring(4)))
                    {
                        AirForce.TEMPORARY_CARRYBUS_UNIT = msg.Substring(4);
                        logMessage("CB: Set unit: " + AirForce.TEMPORARY_CARRYBUS_UNIT, false, Vector3.zero);
                    } else
                    {
                        logMessage("CB: UNIT NOT FOUND: " + msg.Substring(4), false, Vector3.zero);
                    }
                }
                else if (msg.StartsWith("/jet"))
                {
                    LevelData.ts.jets += int.Parse(msg.Substring(5));
                }
                else if (msg.StartsWith("/destroyer"))
                {
                    LevelData.ts.destroyers += int.Parse(msg.Substring(11));
                }
                else if (msg.StartsWith("/cyclone"))
                {
                    LevelData.ts.cyclones += int.Parse(msg.Substring(9));
                }
                else if (msg.StartsWith("/carrybus"))
                {
                    LevelData.ts.carrybuses += int.Parse(msg.Substring(10));
                }
                else if (msg.StartsWith("/debris"))
                {
                    LevelData.ts.debrises += int.Parse(msg.Substring(8));
                }
                else if (msg.StartsWith("/win"))
                {
                    levelManager.endGame(int.Parse(msg.Substring(5)), "Cheated!");
                }
                else if (msg.StartsWith("/overtake"))
                {
                    int teamFromId = int.Parse(msg.Substring(10,1));
                    int teamToId = int.Parse(msg.Substring(12));
                    levelManager.teamOvertakeUnits(teamFromId, teamToId);
                }
                else if (msg.StartsWith("/time"))
                {
                    int time = int.Parse(msg.Substring(6));
                    time %= 1440;
                    LevelData.environment.eventCheckCoooldown = 0;
                    LevelData.environment.time = time;
                    logMessage("Time set!", false, Vector3.zero);
                }
                else if (msg.StartsWith("/fpsfix"))
                {
                    int toggle = int.Parse(msg.Substring(8));
                    if (toggle == 0)
                    {
                        gameLevelLoader.gameObject.SetActive(false);
                        logMessage("Unit hard-restart OFF", false, Vector3.zero);
                    } else if (toggle == 1)
                    {
                        gameLevelLoader.gameObject.SetActive(true);
                        logMessage("Unit hard-restart ON", false, Vector3.zero);
                    } else
                    {
                        logMessage("Unknown signal", false, Vector3.zero);
                    }
                }
                else if (msg.StartsWith("/nnm"))
                {
                    FindObjectOfType<NNManagerPanel>().gameObject.SetActive(true);
                    logMessage("Neural network manager", false, Vector3.zero);
                }
            } catch (Exception e) {
                logMessage("Error: Failed to parse command...", false, Vector3.zero);
                Debug.LogError(e);
            }
        }

        //Log
        //logMessage("<Guest>: " + msg, );
    }

    public static void logMessage(string message, bool playSound, Vector3 positionEvent)
    {
        staticChatLog.text += "\n" + message;

        RectTransform newMSg = Instantiate(staticChatMsgPrefab, staticChatContent).GetComponent<RectTransform>();
        newMSg.gameObject.GetComponent<TMP_Text>().text = message;
        Destroy(newMSg.gameObject, 3.5f);

        if (playSound)
        {
            LevelManager.levelManager.gameUIController.chatAlertSound.Play();
        }

        staticMsgPosition = positionEvent;
    }

    private void closeChatWriteMode()
    {
        writingToChat = false;
        lockCameraMovement = false;
        chatEnterToWriteMsg.SetActive(true);
        chatInput.gameObject.SetActive(false);
    }

    public void showLevelObjectives()
    {
        levelObjectivesRoot.show();
    }

    public void showPlayerList()
    {
        playerListTab.show();
    }
}
