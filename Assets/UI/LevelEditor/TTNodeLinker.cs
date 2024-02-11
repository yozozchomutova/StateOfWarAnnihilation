using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTNodeLinker : MonoBehaviour
{
    /*public TTNode nodeFrom;
    public int outputId;

    public TTNode nodeTo;
    public int inputId;*/

    public MeshCollider meshCollider; //attached to the lineRenderer gameobject or child
    private Mesh mesh;
    private Camera uiCamera;

    private LineRenderer lr;
    private float updateColliderCooldown = 1f;
    private float ucCooldown;

    public TTNode nodeFrom;
    public TTNode nodeTo;

    public int outputId;
    public int inputId;

    [HideInInspector] public bool shouldExist = true;

    //Generated
    private GameObject followGO1;
    private GameObject followGO2;

    private void Start()
    {
        ucCooldown = updateColliderCooldown;

        mesh = new Mesh();
        lr = gameObject.GetComponent<LineRenderer>();
        uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
    }

    public void crateLink(TTNode nodeFrom, int outputId, TTNode nodeTo, int inputId)
    {
        this.nodeFrom = nodeFrom;
        this.nodeTo = nodeTo;

        this.outputId = outputId;
        this.inputId = inputId;

        print(nodeTo.nodeName + " INPUT ID: " + inputId);

        followGO1 = nodeFrom.outputBtns[outputId].gameObject;
        followGO2 = nodeTo.inputBtns[inputId].gameObject;
    }

    public bool checkValidation() //Link still exists?
    {
        if (!shouldExist)
            return false;

        if (nodeFrom == null || nodeTo == null)
            return false;

        for (int i = 0; i < nodeFrom.inputNodes.Length; i++)
        {
            if (nodeFrom.inputNodes[i] == nodeTo)
            {
                //print("F: " + nodeFrom.nodeName + " T: " + nodeTo.nodeName + " i: " + i );
                return true;
            }
        }

        return false;
    }

    void Update()
    {
        if (followGO1 == null || followGO2 == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos1 = followGO1.GetComponent<RectTransform>().position;
        Vector3 pos2 = followGO2.GetComponent<RectTransform>().position;
        lr.SetPosition(0, new Vector3(pos1.x, pos1.y, 5));
        lr.SetPosition(1, new Vector3(pos2.x, pos2.y, 5));
        //ec.points[0] =  new Vector2(pos2.x, pos2.z);
        //ec.points[1] = new Vector3(pos2.x, pos2.z);

        //Collider
        /*ucCooldown -= Time.deltaTime;
        if (ucCooldown <= 0)
        {
            ucCooldown = updateColliderCooldown;
            setLrCollider();
        }*/

        if (Input.GetMouseButtonDown(1))
        {
            setLrCollider();
        }
    }

    void setLrCollider()
    {
        /*lr.BakeMesh(mesh, uiCamera, false);
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 oldV = mesh.vertices[i];
            print("Old: " + oldV);
            mesh.vertices[i] = uiCamera.WorldToScreenPoint(oldV);
            print("New: " + mesh.vertices[i]);
        }*/

        /*mesh.Clear();
        Vector3[] vector3s = new Vector3[4]
        {
            lr.GetPosition(0) + new Vector3(0, 0.5f, 0),
            lr.GetPosition(0) - new Vector3(0, 0.5f, 0),
            lr.GetPosition(1) + new Vector3(0, 0.5f, 0),
            lr.GetPosition(1) - new Vector3(0, 0.5f, 0)
        };

        int[] triangles = new int[6]
        {
            0, 1, 2, 0, 3, 2
        };

        mesh.SetVertices(vector3s);
        mesh.SetTriangles(triangles, 0);

        meshCollider.sharedMesh = mesh;*/

        Vector3[] positions = new Vector3[lr.positionCount];
        lr.GetPositions(positions);

        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 start = positions[i];
            Vector3 end = positions[i + 1];

            if (IsMouseCloseToLine(start, end))
            {
                print("Link removed!");
                nodeFrom.inputNodes[outputId] = null;
                shouldExist = false;
                break;
            }
        }
    }

    private bool IsMouseCloseToLine(Vector3 start, Vector3 end)
    {
        // Calculate the distance from the mouse position to the line
        // and check if it is close enough to consider it a click on the line
        // You can adjust the threshold value as needed
        float threshold = 7f;
        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPosStart = uiCamera.WorldToScreenPoint(start);
        Vector3 screenPosEnd = uiCamera.WorldToScreenPoint(end);
        float distance = DistancePointLine(mousePos, screenPosStart, screenPosEnd);
        print("D: " + Mathf.Abs(distance));

        return Mathf.Abs(distance) < threshold;
    }

    private float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        // Calculate the dot product
        float dotProduct = Vector3.Dot((point - lineStart), (lineEnd - lineStart)) / ((lineEnd - lineStart).magnitude * (lineEnd - lineStart).magnitude);

        // Clamp the dot product to the line segment
        dotProduct = Mathf.Clamp(dotProduct, 0, 1);

        // Calculate the closest point on the line segment
        Vector3 closestPoint = lineStart + dotProduct * (lineEnd - lineStart);
        //print("p: " + point + " |S: " + lineStart + " |E: " + lineEnd + " |LN: " + closestPoint);

        // Calculate the distance between the point and the line segment
        float distance = (point - closestPoint).magnitude;
        return distance;
    }
}
