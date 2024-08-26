using UnityEngine;
using UnityEngine.Rendering;

public class SC_EditorFlyCamera : MonoBehaviour
{
    public float moveSpeed = 15;
    public float turnSpeed = 3;

    bool moveFast = false;
    float rotationY;

    //camera rot
    private float lastMouseX;
    private float lastMouseY;
    private bool rightButtonHeldDown;

    private float camRotX = 0;
    private float camRotY = 0;

    //Terrain edge
    public GameObject terrainEdge;
    public Material TE_mat;

    private double maxTE_alpha = 0.08627451f;
    private double minTE_alpha = 0.03627451f;
    private double curTE_alpha;
    private bool TE_alpha_rising = false;

    //PostProcessing
    public Transform waterLevel;

    public Volume ppNormal;
    public Volume ppUnderwater;

    // Use this for initialization
    void Start()
    {
        rotationY = -transform.localEulerAngles.x;

        //Starting properties for Terrain edge
        curTE_alpha = maxTE_alpha;
        TE_mat.color = new Color(1f, 0.962297f, 0f, (float)maxTE_alpha);
    }

    // Update is called once per frame
    void Update()
    {
        Movement();

        if (Input.GetKeyDown(KeyCode.H))
        {
            terrainEdge.SetActive(!terrainEdge.activeSelf);
        }

        //Terrain edge breathe effect
        if (curTE_alpha > maxTE_alpha)
        {
            TE_alpha_rising = false;
        } else if (curTE_alpha < minTE_alpha)
        {
            TE_alpha_rising = true;
        }

        curTE_alpha += TE_alpha_rising ? Time.deltaTime / 45 : -Time.deltaTime / 45;
        TE_mat.color = new Color(1f, 0.962297f, 0f, (float)curTE_alpha);
    }

    void Movement()
    {
        moveFast = Input.GetKey(KeyCode.LeftShift);

        float speed = moveSpeed * Time.deltaTime * (moveFast ? 3 : 1);

        if (!BarBuildings.IsPointerOverUIElement())
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.root.Translate(transform.forward * speed, Space.World);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.root.Translate(-transform.forward * speed, Space.World);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.root.Translate(-transform.right * speed, Space.World);
            }
            if (!Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.D))
            {
                transform.root.Translate(transform.right * speed, Space.World);
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.root.Translate(transform.up * speed, Space.World);
            }
            if (Input.GetKey(KeyCode.Q))
            {
                transform.root.Translate(-transform.up * speed, Space.World);
            }

            //Rotating camera
            if (Input.GetMouseButton(1))
            {
                float curMouseX = Input.mousePosition.x;
                float curMouseY = Input.mousePosition.y;

                if (rightButtonHeldDown)
                {
                    float differenceX = curMouseX - lastMouseX;
                    float differenceY = curMouseY - lastMouseY;

                    camRotX -= differenceY / 6f;
                    camRotY += differenceX / 6f;

                    transform.localRotation = Quaternion.Euler(camRotX, camRotY, 0);
                    //editorCamera.localRotation = Quaternion.Euler(differenceY / 3f, differenceX / 3f, 0);
                    //editorCamera.localRotation = Quaternion.Euler(editorCamera.localRotation.x, editorCamera.localRotation.y, 0);
                }

                lastMouseX = curMouseX;
                lastMouseY = curMouseY;

                rightButtonHeldDown = true;
            }
            else
            {
                rightButtonHeldDown = false;
            }
        }

        //Decide post-processing
        if (transform.position.y < waterLevel.position.y)
        {
            ppUnderwater.enabled = true;
        } else
        {
            ppUnderwater.enabled = false;
        }

        /*if (Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        freeLook = Input.GetMouseButton(1);
        if (Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (freeLook)
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * turnSpeed;

            rotationY += Input.GetAxis("Mouse Y") * turnSpeed;
            rotationY = Mathf.Clamp(rotationY, -90, 90);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }*/
    }
}