using UnityEngine;

public class EditorControll : MonoBehaviour
{
    public Transform editorCamera;

    private float lastMouseX;
    private float lastMouseY;
    private bool rightButtonHeldDown;

    private float camRotX = 0;
    private float camRotY = 0;

    public float normalSpeed = 0.8f;
    public float chargedSpeed = 3f;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        //Camera speed
        float curSpeed = normalSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            curSpeed = chargedSpeed;
        }

        //Moving camera
        if (Input.GetKey(KeyCode.W))
        {
            editorCamera.position += editorCamera.forward * curSpeed;
        } if (Input.GetKey(KeyCode.S))
        {
            editorCamera.position -= editorCamera.forward * curSpeed;
        } if (Input.GetKey(KeyCode.A))
        {
            editorCamera.position -= editorCamera.right * curSpeed;
        } if (Input.GetKey(KeyCode.D))
        {
            editorCamera.position += editorCamera.right * curSpeed;
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

                editorCamera.localRotation = Quaternion.Euler(camRotX, camRotY, 0);
                //editorCamera.localRotation = Quaternion.Euler(differenceY / 3f, differenceX / 3f, 0);
                //editorCamera.localRotation = Quaternion.Euler(editorCamera.localRotation.x, editorCamera.localRotation.y, 0);
            }

            lastMouseX = curMouseX;
            lastMouseY = curMouseY;

            rightButtonHeldDown = true;
        } else
        {
            rightButtonHeldDown = false;
        }
    }
}