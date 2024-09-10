using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static EditorManager;
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
    
    [HideInInspector] private Unit newHoveredUnit, lastHoveredUnit;

    void Start()
    {

    }

    void Update()
    {
        raycastHit();

        //Rotate
        if (toolType == ToolType.PLACE) {
            if (Input.mouseScrollDelta.y > 0.0f) { //Up
                selectedRoot.eulerAngles = new Vector3(selectedRoot.eulerAngles.x, selectedRoot.eulerAngles.y + 15, 0);
            } else if (Input.mouseScrollDelta.y < 0.0f) { //Down
                selectedRoot.eulerAngles = new Vector3(selectedRoot.eulerAngles.x, selectedRoot.eulerAngles.y - 15, 0);
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
        if (!currentAdditionalPropertiesInitalized_HoveredUnit && lastHoveredUnit != null && (currentAdditionalProperties != null || lastHoveredUnit.propertiesUI_editor == null))
        {
            lastHoveredUnit.getValues(unitHealthSlider, out selectedTeamID, currentAdditionalProperties);
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
                rayPlaceUnit(ray);
            } else if (toolType == ToolType.SELECT) { //SELECT
                raySelectUnit(ray);
            } else if (toolType == ToolType.DESTROY) {//DESTROY
                rayDestroyUnit(ray);
            }
        }
    }

    private void rayPlaceUnit(Ray ray)
    {
        if (Physics.Raycast(ray, out hit, 300, 1 << 13))
        {
            CursorManager.SetCursor(CursorManager.spriteBuild);

            if (selectedUnit != null)
            {
                selectedUnit.gameObject.SetActive(true);
            }

            float unitPosX, unitPosZ;
            int unitGridX, unitGridY;
            (unitPosX, unitPosZ) = LevelData.gridManager.AlignPosition(hit.point.x, hit.point.z);
            (unitGridX, unitGridY) = LevelData.gridManager.SamplePosition(hit.point.x, hit.point.z);
            selectedRoot.position = new Vector3(unitPosX, hit.point.y, unitPosZ) + rootOffset;

            //Check if colliding with occupied tiles in grid system
            bool canBePlaced = selectedUnit.canBePlaced(LevelData.Scene.EDITOR);
            LevelData.gridManager.UpdateSelection(unitGridX, unitGridY, selectedUnit.gridSize.x, selectedUnit.gridSize.y, canBePlaced, selectedRoot.eulerAngles.y);

            globalInfo.text = String.Format("XYZ: {0:F1}, {1:F1}, {2:F1}", selectedRoot.position.x, selectedRoot.position.y, selectedRoot.position.z);

            //Place?
            if (Input.GetMouseButtonDown(0) && canBePlaced)
            {
                Destroy(Instantiate(buildSmoke_particle, selectedUnit.transform.position, buildSmoke_particle.transform.localRotation), 1);
                build_wav.Play();

                UnitSerializable data = selectedUnit.serializeUnit();
                data.setI(KEY_UNIT_TEAMID, getSelectedTeam().id);
                data.setF(KEY_UNIT_HP, unitHealthSlider.value);
                Unit newUnit = MapLevel.spawnUnit(data);

                LevelData.gridManager.PlaceUnit(newUnit, unitGridX, unitGridY, selectedRoot.eulerAngles.y);
                LevelData.gridManager.ShowGrid();

                //Save unit values
                newUnit.setValues(unitHealthSlider.value, getSelectedTeam(), currentAdditionalProperties);

                //Undo
                byte[] undoData = new byte[8];
                BitConverter.GetBytes(unitGridX).CopyTo(undoData, 0); //GridX
                BitConverter.GetBytes(unitGridY).CopyTo(undoData, 4); //GridY
                EditorManager.self.createUndoAction(EditorManager.UndoType.PLACE_UNIT, undoData, 0);
            }

            selectedUnit.setMeshRendererMat(canBePlaced ? GlobalList.matHologramGreen : GlobalList.matHologramRed);
        }
        else
        {
            if (selectedUnit != null) selectedUnit.gameObject.SetActive(false);
            LevelData.gridManager.UpdateSelection(0, 0, 0, 0, true);
        }
    }

    private void raySelectUnit(Ray ray)
    {
        //Cast ray to hit unit collider
        newHoveredUnit = null;
        bool success = Physics.Raycast(ray, out hit, 300, 1 << 14);
        if (success)
            newHoveredUnit = Unit.getUnit(hit.collider);

        if (newHoveredUnit != null)
        {
            globalInfo.text = "Unit ID: " + LevelData.units.IndexOf(newHoveredUnit);
            CursorManager.SetCursor(CursorManager.spriteSelect);
        }

        if (selectedUnit == null)
        {
            if (lastHoveredUnit != newHoveredUnit)
            { //New unit being hovered on
                if (lastHoveredUnit)
                {
                    lastHoveredUnit.DeSelectUnit();

                    unitName.text = "";
                    unitTab.SetActive(false);

                    if (currentAdditionalProperties != null)
                        Destroy(currentAdditionalProperties);
                }

                if (newHoveredUnit)
                {
                    newHoveredUnit.SelectUnit();

                    unitName.text = newHoveredUnit.name;
                    unitIcon.texture = newHoveredUnit.icon;

                    unitTab.SetActive(true);

                    showAdditionalProps(newHoveredUnit, true);
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (selectedUnit != null)
            {
                selectedUnit.DeSelectUnit(); //Deselect effect
                selectedUnit.setValues(unitHealthSlider.value, getSelectedTeam(), currentAdditionalProperties); //Save unit values
                selectedUnit.restoreMeshRendererMat(getSelectedTeam()); //Restore body material

                selectedUnit = null; //NULLate
                unitTab.SetActive(false); //Hide
            } else if (newHoveredUnit != null) //Selects unit
            {
                selectedUnit = newHoveredUnit;
                showAdditionalProps(newHoveredUnit);
            }
        }

        lastHoveredUnit = newHoveredUnit;
    }

    private void rayDestroyUnit(Ray ray)
    {
        newHoveredUnit = null;
        if (Physics.Raycast(ray, out hit, 300, 1 << 14))
        {
            newHoveredUnit = Unit.getUnit(hit.collider);

            CursorManager.SetCursor(CursorManager.spriteAttack); //DESTROY

            if (lastHoveredUnit != newHoveredUnit)
                newHoveredUnit.SelectUnit();

            //Destroy?
            if (Input.GetMouseButtonDown(0))
            {
                var (unitGridX, unitGridY) = LevelData.gridManager.SamplePosition(newHoveredUnit.transform.position.x, newHoveredUnit.transform.position.z);

                //Undo
                byte[] undoData = ArrayWorker.SerializableToBytes<UnitSerializable>(newHoveredUnit.serializeUnit());
                EditorManager.self.createUndoAction(EditorManager.UndoType.REMOVE_UNIT, undoData, 0);

                //Remove from game
                Destroy(Instantiate(buildSmoke_particle, hit.collider.gameObject.transform.position, buildSmoke_particle.transform.localRotation), 1);
                remove_wav.Play();

                LevelData.gridManager.RemoveUnit(unitGridX, unitGridY);
                //LevelData.gridManager.ShowGrid();

                newHoveredUnit.destroyBoth();
            }
        }

        if (lastHoveredUnit && (lastHoveredUnit != newHoveredUnit)) //Deselects
        {
            lastHoveredUnit.DeSelectUnit();
        }

        lastHoveredUnit = newHoveredUnit;
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
    }

    private void OnDisable()
    {
        //Reset
        if (toolType == ToolType.PLACE)
        {
            if (selectedUnit != null)
                selectedUnit.destroyBoth();
            LevelData.gridManager.UpdateSelection(0, 0, 0, 0, true);
        } else if (toolType == ToolType.SELECT) {
            selectedUnit = null; //NULLate
            unitTab.SetActive(false); //Hide
        }
    }

    public void onToolTypeChange(int toolID)
    {
        toolType = (ToolType) toolID;

        unitName.text = "";
        unitIcon.texture = null;
        unitTab.SetActive(false);
        unitTypeSelectRoot.SetActive(false);

        //UI changes
        tool_build.colors = GlobalList.btnNormal1;
        tool_select.colors = GlobalList.btnNormal1;
        tool_destroy.colors = GlobalList.btnNormal1;

        //Reset
        if (selectedUnit != null)
            selectedUnit.destroyBoth();
        LevelData.gridManager.UpdateSelection(0, 0, 0, 0, true);

        if (toolType == ToolType.PLACE) //Place
        {
            tool_build.colors = GlobalList.btnHighlight1;

            unitTab.SetActive(true);
            unitTypeSelectRoot.SetActive(true);

            onBuildingSelectionChange();
        } else if (toolType == ToolType.SELECT) //Select
        {
            tool_select.colors = GlobalList.btnHighlight1;
        } else if (toolType == ToolType.DESTROY) //Destroy
        {
            tool_destroy.colors = GlobalList.btnHighlight1;
        }
    }

    public void onUnitTypeChange(int unitTypeID)
    {
        unitType = (UnitType) unitTypeID;

        //UI changes
        ut_buildings.colors = GlobalList.btnNormal1;
        ut_towers.colors = GlobalList.btnNormal1;
        ut_units.colors = GlobalList.btnNormal1;

        if (unitType == UnitType.BUILDING) {
            ut_buildings.colors = GlobalList.btnHighlight1;
            onBuildingSelectionChange("0_commandCenter1");
        } else if (unitType == UnitType.TOWER) {
            ut_towers.colors = GlobalList.btnHighlight1;
            onBuildingSelectionChange("0_antT1");
        } else if (unitType == UnitType.UNIT) {
            ut_units.colors = GlobalList.btnHighlight1;
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

    public static Vector3 GetTerrainNormal(Vector3 position, Terrain terrain)
    {
        // Convert world position to terrain position
        Vector3 terrainPosition = position - terrain.transform.position;

        // Get the normalized terrain coordinates (from 0 to 1)
        Vector3 normalizedPos = new Vector3(
            terrainPosition.x / terrain.terrainData.size.x,
            terrainPosition.y / terrain.terrainData.size.y,
            terrainPosition.z / terrain.terrainData.size.z
        );

        // Get the terrain normal at the given position
        return terrain.terrainData.GetInterpolatedNormal(normalizedPos.x, normalizedPos.z);
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
