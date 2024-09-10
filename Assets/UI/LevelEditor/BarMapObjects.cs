using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Drawing;
using static BarBuildings;
using Unity.VisualScripting;

public class BarMapObjects : MonoBehaviour
{
    //class data
    private RaycastHit hit;
    private Vector3 lastPlacedTreePos;

    //UI
    public RawImage objectIcon;
    public Text objectName;

    public Button tool_build, tool_destroy;
    public Slider spacingSlider;

    //Settings
    private float spacingValue;
    private bool buldozeMode;

    //Currently selected object
    private int selectedID; //Only for build
    private MapObject selectedObject;

    [Header("UI Building references")]
    public GameObject buildPanel;
    public Transform uiObjectList;
    public UIListElement1 uiElement1;
    private UIListElement1[] objectUIElements;

    #region [Functions] Unity functions (Start/Update)
    void Awake()
    {
        //Fill mapObject-selection list
        int x = 5, y = -5;
        objectUIElements = new UIListElement1[GlobalList.mapObjects.Count];
        for (int i = 0; i < objectUIElements.Length; i++)
        {
            MapObject obj = GlobalList.mapObjects.ElementAt(i).Value;

            int finalI = i;
            UIListElement1 newElement = Instantiate(uiElement1, uiObjectList);
            Button.ButtonClickedEvent clickEvent = new Button.ButtonClickedEvent();
            clickEvent.AddListener(delegate { OnObjectSelectionChange(finalI); });
            newElement.btn.onClick = clickEvent;
            newElement.setPosition(x, y);
            newElement.updateParameters(obj.icon, obj.name);
            objectUIElements[i] = newElement;

            x += 53;
            if ((i + 1) % 4 == 0)
            {
                x = 5;
                y -= 69;
            }
        }
    }

    void Update()
    {
        raycastHit();

        //Rotate
        if (!buldozeMode && selectedObject)
        {
            Transform selectedRoot = selectedObject.transform;
            if (Input.mouseScrollDelta.y > 0.0f)
            { //Up
                selectedRoot.eulerAngles = new Vector3(selectedRoot.eulerAngles.x, selectedRoot.eulerAngles.y + 15, 0);
            }
            else if (Input.mouseScrollDelta.y < 0.0f)
            { //Down
                selectedRoot.eulerAngles = new Vector3(selectedRoot.eulerAngles.x, selectedRoot.eulerAngles.y - 15, 0);
            }
        }
    }

    private void OnEnable()
    {
        OnToolSelect(false);
        onSpacingValueChange();
    }

    private void OnDisable()
    {
        DestroySelectedObject();
        LevelData.gridManager.UpdateSelection(0, 0, 0, 0, true);
    }
    #endregion
    void raycastHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hit = new RaycastHit();

        //Raycasts
        if (!IsPointerOverUIElement())
        {
            if (buldozeMode)
                rayBuldozeMode(ray);
            else
                rayBuildMode(ray);
        }
    }

    private void rayBuildMode(Ray ray)
    {
        if (Physics.Raycast(ray, out hit, 300, 1 << 13))
        {
            selectedObject.gameObject.SetActive(true);

            //Implemented from: BarBuildings.cs
            float unitPosX, unitPosZ;
            int unitGridX, unitGridY;
            (unitPosX, unitPosZ) = LevelData.gridManager.AlignPosition(hit.point.x, hit.point.z);
            (unitGridX, unitGridY) = LevelData.gridManager.SamplePosition(hit.point.x, hit.point.z);
            selectedObject.transform.position = new Vector3(unitPosX, hit.point.y, unitPosZ);

            //Check if colliding with occupied tiles in grid system
            bool canBePlaced = selectedObject.CanBePlaced();

            //Visual
            LevelData.gridManager.UpdateSelection(unitGridX, unitGridY, selectedObject.gridSize.x, selectedObject.gridSize.y, canBePlaced, selectedObject.transform.eulerAngles.y);
            selectedObject.SetMeshRendererMat(canBePlaced ? GlobalList.matHologramGreen : GlobalList.matHologramRed);

            if (Input.GetMouseButtonDown(0) && canBePlaced)
            {
                AddObject(selectedObject.transform, unitGridX, unitGridY);
                lastPlacedTreePos = hit.point;
            }
            else if (Input.GetMouseButton(0) && canBePlaced)
            {
                if (Vector3.Distance(hit.point, lastPlacedTreePos) > spacingValue)
                {
                    AddObject(selectedObject.transform, unitGridX, unitGridY);
                    lastPlacedTreePos = hit.point;
                }
            }
        }
        else
        {
            selectedObject.gameObject.SetActive(false);
            LevelData.gridManager.UpdateSelection(0, 0, 0, 0, true);
        }
    }

    private void rayBuldozeMode(Ray ray)
    {
        if (Physics.Raycast(ray, out hit, 300, 1 << 12))
        {
            MapObject mp = hit.collider.gameObject.GetComponent<MapObject>();

            if (mp != selectedObject) //Different object
            {
                mp.Select();
                if (selectedObject) selectedObject.DeSelect();
            }
            
            selectedObject = mp;
            
            if (Input.GetMouseButtonDown(0))
            {
                LevelData.mapObjects.Remove(mp);
                var (unitGridX, unitGridY) = LevelData.gridManager.SamplePosition(mp.transform.position.x, mp.transform.position.z);
                LevelData.gridManager.RemoveMapObject(unitGridX, unitGridY);
                Destroy(hit.collider.gameObject);
            }
        }
        else
        {
            if (selectedObject)
            {
                selectedObject.DeSelect();
                selectedObject = null;
            }
        }
    }

    public void AddObject(Transform t, int gridX, int gridY)
    {
        MapObject mapObject = GlobalList.mapObjects.ElementAt(selectedID).Value;

        MapObject newObject = Instantiate(mapObject.gameObject).GetComponent<MapObject>();
        newObject.transform.position = t.position;
        newObject.transform.eulerAngles = t.eulerAngles;
        LevelData.gridManager.PlaceMapObject(newObject, gridX, gridY, t.eulerAngles.y);
        LevelData.mapObjects.Add(newObject);
    }

    public void OnObjectSelectionChange(int id)
    {
        this.selectedID = id;
        for (int i = 0; i < objectUIElements.Length; i++)
        {
            objectUIElements[i].setHighlight(i == selectedID);
        }

        //Destroy & initiate new object
        DestroySelectedObject();

        MapObject selectedMo = GlobalList.mapObjects.ElementAt(selectedID).Value;
        selectedObject = Instantiate(selectedMo.gameObject).GetComponent<MapObject>();

        //Change visuals
        objectIcon.texture = selectedMo.icon;
        objectName.text = selectedMo.objectName;
    }

    private void DestroySelectedObject()
    {
        if (selectedObject)
            Destroy(selectedObject.gameObject);
    }
    #region [Functions] On UI Change
    public void OnToolSelect(bool buldoze)
    {
        buldozeMode = buldoze;

        //UI changes
        tool_build.colors = GlobalList.btnNormal1;
        tool_destroy.colors = GlobalList.btnNormal1;

        //Reset
        DestroySelectedObject();
        LevelData.gridManager.UpdateSelection(0, 0, 0, 0, true);

        buildPanel.SetActive(!buldoze);
        if (!buldoze) //Place
        {
            tool_build.colors = GlobalList.btnHighlight1;

            OnObjectSelectionChange(0);
        }
        else if (buldoze) //Destroy
        {
            tool_destroy.colors = GlobalList.btnHighlight1;
        }
    }

    public void onSpacingValueChange()
    {
        spacingValue = spacingSlider.value;
    }
    #endregion
    #region [Functions] Implemented from somewhere
    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }

    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
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
    #endregion
}
