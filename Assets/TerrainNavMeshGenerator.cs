using UnityEngine;
using UnityEngine.AI;

public class TerrainNavMeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public Terrain terrain;

    void Start()
    {
        mesh = new Mesh();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void CreateShape(NavMeshTriangulation triangulation)
    {
        TerrainData tData = terrain.terrainData;
        vertices = triangulation.vertices;

        //Adjust vertice heights
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertice = vertices[i];
            float heightY = terrain.SampleHeight(new Vector3(vertice.x, 0, vertice.z));

            vertice = new Vector3(vertice.x, heightY, vertice.z);
            vertices[i] = vertice;
        } 

        triangles = new int[triangulation.indices.Length];

        for (int i = 0; i < triangulation.indices.Length; i += 3)
        {
            var triangleIndex = i / 3;
            var i1 = triangulation.indices[i];
            var i2 = triangulation.indices[i + 1];
            var i3 = triangulation.indices[i + 2];
            var p1 = triangulation.vertices[i1];
            var p2 = triangulation.vertices[i2];
            var p3 = triangulation.vertices[i3];
            var areaIndex = triangulation.areas[triangleIndex];

            triangles[i] = i1;
            triangles[i+1] = i2;
            triangles[i+2] = i3;

            /*if (areaIndex == 0)
            {

            }*/

            /*switch (areaIndex)
            {
                case 0:
                    color = Color.blue; break;
                case 1:
                    color = Color.blue; break;
                default:
                    color = Color.blue; break;
            }*/

        }

        UpdateMesh();
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    public void Clear()
    {
        mesh.Clear();
    }
}
