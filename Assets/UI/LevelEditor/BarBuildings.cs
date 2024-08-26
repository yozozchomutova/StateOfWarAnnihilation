using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static MapLevelManager;
using static MapLevelManager.UnitSerializable;

public class BarBuildings : MonoBehaviour
{
    //Tool type
    private ToolType toolType; //Place, Selection, Destroy

    //> Build tool
    public GameObject unitTypeSelectRoot;
    private UnitType unitType; //Building, Tower, Unit

    //> Select tool

    //> Destroy tool

    //Header
    public Text unitName;
    public Text globalInfo;
    public Image unitIconBcg;
    public RawImage unitIcon;
    public Slider unitHealthSlider;
    private int selectedTeamID;

    public GameObject unitTab;

    public Button tool_build, tool_select, tool_destroy;
    public Button ut_buildings, ut_towers, ut_units;

    private ColorBlock tool_btn_normal, tool_btn_highlighted;

    //Building global properties
    [Header("Global properties")]
    public SelectUnitPanel buildingSelectionPanel;
    public GameObject buildingPropsPanel;

    //Tower global properties
    [Header("Tower properties")]
    public SelectUnitPanel towerSelectionPanel;

    //Unit global properties
    [Header("Unit properties")]
    public SelectUnitPanel unitSelectionPanel;

    //Editing
    [HideInInspector] private RaycastHit hit;
    public Transform mouseSphere;
    public Transform objectTrans;

    [Header("Effects")]
    public GameObject buildSmoke_particle;

    [Header("Audio")]
    public AudioSource build_wav;
    public AudioSource remove_wav;

    //List
    [HideInInspector] private string selectedUnitID;
    [HideInInspector] private Unit selectedUnit;
    [HideInInspector] private Transform selectedRoot; //= selectbody OR selectUnit if selectbody is null
    [HideInInspector] private Vector3 rootOffset = Vector3.zero; //= Offset of: selectedRoot
    [HideInInspector] private GameObject currentAdditionalProperties;
    [HideInInspector] private bool currentAdditionalPropertiesInitalized = true;
    [HideInInspector] private bool currentAdditionalPropertiesInitalized_HoveredUnit = true;
    
    [HideInInspector] private Unit hoveredBuilding;

    void Start()
    {
        //init color blocks
        tool_btn_normal = new ColorBlock();
        tool_btn_normal.colorMultiplier = 1;
        tool_btn_normal.normalColor = new Color32(0, 0, 0, byte.MaxValue);
        tool_btn_normal.highlightedColor = new Color32(240, 240, 240, byte.MaxValue);
        tool_btn_normal.selectedColor = new Color32(240, 240, 240, byte.MaxValue);

        tool_btn_highlighted = new ColorBlock();
        tool_btn_highlighted.colorMultiplier = 1;
        tool_btn_highlighted.normalColor = new Color32(240, 240, 240, byte.MaxValue);
        tool_btn_highlighted.highlightedColor = new Color32(240, 240, 240, byte.MaxValue);
        tool_btn_highlighted.selectedColor = new Color32(240, 240, 240, byte.MaxValue);
    }

    void Update()
    {
        raycastHit();

        //Rotate
        if (toolType == ToolType.PLACE) {
            if (Input.mouseScrollDelta.y > 0.0f) { //Up
                selectedRoot.eulerAngles = new Vector3(selectedRoot.eulerAngles.x, selectedRoot.eulerAngles.y + 10, 0);
            } else if (Input.mouseScrollDelta.y < 0.0f) { //Down
                selectedRoot.eulerAngles = new Vector3(selectedRoot.eulerAngles.x, selectedRoot.eulerAngles.y - 10, 0);
            }
        }
    }
    private void LateUpdate()
    {
        //Preventing random error, that automatically unchecks toggle (and other weird stuff) after instantiate for no reason :/
        if (!currentAdditionalPropertiesInitalized && selectedUnit != null && (currentAdditionalProperties != null || selectedUnit.propertiesUI_editor == null)) {
            if (toolType == ToolType.PLACE)
            {
                selectedUnit.getValues(unitHealthSlider, out _, currentAdditionalProperties);
            } else
            {
                selectedUnit.getValues(unitHealthSlider, out selectedTeamID, currentAdditionalProperties);
            }

            setTeamID(selectedTeamID);
            currentAdditionalPropertiesInitalized = true;
        }
        if (!currentAdditionalPropertiesInitalized_HoveredUnit && hoveredBuilding != null && (currentAdditionalProperties != null || hoveredBuilding.propertiesUI_editor == null))
        {
            hoveredBuilding.getValues(unitHealthSlider, out selectedTeamID, currentAdditionalProperties);
            setTeamID(selectedTeamID);
            currentAdditionalPropertiesInitalized_HoveredUnit = true;
        }
    }

    void raycastHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hit = new RaycastHit();

        //Raycasts
        if (!IsPointerOverUIElement()) {
            if (toolType == ToolType.PLACE) {//PLACE
                if (Physics.Raycast(ray, out hit, 300, 1 << 13)) {
                    if (selectedUnit != null) selectedUnit.gameObject.SetActive(true);
                    selectedRoot.position = hit.point + rootOffset;

                    float mapSize = LevelData.mainTerrain.terrainData.size.x / 4f;
                    globalInfo.text = String.Format("XYZ: {0:F1}, {1:F1}, {2:F1}", selectedRoot.position.x - mapSize, selectedRoot.position.y, selectedRoot.position.z - mapSize);

                    //Place?
                    if (Input.GetMouseButtonDown(0) && !selectedUnit.isColliding()) {
                        Destroy(Instantiate(buildSmoke_particle, selectedUnit.transform.position, buildSmoke_particle.transform.localRotation), 1);
                        build_wav.Play();

                        UnitSerializable data = selectedUnit.serializeUnit();
                        data.setI(KEY_UNIT_TEAMID, getSelectedTeam().id);
                        data.setF(KEY_UNIT_HP, unitHealthSlider.value);
                        Unit newUnit = MapLevel.spawnUnit(data);

                        //Save unit values
                        newUnit.setValues(unitHealthSlider.value, getSelectedTeam(), currentAdditionalProperties);
                    }
                } else {
                    if (selectedUnit != null) selectedUnit.gameObject.SetActive(false);
                }
            } else if (toolType == ToolType.SELECT) { //SELECT
                if (Physics.Raycast(ray, out hit, 300, 1 << 14)) {
                    mouseSphere.gameObject.SetActive(true);
                    mouseSphere.transform.position = hit.point;

                    Unit newHoveredUnit = Unit.getUnit(hit.collider);

                    globalInfo.text = "Unit ID: " + LevelData.units.IndexOf(newHoveredUnit);

                    if (selectedUnit == null) {
                        if (hoveredBuilding != newHoveredUnit) {
                            unitName.text = newHoveredUnit.name;
                            unitIcon.texture = newHoveredUnit.icon;

                            unitTab.SetActive(true);

                            showAdditionalProps(newHoveredUnit, true);
                        }
                    }

                    hoveredBuilding = newHoveredUnit;

                    //Select?
                    if (Input.GetMouseButtonDown(0)) {
                        showAdditionalProps(hoveredBuilding);
                        selectedUnit = hoveredBuilding;
                    }
                } else {
                    mouseSphere.gameObject.SetActive(false);

                    if (selectedUnit == null) {
                        unitName.text = "";
                        unitTab.SetActive(false);

                        if (currentAdditionalProperties != null)
                            Destroy(currentAdditionalProperties);
                    }

                    //Deselect?
                    if (Input.GetMouseButtonDown(0) && selectedUnit != null) {
                        //Save unit values
                        selectedUnit.setValues(unitHealthSlider.value, getSelectedTeam(), currentAdditionalProperties);

                        //Save body values
                        selectedUnit.restoreMeshRendererMat(getSelectedTeam());

                        selectedUnit = null; //NULLate

                        unitTab.SetActive(false); //Hide
                    }

                    hoveredBuilding = null; //Reset
                }
            } else if (toolType == ToolType.DESTROY) {//DESTROY
                if (Physics.Raycast(ray, out hit, 300, 1 << 14)) {
                    mouseSphere.gameObject.SetActive(true);
                    mouseSphere.transform.position = hit.point;

                    //Destroy?
                    if (Input.GetMouseButtonDown(0)) {
                        Destroy(Instantiate(buildSmoke_particle, hit.collider.gameObject.transform.position, buildSmoke_particle.transform.localRotation), 1);
                        remove_wav.Play();

                        Unit u = Unit.getUnit(hit.collider);
                        if (Unit.getUnit(hit.collider).hasBodyID())
                        {
                            Destroy(hit.collider.transform.parent.parent.gameObject);

                        } else
                        {
                            Destroy(hit.collider.gameObject);
                        }
                        LevelData.units.Remove(u);
                    }
                } else {
                    mouseSphere.gameObject.SetActive(false);
                }
            }
        }
    }

    private void OnEnable()
    {
        if (toolType == ToolType.PLACE)
        {
            //Default value
            selectedUnitID = GlobalList.units.Keys.ElementAt(0);

            //Update UI
            onBuildingSelectionChange();
        }

        mouseSphere.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        //Reset
        if (toolType == ToolType.PLACE)
        {
            if (selectedUnit != null)
                selectedUnit.destroyBoth();
        } else if (toolType == ToolType.SELECT) {
            selectedUnit = null; //NULLate
            unitTab.SetActive(false); //Hide
        }

        mouseSphere.gameObject.SetActive(false);
    }

    public void onToolTypeChange(int toolID)
    {
        toolType = (ToolType) toolID;

        unitName.text = "";
        mouseSphere.gameObject.SetActive(false);
        unitIcon.texture = null;
        unitTab.SetActive(false);
        unitTypeSelectRoot.SetActive(false);

        //UI changes
        tool_build.colors = tool_btn_normal;
        tool_select.colors = tool_btn_normal;
        tool_destroy.colors = tool_btn_normal;

        //Reset
        if (selectedUnit != null)
            selectedUnit.destroyBoth();

        if (toolType == ToolType.PLACE) //Place
        {
            tool_build.colors = tool_btn_highlighted;

            mouseSphere.gameObject.SetActive(false);

            unitTab.SetActive(true);
            unitTypeSelectRoot.SetActive(true);

            onBuildingSelectionChange();
        } else if (toolType == ToolType.SELECT) //Select
        {
            tool_select.colors = tool_btn_highlighted;
        } else if (toolType == ToolType.DESTROY) //Destroy
        {
            tool_destroy.colors = tool_btn_highlighted;
        }
    }

    public void onUnitTypeChange(int unitTypeID)
    {
        unitType = (UnitType) unitTypeID;

        //UI changes
        ut_buildings.colors = tool_btn_normal;
        ut_towers.colors = tool_btn_normal;
        ut_units.colors = tool_btn_normal;

        if (unitType == UnitType.BUILDING) {
            ut_buildings.colors = tool_btn_highlighted;
            onBuildingSelectionChange("0_commandCenter1");
        } else if (unitType == UnitType.TOWER) {
            ut_towers.colors = tool_btn_highlighted;
            onBuildingSelectionChange("0_antT1");
        } else if (unitType == UnitType.UNIT) {
            ut_units.colors = tool_btn_highlighted;
            onBuildingSelectionChange("0_tonk1");
        }
    }

    public void OnUnitIconClick()
    {
        if (toolType == ToolType.PLACE) {//Build
            if (unitType == UnitType.BUILDING) { //Building
                buildingSelectionPanel.setSelectCallback(new SC_SUP_Building(this));
                buildingSelectionPanel.selectUnitPanel.show();
            } else if (unitType == UnitType.TOWER) { //Tower
                towerSelectionPanel.setSelectCallback(new SC_SUP_Building(this));
                towerSelectionPanel.selectUnitPanel.show();
            } else if (unitType == UnitType.UNIT) { //Unit
                unitSelectionPanel.setSelectCallback(new SC_SUP_Building(this));
                unitSelectionPanel.enableFilterAll();
                unitSelectionPanel.selectUnitPanel.show();
            }
        }
    }

    public void onBuildingSelectionChange(string newId)
    {
        this.selectedUnitID = newId;
        onBuildingSelectionChange();
    }

    public void onBuildingSelectionChange()
    {
        //Reset
        if (selectedUnit != null)
            selectedUnit.destroyBoth();

        //New
        UnitSerializable us = GlobalList.units[selectedUnitID].serializeUnit();
        us.setF(KEY_UNIT_HP, 1f);

        selectedUnit = MapLevel.spawnUnit(us, Unit.VirtualSpace.GHOST);
        
        selectedUnit.setMeshRendererMat(GlobalList.matHologramGreen);

        selectedRoot = selectedUnit.getRoot();

        unitName.text = selectedUnit.name;
        unitIcon.texture = selectedUnit.icon;

        showAdditionalProps(selectedUnit);
    }

    public void showAdditionalProps(Unit unit)
    {
        showAdditionalProps(unit, false);
    }

    public void showAdditionalProps(Unit unit, bool unitIsMouseHovered)
    {
        if (currentAdditionalProperties != null)
            Destroy(currentAdditionalProperties.gameObject);

        if (unit.propertiesUI_editor != null)
        {
            currentAdditionalProperties = Instantiate(unit.propertiesUI_editor, unitTab.transform);
            currentAdditionalProperties.GetComponent<RectTransform>().anchoredPosition = new Vector2(-65, -35f);

        }
        if (unitIsMouseHovered) currentAdditionalPropertiesInitalized_HoveredUnit = false;
        else currentAdditionalPropertiesInitalized = false;
    }

    public void changeSelectedBuilding(GameObject buildingProp, Texture buildingIcon, GameObject buildingShadow)
    {
        buildingPropsPanel.SetActive(true);
        buildingProp.SetActive(true);
        unitIcon.texture = buildingIcon;
    }

    public void setTeamID(int newTeamID)
    {
        if (selectedTeamID == Team.WHITE && newTeamID != Team.WHITE) //Coming from white => normal buildings have 100%
        {
            unitHealthSlider.value = 1f;
        }
        if (newTeamID == Team.WHITE) //Neutral buildings have usually 50%
        {
            unitHealthSlider.value = 0.5f; 
        }

        selectedTeamID = newTeamID;
        unitIconBcg.color = getSelectedTeam().minimapColor * new Color(1f, 1f, 1f, 0.1f);
    }

    private Team getSelectedTeam()
    {
        return GlobalList.teams[selectedTeamID];
    }

    //-<==============>-
    //-<= INTERFACES =>-
    //-<==============>-

    //Select callback _ Select unit panel _ ...
    public class SC_SUP_Building : SelectUnitPanel.SelectCallback { BarBuildings bb; public SC_SUP_Building(BarBuildings bb) { this.bb = bb; } public void OnSelect(string id) {
            bb.selectedUnitID = id;
            bb.onBuildingSelectionChange();
     }}

    public class SC_SUP_Unit : SelectUnitPanel.SelectCallback
    {
        public void OnSelect(string id)
        {

        }
    }

    //  |=>------------------------------------------------------------------------------<=|
    //  |=>------------------------------<IMPLEMENTED>-----------------------------------<=|
    //  |=>------------------------------------------------------------------------------<=|
    //Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == 5) //5 = UI
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    public enum ToolType
    {
        PLACE, SELECT, DESTROY
    }

    public enum UnitType
    {
        BUILDING, TOWER, UNIT
    }
}
