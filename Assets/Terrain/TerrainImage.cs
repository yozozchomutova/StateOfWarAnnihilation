using UnityEngine;

public class TerrainImage : MonoBehaviour
{
    public enum TransformMethod
    {
        XYZ,
        XZY
    }

    public enum Mode
    {
        TERRAIN,
        FLAT,
        NONE
    }

    public Terrain t;
    public MeshFilter meshF;
    public MeshRenderer meshR;
    [HideInInspector] public Mesh mesh;
    private Vector3[] vertices;

    public TransformMethod transformMethod;
    public Mode mode;

    public Vector2 positionMultiplier;
    public float heightOffset;

    public bool makeStatic;
    private bool isStatic;

    private float offsetY = 0f;

    void Start()
    {
        mesh = meshF.mesh;
        vertices = mesh.vertices;

        if (t == null)
        {
            t = FindObjectOfType<Terrain>();
        }

        offsetY = transform.position.y;
    }

    void Update()
    {
        if (!isStatic)
        {
            offsetY = transform.position.y;

            float objectScale = transform.localScale.x;

            //Align vertices to terrain
            if (mode == Mode.TERRAIN)
            {
                if (transformMethod == TransformMethod.XYZ)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        float newY = t.SampleHeight(gameObject.transform.position + new Vector3(vertices[i].x * objectScale, vertices[i].y, vertices[i].z * objectScale));
                        newY += heightOffset;
                        vertices[i] = new Vector3(vertices[i].x, newY - offsetY, vertices[i].z);
                    }
                }
                else if (transformMethod == TransformMethod.XZY)
                {
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        float newY = t.SampleHeight(gameObject.transform.position + new Vector3(vertices[i].x * objectScale, vertices[i].z, vertices[i].y * objectScale));
                        newY += heightOffset;
                        vertices[i] = new Vector3(vertices[i].x, vertices[i].y, -(newY - offsetY));
                    }
                }
            } else if (mode == Mode.FLAT)
            {
                float newY = t.SampleHeight(gameObject.transform.position);
                newY += heightOffset;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Vector3(vertices[i].x, newY - offsetY, vertices[i].z);
                }
            } else if (mode == Mode.NONE)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Vector3(vertices[i].x, 0, vertices[i].z);
                }
            }

            mesh.vertices = vertices;

            isStatic = makeStatic;
        }
    }
}
