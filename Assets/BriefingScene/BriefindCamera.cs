using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BriefindCamera : MonoBehaviour
{
    public bool RMBheld;

    private float curProceedRotateTimer, proceedRotateTimer = 0.01f;

    float lastMouseX1, lastMouseY1;
    float camRotX, camRotY;
    Vector3 camRotVel = Vector3.zero;

    public Transform cameraPivot;

    //Zoom

    private float cameraZoom = -50, targetedZoom = -8;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        //Zoom camera
        if (Input.GetAxis("Mouse ScrollWheel") != 0f) // forward
        {
            targetedZoom += Input.GetAxis("Mouse ScrollWheel") * 15f;
            targetedZoom = Mathf.Clamp(targetedZoom, -50f, -4.5f);
        }

        cameraZoom += (targetedZoom - cameraZoom) / 25f;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, cameraZoom);

        //call proceedRotate
        curProceedRotateTimer -= Time.deltaTime;
        if (curProceedRotateTimer <= 0)
        {
            curProceedRotateTimer = proceedRotateTimer;
            proceedRotate();
        }
    }

    void proceedRotate()
    {
        if (Input.GetMouseButton(1) && !RMBheld)
        {
            RMBheld = true;
            lastMouseX1 = Input.mousePosition.x;
            lastMouseY1 = Input.mousePosition.y;

            camRotVel.x = 0f;
            camRotVel.y = 0f;
        } else if (!Input.GetMouseButton(1) && RMBheld)
        {
            RMBheld = false;
        }

        //Rotating camera
        if (RMBheld)
        {
            float curMouseX1 = Input.mousePosition.x;
            float curMouseY1 = Input.mousePosition.y;

            float differenceX = curMouseX1 - lastMouseX1;
            float differenceY = curMouseY1 - lastMouseY1;

            camRotVel.x = differenceY / 6f;
            camRotVel.y = differenceX / 6f;

            lastMouseX1 = curMouseX1;
            lastMouseY1 = curMouseY1;
        }

        //Apply rotating velocity
        camRotX -= camRotVel.x;
        camRotY += camRotVel.y;

        cameraPivot.localRotation = Quaternion.Euler(camRotX, camRotY, 0);

        camRotVel.x /= 1.1f;
        camRotVel.y /= 1.1f;
    }
}
