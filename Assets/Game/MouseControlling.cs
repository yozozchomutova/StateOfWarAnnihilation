using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MouseControlling : MonoBehaviour
{
    public enum SelectedUnitsType
    {
        NONE, UNITS, PVT
    }

    public enum SelectedActionType
    {
        NONE, MOVE, ATTACK, REPAIR, UPGRADE
    }

    public enum CursorType
    {
        NORMAL1, ATTACK, UPGRADE, REPAIR, SENDAIRFORCES, FREECAM, SELECT
    }

    public static readonly Vector3 zNullate = new Vector3(1, 1, 0);

    private Camera mainCamera;

    public float moveSpeed = 15;
    public float turnSpeed = 3;

    bool moveFast = false;
    float rotationY;

    public Transform cameraPivot;

    public LevelManager levelManager;

    //camera rot
    private Vector3 firstMouseCamPos = Vector3.zero;
    private float firstMouseX0;
    private float firstMouseY0;
    private float firstMouseX1;
    private float firstMouseY1;
    private float lastMouseX0;
    private float lastMouseY0;
    private float lastMouseX1;
    private float lastMouseY1;

    [HideInInspector] public bool rightClick;
    [HideInInspector] public bool leftClick;
    [HideInInspector] public Vector2 lastDrag = Vector2.zero;

    private float camRotX = 55;
    private float camRotY = 45;

    private float cameraZoom = -17;
    private float targetedZoom = -17;

    //Selection
    private SelectedUnitsType selectedUnitsType = SelectedUnitsType.NONE;

    public Image selectionArea;
    public float selectionAreaMultiplierX;
    public float selectionAreaMultiplierY;
    public float selectionCameraErrMultiply;
    public float selectionCameraErrAdd;
    private RectTransform selectionAreaRect;
    private Vector3 selectionAreaOffset;

    //[HideInInspector] public Building selectedBuilding;
    public ArrayList selectedGroundUnits = new ArrayList();

    public Transform unitSelector;

    //Cursor
    [HideInInspector] public CursorType cursorType = CursorType.NORMAL1;
    private CursorType currentCursorType = CursorType.NORMAL1;

    //
    public Transform waterLevel;

    public Volume ppNormal;
    public Volume ppUnderwater;

    // Use this for initialization
    void Start()
    {
        mainCamera = gameObject.GetComponent<Camera>();

        rotationY = -transform.localEulerAngles.x;
        selectionAreaRect = selectionArea.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        cameraPivot.localRotation = Quaternion.Euler(camRotX, camRotY, 0);

        UpdateCursorTexture();
    }

    private void UpdateCursorTexture()
    {
        //Did mouse state changed?
        if (cursorType != currentCursorType)
        {
            if (cursorType == CursorType.NORMAL1)
            {
                Cursor.SetCursor(GlobalList.mouseSpriteNormal, Vector2.zero, CursorMode.Auto);
            } else if (cursorType == CursorType.ATTACK)
            {
                Cursor.SetCursor(GlobalList.mouseSpriteAttack, Vector2.zero, CursorMode.Auto);
            } else if (cursorType == CursorType.UPGRADE)
            {
                Cursor.SetCursor(GlobalList.mouseSpriteUpgrade, Vector2.zero, CursorMode.Auto);
            } else if (cursorType == CursorType.REPAIR)
            {
                Cursor.SetCursor(GlobalList.mouseSpriteRepair, Vector2.zero, CursorMode.Auto);
            } else if (cursorType == CursorType.SENDAIRFORCES)
            {
                Cursor.SetCursor(GlobalList.mouseSpriteSendAirforce, Vector2.zero, CursorMode.Auto);
            } else if (cursorType == CursorType.FREECAM)
            {
                Cursor.SetCursor(GlobalList.mouseSpriteFreeCam, Vector2.zero, CursorMode.Auto);
            } else if (cursorType == CursorType.SELECT)
            {
                Cursor.SetCursor(GlobalList.mouseSpriteSelect, Vector2.zero, CursorMode.Auto);
            }

            currentCursorType = cursorType;
        }

        cursorType = CursorType.NORMAL1; //Reset
    }

    void Movement()
    {
        moveFast = Input.GetKey(KeyCode.LeftShift);

        float speed = moveSpeed * Time.unscaledDeltaTime * (moveFast ? 3 : 1);

        if (Input.GetKeyDown(KeyCode.Space) && !GameLevelUIController.lockCameraMovement)
        {
            cameraPivot.transform.position = GameLevelUIController.staticMsgPosition;
        }

        if (!GameLevelUIController.lockCameraMovement)
        {
            if (!mainCamera.orthographic)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    cameraPivot.position += new Vector3(transform.forward.x, 0, transform.forward.z).normalized * speed;
                    //cameraPivot.root.Translate(new Vector3(transform.forward.x, 0, transform.forward.z).normalized * speed, Space.World);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    cameraPivot.root.Translate(-new Vector3(transform.forward.x, 0, transform.forward.z).normalized * speed, Space.World);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    cameraPivot.root.Translate(-new Vector3(transform.right.x, 0, transform.right.z).normalized * speed, Space.World);
                }
                if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.D))
                {
                    cameraPivot.root.Translate(new Vector3(transform.right.x, 0, transform.right.z).normalized * speed, Space.World);
                }
                if (Input.GetKey(KeyCode.Q))
                {
                    transform.root.Translate(Vector3.up * speed, Space.World);
                }
                if (Input.GetKey(KeyCode.E))
                {
                    transform.root.Translate(Vector3.down * speed, Space.World);
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.W))
                {
                    selectionAreaOffset.y += speed;
                    //firstMouseY0 -= 500 * Time.deltaTime;
                    cameraPivot.root.Translate(new Vector3(cameraPivot.forward.x, 0, cameraPivot.forward.z) * speed, Space.World);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    selectionAreaOffset.y -= speed;
                    //firstMouseY0 += 500 * Time.deltaTime;
                    cameraPivot.root.Translate(new Vector3(-cameraPivot.forward.x, 0, -cameraPivot.forward.z) * speed, Space.World);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    selectionAreaOffset.x -= speed;
                    //firstMouseX0 += 1000 * Time.deltaTime;
                    cameraPivot.root.Translate(new Vector3(-cameraPivot.right.x, 0, -cameraPivot.right.z) * speed, Space.World);
                }
                if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.D))
                {
                    selectionAreaOffset.x += speed;
                    //firstMouseX0 -= 1000 * Time.deltaTime;
                    cameraPivot.root.Translate(new Vector3(cameraPivot.right.x, 0, cameraPivot.right.z) * speed, Space.World);
                }
            }
        }

        //Border camera movement
        cameraPivot.localPosition = new Vector3(
            Mathf.Clamp(cameraPivot.localPosition.x, 0, LevelData.mainTerrain.terrainData.size.x),
            cameraPivot.localPosition.y,
            Mathf.Clamp(cameraPivot.localPosition.z, 0, LevelData.mainTerrain.terrainData.size.z)
            );

        //Decide post-processing
        if (transform.position.y < waterLevel.position.y)
            ppUnderwater.enabled = true;
        else
            ppUnderwater.enabled = false;

        //Zoom camera
        if (Input.GetAxis("Mouse ScrollWheel") != 0f) 
        {
            float multiplier = mainCamera.orthographic ? 83f : 12f;
            cameraZoom += Input.GetAxis("Mouse ScrollWheel") * multiplier;
            cameraZoom = Mathf.Clamp(cameraZoom, -50f, 0f);
        }

        if (mainCamera.orthographic)
        {
            float calculatedSize = -(cameraZoom / 4f) + 3;
            mainCamera.orthographicSize += (calculatedSize - mainCamera.orthographicSize) * Time.deltaTime * 11f; //Smooth motion
        }
        else
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + ((cameraZoom - transform.localPosition.z) * Time.deltaTime * 11f));

        //Rotating camera
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            rightClick = true;

            lastMouseX1 = Input.mousePosition.x;
            lastMouseY1 = Input.mousePosition.y;

            firstMouseX1 = lastMouseX1;
            firstMouseY1 = lastMouseY1;
        }
        else if (Input.GetMouseButton(1) && !mainCamera.orthographic) //No rotation for ortographic!
        {
            cursorType = CursorType.FREECAM;

            float curMouseX1 = Input.mousePosition.x;
            float curMouseY1 = Input.mousePosition.y;

            if (rightClick && curMouseX1 != lastMouseX1 && curMouseY1 != lastMouseY1)
            {
                //Click -> Drag
                rightClick = false;
            } else
            {
                float differenceX = curMouseX1 - lastMouseX1;
                float differenceY = curMouseY1 - lastMouseY1;

                camRotX -= differenceY / 6f;
                camRotY += differenceX / 6f;

                //Clamp
                camRotX = Mathf.Clamp(camRotX, 0, 90);
            }

            lastMouseX1 = curMouseX1;
            lastMouseY1 = curMouseY1;
        }
        else if (Input.GetMouseButton(2) && mainCamera.orthographic) //Special rotation for ortographic
        {
            cursorType = CursorType.FREECAM;

            float curMouseX1 = Input.mousePosition.x;
            float curMouseY1 = Input.mousePosition.y;

            float differenceX = curMouseX1 - lastMouseX1;
            float differenceY = curMouseY1 - lastMouseY1;

            //camRotX -= differenceY / 6f;
            camRotY += differenceX / 6f;

            //Clamp
            camRotX = Mathf.Clamp(camRotX, 0, 90);

            lastMouseX1 = curMouseX1;
            lastMouseY1 = curMouseY1;
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (rightClick)
            {
                rightClick = false;
            }
        }

        //Mouse events
        if (Input.GetMouseButtonDown(0))
        {
            leftClick = true;

            lastMouseX0 = Input.mousePosition.x;
            lastMouseY0 = Input.mousePosition.y;

            firstMouseX0 = lastMouseX0;
            firstMouseY0 = lastMouseY0;
            firstMouseCamPos = cameraPivot.position;

            // Get the position of the mouse on the screen
            /*Ray ray = mainCamera.ScreenPointToRay(new Vector3(lastMouseX0, lastMouseY0));
            selectionAreaOffset = ray.GetPoint(50f);*/

            selectionAreaOffset = mainCamera.ScreenToWorldPoint(new Vector3(lastMouseX0, lastMouseY0, mainCamera.nearClipPlane));
            selectionAreaOffset += (mainCamera.transform.forward * 20);

        } else if (Input.GetMouseButton(0))
        {
            float curMouseX0 = Input.mousePosition.x;
            float curMouseY0 = Input.mousePosition.y;

            if (leftClick && curMouseX0 != lastMouseX0 && curMouseY0 != lastMouseY0)
            {
                //Click -> Drag
                leftClick = false;

                selectionAreaRect.sizeDelta = new Vector2(0.1f, 0.1f);
                selectionArea.gameObject.SetActive(true);
            } else
            {
                /*float normalizedX = Screen.currentResolution.width / 1920f;
                float normalizedY = Screen.currentResolution.height / 1920f;
                float areaRectX = firstMouseX0 + selectionAreaOffset.x * normalizedX * selectionAreaMultiplierX * (selectionCameraErrMultiply / (cameraZoom+selectionCameraErrAdd));
                float areaRectY = firstMouseY0 + selectionAreaOffset.y * normalizedY * selectionAreaMultiplierY * (selectionCameraErrMultiply / (cameraZoom+ selectionCameraErrAdd));*/

                // Vector3 worldStartPoint = mainCamera.ScreenToWorldPoint(new Vector2(firstMouseX0, firstMouseY0));

                // Get the position of the mouse on the screen
                Vector3 screenPos = mainCamera.WorldToScreenPoint(selectionAreaOffset);
                /*print(new Vector3(firstMouseX0, firstMouseY0, 0));
                print(selectionAreaOffset);
                print(screenPos);*/

                float areaRectX = screenPos.x;
                float areaRectY = screenPos.y;


                float widthX = lastMouseX0 - areaRectX;
                float widthY = lastMouseY0 - areaRectY;
                selectionAreaRect.sizeDelta = new Vector2(Mathf.Max(widthX, -widthX), Mathf.Max(widthY, -widthY));
                selectionAreaRect.position = new Vector2(areaRectX, areaRectY);

                //Flipping
                int pivotX = curMouseX0 < firstMouseX0 ? 1 : 0;
                int pivotY = curMouseY0 < firstMouseY0 ? 1 : 0;

                selectionAreaRect.pivot = new Vector2(pivotX, pivotY);

                //Record drag
                lastDrag = new Vector2(curMouseX0 - lastMouseX0, curMouseY0 - lastMouseY0);
            }

            lastMouseX0 = curMouseX0;
            lastMouseY0 = curMouseY0;
        }

        if (Input.GetMouseButtonUp(0))
        {
            lastDrag = Vector2.zero;
            if (leftClick)
            {
                leftClick = false;
            } else
            {
                selectionArea.gameObject.SetActive(false);

                //Select units in area
                Rect selectRectBox = new Rect(
                    Mathf.Min(firstMouseX0, lastMouseX0),
                    Mathf.Min(firstMouseY0, lastMouseY0),
                    Mathf.Max(firstMouseX0, lastMouseX0) - Mathf.Min(firstMouseX0, lastMouseX0),
                    Mathf.Max(firstMouseY0, lastMouseY0) - Mathf.Min(firstMouseY0, lastMouseY0));

                foreach(Unit unit in LevelData.units)
                {
                    bool multiselectionAllowed =
                        unit.unitType == Unit.UnitType.UNIT ||
                        unit.unitType == Unit.UnitType.STEPPER;

                    if (multiselectionAllowed && unit.team.id == LevelData.ts.teamId && selectRectBox.Contains(mainCamera.WorldToScreenPoint(unit.transform.position)))
                    {
                        levelManager.selectUnit(unit);
                    }
                }
            }
        }
    }
}
