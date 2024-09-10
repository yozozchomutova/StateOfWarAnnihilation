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

    //PostProcessing
    public Transform waterLevel;

    public Volume ppNormal;
    public Volume ppUnderwater;

    // Use this for initialization
    void Start()
    {
        rotationY = -transform.localEulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
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
                }

                lastMouseX = curMouseX;
                lastMouseY = curMouseY;

                rightButtonHeldDown = true;
                CursorManager.SetCursor(CursorManager.spriteFreeCam);
            }
            else
            {
                rightButtonHeldDown = false;
            }
        }

        //Decide post-processing
        ppUnderwater.enabled = transform.position.y < waterLevel.position.y;
    }
}