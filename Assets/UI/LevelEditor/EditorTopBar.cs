using UnityEngine;

public class EditorTopBar : MonoBehaviour {

    [Header("List of editing Panels/Sidebars")]
    public GameObject terrainEditBarBcg;
    public GameObject terrainPaintBarBcg;
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
        barTerrainPaint = terrainPaintBarBcg.GetComponent<BarTerrainPaint>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && currentPanel != null)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
        }
    }

    public void ShowElement(string id)
    {
        if (id == "terrain_edit")
        {
            barTerrainPaint.modeSelector.value = 0;
            barTerrainPaint.onModeChange();
        } else if (id == "terrain_paint")
        {
            barTerrainPaint.modeSelector.value = 1;
            barTerrainPaint.onModeChange();
        }

        currentPanel = id == "terrain_edit" ? terrainPaintBarBcg :
            id == "terrain_paint" ? terrainPaintBarBcg :
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

        currentPanel.SetActive(true);
    }
 }
