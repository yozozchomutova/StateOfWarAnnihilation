using UnityEngine;

public class EditorTopBar : MonoBehaviour {

    [Header("List of editing Panels/Sidebars")]
    public GameObject terrainEditBar;
    public GameObject navigationsBarBcg;
    public GameObject mapObjectsBarBcg;
    public GameObject unitsBarBcg;
    public GameObject eventsPanel;
    public GameObject levelInfoPanel;
    public GameObject environmentPanel;

    [Header("List of file managing Panels")]
    public GameObject newLevelPanel;
    public GameObject loadLevelPanel;
    public GameObject saveSummaryPanel;
    public GameObject exitConfirmPanel;

    [HideInInspector] private GameObject currentPanel;

    [HideInInspector] private BarTerrainPaint barTerrainPaint;

    private void Start()
    {
        barTerrainPaint = terrainEditBar.GetComponent<BarTerrainPaint>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ShowElement("");
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            ShowElement("terrain_edit");
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            ShowElement("terrain_paint");
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            ShowElement("");
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            ShowElement("map_objects");
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            ShowElement("units");
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            ShowElement("events");
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            ShowElement("level_info");
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            ShowElement("environment");
    }

    public void ShowElement(string id)
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
        }

        if (id == "terrain_edit")
        {
            barTerrainPaint.modeSelector.value = 0;
            barTerrainPaint.onModeChange();
        } else if (id == "terrain_paint")
        {
            barTerrainPaint.modeSelector.value = 1;
            barTerrainPaint.onModeChange();
        }

        currentPanel = id == "terrain_edit" ? terrainEditBar :
            id == "terrain_paint" ? terrainEditBar :
            id == "navigations" ? navigationsBarBcg :
            id == "map_objects" ? mapObjectsBarBcg :
            id == "units" ? unitsBarBcg :
            id == "events" ? eventsPanel :
            id == "level_info" ? levelInfoPanel :
            id == "environment" ? environmentPanel :

            id == "new_level" ? newLevelPanel :
            id == "load_level" ? loadLevelPanel :
            id == "save_level" ? saveSummaryPanel :
            id == "exit_editor" ? exitConfirmPanel :
            null;

        if (currentPanel != null)
            currentPanel.SetActive(true);
    }
 }
