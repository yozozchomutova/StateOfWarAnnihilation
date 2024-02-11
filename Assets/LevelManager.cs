using SOWUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static MapLevelManager;
using static MapLevelManager.UnitSerializable;

public class LevelManager : MonoBehaviour
{
    //Self-instance
    public static LevelManager levelManager;

    public bool gameIsRunning = true;

    //Mouse controller
    public GameLevelUIController gameUIController;
    public MouseControlling mouseController;
    public MouseControlling.SelectedActionType selectedActionType = MouseControlling.SelectedActionType.NONE;

    //Building
    public bool buildMode = false;

    public GameObject BM_root;
    public Text BM_unitName;
    public Text BM_unitCost;
    public Text BM_hint;

    //UI
    public GameLevelUIController gui;
    public Text moneyText;
    public Text researchText;
    public Text researchableCountText;
    public Text energyText;

    //Game stats
    public GameStatsManager gameStatsManager;

    //Time warps
    public RawImage ri_timeWarp_0_0;
    public RawImage ri_timeWarp_1_0;
    public RawImage ri_timeWarp_2_0;
    public RawImage ri_timeWarp_4_0;

    //Colors
    public Color COLOR_WHITE = new Color(1f, 1f, 1f, 1f);
    public Color COLOR_GREEN = new Color(0f, 1f, 0f, 1f);

    //Selected Unit
    [Header("Unit selection")]
    public UnitHPBar unitHPBarPrefab;
    [HideInInspector] public List<UnitHPBar> unitHPBars = new List<UnitHPBar>();

    [HideInInspector] public List<Unit> selectedUnits = new List<Unit>();
    //[HideInInspector] public UnitLE selectedUnit;
    [HideInInspector] private UnitBody selectedBody;

    //Hovered unit & unit properties
    public GameObject ui_unitTab;
    public Text ui_unitText;
    public RawImage ui_unitIcon;
    public Slider ui_unitHealth;
    public Text ui_unitHealthText;
    public Slider ui_unitUpgrade;
    public Image ui_unitTeamFrame;
    public Image ui_unitTeamLine;
    public Text ui_unitTeamText;

    [HideInInspector] private GameObject currentAdditionalProperties;
    [HideInInspector] public Unit hoveredBuilding;
    [HideInInspector] private bool currentAdditionalPropertiesInitalized = true;
    [HideInInspector] private bool currentAdditionalPropertiesInitalized_HoveredUnit = true;

    //Building unit
    public Transform objectTrans;
    private Transform selectedRoot;
    private Vector3 rootOffset;

    //Sending air forces
    public bool sendAirforceMode;
    [HideInInspector] public float airForceAdjustment;

    public float airForceSendCooldown;
    private float curAirForceSendCooldown;
    private bool airForceSendReady;

    private AirForce holdingAF;

    public GameObject AFM_root;
    public Text AFM_forceName;
    public RawImage AFM_icon;
    public TerrainImage AFM_ti;
    public TerrainImage[] AFM_corners;
    //Effects
    [Header("Effects")]
    public MeshRenderer eff_pointerPrefab;

    private void Awake()
    {
        levelManager = this;
        curAirForceSendCooldown = airForceSendCooldown;
        timeWarp_0(); //Stop game for loading
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        raycastHit(ray);

        if (Time.timeScale != 0f)
        {            
            //Update teams energy
            for (int i = 0; i < LevelData.teamStats.Length; i++)
            {
                LevelData.teamStats[i].lastEnergy = LevelData.teamStats[i].newEnergy;
                LevelData.teamStats[i].newEnergy = 1;
            }

            //FrameUpdate()
            for (int i = LevelData.units.Count - 1; i >= 0; i--)
            {
                if (LevelData.units[i] == null)
                {
                    LevelData.units.RemoveAt(i);
                }
                else if (LevelData.units[i].virtualSpace == Unit.VirtualSpace.NORMAL)
                {
                    //print("T: " + GO.GetGameObjectPath(LevelData.units[i].gameObject));
                    LevelData.units[i].frameUpdate();
                }
            }

            //Check for command centers - Ignore team 0 (Neutral)
            if (gameIsRunning)
            {
                //Handle all node update function
                for (int i = 0; i < LevelData.teamStats.Length; i++)
                {
                    for (int j = 0; j < GameLevelObjectives.allObjectives[i].Count; j++)
                    {
                        //print(i + ". IsActive: " + GameLevelObjectives.allObjectives[i][j].isActive + " |H: " + GameLevelObjectives.allObjectives[i][j].ser.nodeId);
                        if (GameLevelObjectives.allObjectives[i][j].isActive)
                            GameLevelObjectives.allObjectives[i][j].HandleUpdate();
                    }
                }

                for (int i = 1; i < LevelData.teamStats.Length; i++)
                {
                    LevelData.teamStats[i].commandCenterPenalty++;
                }
            }

            //Update research availability count
            int newResearchAvailableCount = (int)(LevelData.ts.research / 500f);
            if (LevelData.ts.researchAvailableCount != newResearchAvailableCount)
            {
                if (LevelData.ts.researchAvailableCount < newResearchAvailableCount)
                    GameLevelUIController.logMessage("New technology available", true, Vector3.zero);

                LevelData.ts.researchAvailableCount = newResearchAvailableCount;
            } else
            {
                researchableCountText.text = "" + (LevelData.ts.researchAvailableCount != 0 ? LevelData.ts.researchAvailableCount : "");
            }

            //Update UI frequently
            string additiveZeros = "";
            moneyText.text = additiveZeros + LevelData.ts.money;
            researchText.text = additiveZeros + LevelData.ts.research;
            energyText.text = additiveZeros + Mathf.RoundToInt((LevelData.ts.lastEnergy-1)*100);

            if (gui.SAF_cfgId != -1)
            {
                gui.sendAirForce_cfgs[gui.SAF_cfgId].UpdateCounst(new int[]{ LevelData.ts.jets, LevelData.ts.destroyers, LevelData.ts.cyclones, LevelData.ts.carrybuses, LevelData.ts.debrises });
            }
        }

        //Air force cooldown
        if (!airForceSendReady)
        {
            curAirForceSendCooldown -= Time.deltaTime * Time.timeScale;
            if (curAirForceSendCooldown <= 0)
            {
                airForceSendReady = true;
                gameUIController.color_sendAirForce_cfg(false);
            }
        }

        //Air force positioning and launching
        if (sendAirforceMode)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 300, 1 << 13)) //Hit terrain ?
            {
                AFM_ti.transform.position = hit.point;

                foreach (TerrainImage ti in AFM_corners)
                {
                    ti.transform.localPosition = new Vector3(airForceAdjustment * ti.positionMultiplier.x / holdingAF.terrainImageScale_primary, 0, airForceAdjustment * ti.positionMultiplier.y / holdingAF.terrainImageScale_primary);
                }
                
                if (Input.GetMouseButton(0)) //Adjust?
                {
                    airForceAdjustment = Mathf.Clamp(airForceAdjustment + mouseController.lastDrag.y / 15f, holdingAF.MIN_adjustment, holdingAF.MAX_adjustment);
                } else if (Input.GetMouseButtonDown(1)) //Send?
                {
                    sendAirForce(holdingAF.id, GlobalList.teams[LevelData.ts.teamId], hit.point);

                    //Remove 1 air force from inventory
                    if (holdingAF.id == "0_jet1")
                        LevelData.ts.jets--;
                    else if (holdingAF.id == "0_destroyer1")
                        LevelData.ts.destroyers--;
                    else if (holdingAF.id == "0_cyclone1")
                        LevelData.ts.cyclones--;
                    else if (holdingAF.id == "0_carrybus1")
                        LevelData.ts.carrybuses--;
                    else if (holdingAF.id == "0_debris1")
                        LevelData.ts.debrises--;

                    //Reset cooldown
                    curAirForceSendCooldown = airForceSendCooldown;
                    gameUIController.color_sendAirForce_cfg(true);
                    airForceSendReady = false;

                    cancelAirForce();
                }
            }

            mouseController.cursorType = MouseControlling.CursorType.SENDAIRFORCES;
        }

        //Unit selected?
        if (selectedUnits.Count > 0)
        {
            if (hoveredBuilding != null)
            {
                if (hoveredBuilding.team.id != LevelData.ts.teamId || Input.GetKey(KeyCode.LeftControl))
                {
                    selectedActionType = MouseControlling.SelectedActionType.ATTACK;
                    mouseController.cursorType = MouseControlling.CursorType.ATTACK;
                } else
                {
                    if (selectedUnits[0] is SMF)
                    {
                        if (Input.GetMouseButtonDown(2))
                        {
                            if (selectedActionType != MouseControlling.SelectedActionType.UPGRADE && hoveredBuilding.canBeUpgraded(selectedUnits[0])) {
                                selectedActionType = MouseControlling.SelectedActionType.UPGRADE;
                            } else if (hoveredBuilding.canBeRepaired(selectedUnits[0])) {
                                selectedActionType = MouseControlling.SelectedActionType.REPAIR;
                            }
                        }

                        //Set default, if none of them was selected before
                        if (selectedActionType != MouseControlling.SelectedActionType.UPGRADE && selectedActionType != MouseControlling.SelectedActionType.REPAIR)
                        {
                            if (selectedActionType != MouseControlling.SelectedActionType.UPGRADE && hoveredBuilding.canBeUpgraded(selectedUnits[0]))
                            {
                                selectedActionType = MouseControlling.SelectedActionType.UPGRADE;
                            }
                            else if (selectedActionType != MouseControlling.SelectedActionType.REPAIR && hoveredBuilding.canBeUpgraded(selectedUnits[0]))
                            {
                                selectedActionType = MouseControlling.SelectedActionType.REPAIR;
                            }
                        }

                        //Clear cursor if something is 
                        if ((selectedActionType == MouseControlling.SelectedActionType.UPGRADE && !hoveredBuilding.canBeUpgraded(selectedUnits[0]))
                            || (selectedActionType == MouseControlling.SelectedActionType.REPAIR && !hoveredBuilding.canBeUpgraded(selectedUnits[0])) ){
                            selectedActionType = MouseControlling.SelectedActionType.NONE;
                        }

                        //Decide cursor 
                        if (selectedActionType == MouseControlling.SelectedActionType.UPGRADE)
                            mouseController.cursorType = MouseControlling.CursorType.UPGRADE;
                        else if (selectedActionType == MouseControlling.SelectedActionType.REPAIR)
                            mouseController.cursorType = MouseControlling.CursorType.REPAIR;
                    }
                }
            }
        }

        //Move unit?
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0 && selectedUnits[0].team.id == LevelData.ts.teamId)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 300, 1 << 13 | 1 << 14)) //Hit terrain or unit?
            {
                foreach (Unit u in selectedUnits)
                {
                    if (u.unitType == Unit.UnitType.UNIT || u.unitType == Unit.UnitType.STEPPER)
                    {
                        GroundUnit gu = u as GroundUnit;

                        if (hit.collider.gameObject.layer == 14) //hit unit?
                            gu.commandAttackUnit(Unit.getUnit(hit.collider));
                        else
                            gu.commandMove(hit.point.x, hit.point.z);

                    } else if (u.unitType == Unit.UnitType.SMF)
                    {
                        SMF s = u as SMF;

                        if (hit.collider.gameObject.layer == 14) //hit unit?
                        {
                            //Can't be self!
                            Unit hitUnit = Unit.getUnit(hit.collider);
                            if (selectedUnits[0] != hitUnit)
                            {
                                //TODO Forced attack
                                if (selectedActionType == MouseControlling.SelectedActionType.ATTACK)
                                    s.commandDestroyUnit(hitUnit);
                                else if (selectedActionType == MouseControlling.SelectedActionType.REPAIR)
                                    s.commandRepairUnit(hitUnit);
                                else if (selectedActionType == MouseControlling.SelectedActionType.UPGRADE)
                                    s.commandUpgradeUnit(hitUnit);
                            }
                        }
                        else
                            s.commandMove(hit.point.x, hit.point.z);
                    }
                }

                //Effect
                GameObject effectGO = Instantiate(eff_pointerPrefab.gameObject, hit.point, Quaternion.Euler(-89.98f, 0, 0));
                Destroy(effectGO, 0.5f);
            }
        }
    }

    private void LateUpdate()
    {
        //Preventing random error, that automatically unchecks toggle (and other weird stuff) after instantiate for no reason :/
        if (!currentAdditionalPropertiesInitalized && selectedUnits.Count > 0 && (currentAdditionalProperties != null || selectedUnits[0].propertiesUI_editor == null))
        {
            selectedUnits[0].getGameValStart(ui_unitTeamFrame, ui_unitTeamLine, ui_unitTeamText, currentAdditionalProperties);
            currentAdditionalPropertiesInitalized = true;
        }
        if (!currentAdditionalPropertiesInitalized_HoveredUnit && hoveredBuilding != null && (currentAdditionalProperties != null || hoveredBuilding.propertiesUI_editor == null))
        {
            hoveredBuilding.getGameValStart(ui_unitTeamFrame, ui_unitTeamLine, ui_unitTeamText, currentAdditionalProperties);
            currentAdditionalPropertiesInitalized_HoveredUnit = true;
        }
    }

    //Building
    public void enterBuildMode(string selectedUnitID)
    {
        cancelAirForce();
        deselectUnits();

        buildMode = true;

        //Reset
        if (selectedUnits.Count > 0)
            Destroy(selectedUnits[0].gameObject);

        if (selectedBody != null)
        {
            Destroy(selectedBody.gameObject);
            selectedBody = null;
        }

        //New
        Unit unitToInstantiate = GlobalList.units[selectedUnitID].GetComponent<Unit>();
        Unit ghostUnit = MapLevel.spawnUnit(unitToInstantiate.serializeUnit(), Unit.VirtualSpace.GHOST) ;

        //Select unit
        selectedUnits.Add(ghostUnit);

        ghostUnit.setMeshRendererMat(GlobalList.matHologramGreen);

        selectedRoot = ghostUnit.getRoot();

        //UI
        BM_root.SetActive(true);
        BM_unitName.text = selectedUnits[0].name;
        BM_unitCost.text = selectedUnits[0].value + "-";

        //New
        /*Unit unitToInstantiate = GlobalList.units[selectedUnitID].GetComponent<Unit>();

        if (!string.IsNullOrEmpty(unitToInstantiate.bodyId))
        { //Check if body is null
            selectedBody = Instantiate(GlobalList.bodies[unitToInstantiate.bodyId].gameObject).GetComponent<UnitBody>();
            selectedBody.Init();
            rootOffset = selectedBody.defaultOffset;
            selectedBody.setMeshShadowColorGreen();
        }
        else
        {
            rootOffset = Vector3.zero;
        }

        if (selectedBody == null)
        {
            selectedUnits.Add(Instantiate(unitToInstantiate.gameObject).GetComponent<Unit>());
            selectedRoot = selectedUnits[0].transform;
        }
        else if (selectedBody.headPoint != null)
        {
            selectedUnits.Add(Instantiate(unitToInstantiate.gameObject, selectedBody.headPoint).GetComponent<UnitLE>());
            selectedRoot = selectedBody.transform;
            selectedBody.linkUnit(selectedUnits[0]);
        }
        else
        {
            selectedUnits.Add(Instantiate(unitToInstantiate.gameObject, selectedBody.gameObject.transform).GetComponent<UnitLE>());
            selectedRoot = selectedBody.transform;
            selectedBody.linkUnit(selectedUnits[0]);
        }

        //UI
        BM_root.SetActive(true);
        BM_unitName.text = selectedUnits[0].name;
        BM_unitCost.text = selectedUnits[0].value + "-";

        selectedUnits[0].maximumUnitNeighbourRange = 3.5f;
        selectedUnits[0].moneyDependant = true;
        selectedUnits[0].Init();
        selectedUnits[0].holographic = true;
        selectedUnits[0].setMeshShadowColorGreen();*/
    }

    //Send air force
    public void prepareAirForce(int buttonId)
    {
        if (buttonId == 0) prepareAirForce(GlobalList.AirForce.JET);
        else if (buttonId == 1) prepareAirForce(GlobalList.AirForce.DESTROYER);
        else if (buttonId == 2) prepareAirForce(GlobalList.AirForce.CYCLONE);
        else if (buttonId == 3) prepareAirForce(GlobalList.AirForce.CARRYBUS);
        else if (buttonId == 4) prepareAirForce(GlobalList.AirForce.DEBRIS);
    }
    public void prepareAirForce(GlobalList.AirForce airForce)
    {
        if (!airForceSendReady) //Don't continue, if not ready
            return;

        cancelBuild();
        deselectUnits();

        switch (airForce)
        {
            case GlobalList.AirForce.JET:
                holdingAF = GlobalList.units["0_jet1"] as AirForce;
                if (LevelData.ts.jets <= 0) return;
                break;
            case GlobalList.AirForce.DESTROYER:
                holdingAF = GlobalList.units["0_destroyer1"] as AirForce;
                if (LevelData.ts.destroyers <= 0) return;
                break;
            case GlobalList.AirForce.CYCLONE:
                holdingAF = GlobalList.units["0_cyclone1"] as AirForce;
                if (LevelData.ts.cyclones <= 0) return;
                break;
            case GlobalList.AirForce.CARRYBUS:
                holdingAF = GlobalList.units["0_carrybus1"] as AirForce;
                if (LevelData.ts.carrybuses <= 0) return;
                break;
            case GlobalList.AirForce.DEBRIS:
                holdingAF = GlobalList.units["0_debris1"] as AirForce;
                if (LevelData.ts.debrises <= 0) return;
                break;
        }

        sendAirforceMode = true;

        AFM_ti.transform.localScale = new Vector3(holdingAF.terrainImageScale_primary, 1, holdingAF.terrainImageScale_primary);
        AFM_ti.meshR.material = holdingAF.terrainImageMat_primary;

        AFM_root.SetActive(true);
        AFM_icon.texture = holdingAF.icon;
        AFM_forceName.text = holdingAF.name;
        AFM_ti.gameObject.SetActive(true);

        airForceAdjustment = Mathf.Clamp(airForceAdjustment, holdingAF.MIN_adjustment, holdingAF.MAX_adjustment);

        foreach (TerrainImage ti in AFM_corners)
        {
            ti.transform.localScale = new Vector3(holdingAF.adjustmentCornerScale, 1, holdingAF.adjustmentCornerScale);
            ti.meshR.material = holdingAF.adjustmentCornersMat;
            ti.transform.localPosition = new Vector3(airForceAdjustment * ti.positionMultiplier.x, 0, airForceAdjustment * ti.positionMultiplier.y);
            ti.gameObject.SetActive(true);
        }
    }

    public void sendAirForce(string airForceID, Team team, Vector3 p)
    {
        AirForce referenceAF = GlobalList.units[airForceID] as AirForce;
        UnitSerializable afData = referenceAF.serializeUnit();
        afData.setF(KEY_UNIT_HP, 1f);
        afData.setI(KEY_UNIT_TEAMID, team.id);
        AirForce af = MapLevel.spawnUnit(afData) as AirForce;
        af.transform.position = new Vector3(p.x, p.y + referenceAF.heightOffset_end, -40);
        //AirForceLE af = Instantiate(holdingAF.gameObject, new Vector3(p.x, p.y + holdingAF.heightOffset_end, -55), Quaternion.identity).GetComponent<AirForceLE>();
        af.commandLaunch(p.x, LevelData.mainTerrain.terrainData.size.x + 55, p.x, p.z, airForceAdjustment);
        LevelData.ts.airForcesSent++;
    }

    public void cancelAirForce()
    {
        sendAirforceMode = false;
        AFM_root.SetActive(false);
        AFM_ti.gameObject.SetActive(false);
    }

    public void cancelBuild()
    {
        if (buildMode)
        {
            //Reset
            if (selectedUnits.Count > 0) {
                selectedUnits[0].destroyBoth();
            }

            levelManager.deselectUnits();
            BM_root.SetActive(false);
            buildMode = false;
        }
    }

    void raycastHit(Ray ray)
    {
        RaycastHit hit = new RaycastHit();

        //Raycasts
        if (!BarBuildings.IsPointerOverUIElement() && !sendAirforceMode)
        {
            if (buildMode)
            {//PLACE
                mouseController.cursorType = MouseControlling.CursorType.SELECT;
                if (Physics.Raycast(ray, out hit, 300, 1 << 13))
                {
                    if (selectedUnits.Count > 0) selectedUnits[0].gameObject.SetActive(true);
                    if (selectedBody != null) selectedBody.gameObject.SetActive(true);
                    selectedRoot.position = hit.point + rootOffset;

                    //Place?
                    if (selectedUnits[0].canBePlaced())
                    {
                        selectedUnits[0].setMeshRendererMat(GlobalList.matHologramGreen);
                        if (Input.GetMouseButtonDown(0))
                        {
                            //Destroy(Instantiate(buildSmoke_particle, selectedUnit.transform.position, buildSmoke_particle.transform.localRotation), 1);
                            //build_wav.Play();
                            LevelData.ts.RemoveMoney((int)selectedUnits[0].value);//Pay money for unit
                            LevelData.ts.towersBuilt++;

                            UnitSerializable towerData = selectedUnits[0].serializeUnit();
                            towerData.setF(KEY_UNIT_HP, 1f);
                            towerData.setI(KEY_UNIT_TEAMID, LevelData.ts.teamId);
                            MapLevel.spawnUnit(towerData);
                        }
                    } else
                    {
                        selectedUnits[0].setMeshRendererMat(GlobalList.matHologramRed);
                    }
                }
                else
                {
                    if (selectedUnits.Count > 0) selectedUnits[0].gameObject.SetActive(false);
                    if (selectedBody != null) selectedBody.gameObject.SetActive(false);
                }
            }
            else
            { //SELECT
                if (Physics.Raycast(ray, out hit, 300, 1 << 14))
                {
                    mouseController.cursorType = MouseControlling.CursorType.SELECT;

                    Unit newHoveredUnit;
                    if (hit.collider.gameObject.TryGetComponent<UnitBody>(out _))
                    {
                        newHoveredUnit = hit.collider.gameObject.GetComponentInChildren<Unit>();
                    }
                    else
                    {
                        newHoveredUnit = hit.collider.gameObject.GetComponent<Unit>();
                    }

                    if (selectedUnits.Count == 0)
                    {
                        if (hoveredBuilding != newHoveredUnit)
                        {
                            ui_unitText.text = newHoveredUnit.name;
                            ui_unitIcon.texture = newHoveredUnit.icon;
                            ui_unitTeamFrame.color = newHoveredUnit.team.minimapColor;
                            ui_unitTeamLine.color = newHoveredUnit.team.minimapColor;
                            ui_unitTeamText.text = newHoveredUnit.team.name;
                            ui_unitTeamText.color = newHoveredUnit.team.id == Team.WHITE ? Color.black : Color.white;

                            ui_unitTab.SetActive(true);

                            showAdditionalProps(newHoveredUnit, true);

                            //And create new
                            deselectHPBars();

                            UnitHPBar uhpb = Instantiate(unitHPBarPrefab.gameObject, GameObject.Find("CanvasNoScale").transform).GetComponent<UnitHPBar>();
                            uhpb.linkUnit(newHoveredUnit);
                            unitHPBars.Add(uhpb);
                        } else
                        {
                            hoveredBuilding.getGameValUpdate(ui_unitHealth, ui_unitUpgrade, ui_unitHealthText, currentAdditionalProperties);
                        }
                    }

                    hoveredBuilding = newHoveredUnit;

                    //Select?
                    if (Input.GetMouseButtonDown(0))
                    {
                        ui_unitText.text = hoveredBuilding.name;
                        ui_unitIcon.texture = hoveredBuilding.icon;
                        ui_unitTeamFrame.color = hoveredBuilding.team.minimapColor;
                        ui_unitTeamLine.color = hoveredBuilding.team.minimapColor;
                        ui_unitTeamText.text = hoveredBuilding.team.name;
                        ui_unitTeamText.color = hoveredBuilding.team.id == Team.WHITE ? Color.black : Color.white;

                        deselectUnits();

                        selectUnit(hoveredBuilding);
                        showAdditionalProps(hoveredBuilding);
                    }
                }
                else
                {
                    if (selectedUnits.Count == 0)
                    {
                        ui_unitTab.SetActive(false);

                        if (currentAdditionalProperties != null)
                            Destroy(currentAdditionalProperties);

                        deselectHPBars();
                    }

                    //Deselect?
                    if (Input.GetMouseButtonDown(0) && selectedUnits.Count > 0)
                    {
                        deselectUnits();
                    }

                    hoveredBuilding = null; //Reset
                }
            }
        }

        if (selectedUnits.Count > 0 && currentAdditionalPropertiesInitalized == true)
        {
            selectedUnits[0].getGameValUpdate(ui_unitHealth, ui_unitUpgrade, ui_unitHealthText, currentAdditionalProperties);
        }
    }

    public void showAdditionalProps(Unit unit)
    {
        showAdditionalProps(unit, false);
    }

    public void showAdditionalProps(Unit unit, bool unitIsMouseHovered)
    {
        if (currentAdditionalProperties != null)
            Destroy(currentAdditionalProperties.gameObject);

        if (unit.propertiesUI_game != null)
        {
            currentAdditionalProperties = Instantiate(unit.propertiesUI_game, ui_unitTab.transform);
        }
        if (unitIsMouseHovered) currentAdditionalPropertiesInitalized_HoveredUnit = false;
        else currentAdditionalPropertiesInitalized = false;
    }

    //Time warping
    public void timeWarp_3() //4x speed
    {
        Time.timeScale = 4f;
        disableAllTimeWarps();
        setTimeWarp(ri_timeWarp_4_0);
    }

    public void timeWarp_2() //2x speed
    {
        Time.timeScale = 2f;
        disableAllTimeWarps();
        setTimeWarp(ri_timeWarp_2_0);
    }

    public void timeWarp_1() //1x speed - normal
    {
        Time.timeScale = 1f;
        disableAllTimeWarps();
        setTimeWarp(ri_timeWarp_1_0);
    }

    public void timeWarp_0() //0x speed - pause
    {
        Time.timeScale = 0f;
        disableAllTimeWarps();
        setTimeWarp(ri_timeWarp_0_0);
    }

    private void disableAllTimeWarps()
    {
        ri_timeWarp_0_0.color = COLOR_WHITE;
        ri_timeWarp_1_0.color = COLOR_WHITE;
        ri_timeWarp_2_0.color = COLOR_WHITE;
        ri_timeWarp_4_0.color = COLOR_WHITE;
    }

    public void selectUnit(Unit unit)
    {
        selectedUnits.Add(unit);

        UnitHPBar uhpb = Instantiate(unitHPBarPrefab.gameObject, GameObject.Find("CanvasNoScale").transform).GetComponent<UnitHPBar>();
        uhpb.linkUnit(unit);
        unitHPBars.Add(uhpb);
    }

    public void deselectUnits()
    {
        deselectHPBars();
        if (selectedUnits.Count > 0)
        {
            if (selectedUnits[0].virtualSpace == Unit.VirtualSpace.GHOST)
                selectedUnits[0].destroyBoth();
            selectedUnits.Clear();
        }
    }

    private void deselectHPBars()
    {
        //Detroy old hp bars
        foreach (UnitHPBar uhpb in unitHPBars)
            if (uhpb != null)
                Destroy(uhpb.gameObject);
    }

    private void setTimeWarp(RawImage ri)
    {
        ri.color = COLOR_GREEN;
    }

    public void endGame(int teamWinnerId, string reason)
    {
        gameIsRunning = false;
        gameStatsManager.OnGameEnd(teamWinnerId, reason);
    }

    public void teamOvertakeUnits(int teamFromId, int teamToId)
    {
        Team teamFrom = GlobalList.teams[teamFromId];
        Team teamTo = GlobalList.teams[teamToId];

        LevelData.teamStats[teamFromId].activeTeam = false;
        LevelData.teamStats[teamToId].activeTeam = true;

        for (int i = 0; i < LevelData.units.Count; i++)
        {
            Unit u = LevelData.units[i];
            if (u.team == teamFrom)
            {
                u.team = teamTo;
                u.deserializeUnit(u.serializeUnit());
                u.restoreMeshRendererMat(u.team);
            }
        }
    }
}
