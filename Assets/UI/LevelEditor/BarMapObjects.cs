using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BarMapObjects : MonoBehaviour
{
    //class data
    private RaycastHit hit;
    private Vector3 lastPlacedTreePos;

    //UI
    public RawImage objectIcon;
    public Text objectName;
    public Dropdown objectSelection;

    public Toggle buldozeToggle;
    public Toggle dragToDrawToggle;
    public Slider spacingSlider;

    //Settings
    private float spacingValue;
    private bool buldozeMode;
    private bool dragToDrawValue;

    //Drag to Position Mode
    public GameObject curHoldingObject;

    public Transform mouseSphere;

    void Start()
    {
        //Configure dropdown - map object selection
        List<Dropdown.OptionData> OS_data = new List<Dropdown.OptionData>();
        for (int i = 0; i < GlobalList.mapObjects.Count; i++)
        {
            OS_data.Add(new Dropdown.OptionData(GlobalList.mapObjects.ElementAt(i).Value.objectName));
        }
        objectSelection.options = OS_data;

        //Update
        onObjectSelectionChange();
        onDragToDrawChange();
        onBuldozeModeChange();
        onSpacingValueChange();
    }

    void Update()
    {
        raycastHit();
    }

    void raycastHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hit = new RaycastHit();

        //Raycasts
        if (!IsPointerOverUIElement())
        {
            if (!buldozeMode)
            {
                if (Physics.Raycast(ray, out hit, 300, 1 << 13))
                {
                    mouseSphere.gameObject.SetActive(true);
                    mouseSphere.transform.position = hit.point;

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (dragToDrawValue)
                        {
                            addObject(hit.point);
                            lastPlacedTreePos = hit.point;
                        }
                        else
                        {
                            addObject(hit.point);
                        }
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        if (dragToDrawValue)
                        {
                            if (Vector3.Distance(hit.point, lastPlacedTreePos) > spacingValue)
                            {
                                addObject(hit.point);
                                lastPlacedTreePos = hit.point;
                            }
                        }
                        else
                        {
                            curHoldingObject.transform.position = hit.point;
                        }
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        if (!dragToDrawValue)
                        {
                            curHoldingObject = null;
                        }
                    }
                } else
                {
                    mouseSphere.gameObject.SetActive(false);
                }
            }
            else
            {
                if (Physics.Raycast(ray, out hit, 300, 1 << 12))
                {
                    mouseSphere.gameObject.SetActive(true);
                    mouseSphere.transform.position = hit.point;

                    if (Input.GetMouseButtonDown(0))
                    {
                        LevelData.mapObjects.Remove(hit.collider.gameObject.GetComponent<MapObject>());
                        Destroy(hit.collider.gameObject);
                    }
                } else
                {
                    mouseSphere.gameObject.SetActive(false);
                }
            }
        }
    }

    public void addObject(Vector3 position)
    {
        addObject(position, objectSelection.value);
    }

    public void addObject(Vector3 position, int objectIndex)
    {
        MapObject mapObject = GlobalList.mapObjects.ElementAt(objectIndex).Value;

        curHoldingObject = Instantiate(mapObject.gameObject);
        LevelData.mapObjects.Add(curHoldingObject.GetComponent<MapObject>());

        curHoldingObject.transform.position = position;
    }

    public void onObjectSelectionChange()
    {
        //Change visuals
        MapObject selectedMo = GlobalList.mapObjects.ElementAt(objectSelection.value).Value;

        objectIcon.texture = selectedMo.icon;
        objectName.text = selectedMo.objectName;
    }

    public void onDragToDrawChange()
    {
        dragToDrawValue = dragToDrawToggle.isOn;
    }

    public void onBuldozeModeChange()
    {
        buldozeMode = buldozeToggle.isOn;
    }

    public void onSpacingValueChange()
    {
        spacingValue = spacingSlider.value;
    }

    //Implemented
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
}
