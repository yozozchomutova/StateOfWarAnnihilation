using UnityEngine;

public class TerrainImage : MonoBehaviour
{
    public enum TransformMethod
    {
        XYZ,
        XZY
    }

    public Terrain t;
    public MeshFilter meshF;
    public MeshRenderer meshR;
    [HideInInspector] public Mesh mesh;
    private Vector3[] vertices;

    public TransformMethod transformMethod;

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
            //float objectY = t.SampleHeight(gameObject.transform.position) + 0.1f;

            //transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            //transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);

            float objectScale = transform.localScale.x;

            //Align vertices to terrain
            if (transformMethod == TransformMethod.XYZ)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    float newY = t.SampleHeight(gameObject.transform.position + new Vector3(vertices[i].x * objectScale, vertices[i].y, vertices[i].z * objectScale));
                    newY += heightOffset;
                    vertices[i] = new Vector3(vertices[i].x, newY - offsetY, vertices[i].z);
                }
            } else if (transformMethod == TransformMethod.XZY)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    float newY = t.SampleHeight(gameObject.transform.position + new Vector3(vertices[i].x * objectScale, vertices[i].z, vertices[i].y * objectScale));
                    newY += heightOffset;
                    vertices[i] = new Vector3(vertices[i].x, vertices[i].y, -(newY - offsetY));
                }
            }

            mesh.vertices = vertices;

            isStatic = makeStatic;
        }
    }
}
