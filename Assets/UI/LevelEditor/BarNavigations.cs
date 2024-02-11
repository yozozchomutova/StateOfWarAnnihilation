using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BarNavigations : MonoBehaviour
{
    public Dropdown groundAngleDropdown;
    public Slider groundOffsetZSlider;
    public Toggle showGround;

    public GameObject targetMesh;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    public Terrain terrain;
    public Transform terrainNavMeshTransform;
    private NavMeshSurface navTerrain;

    public static int[] availableAgents =
    {
        -334000983,
        1479372276,
        -902729914,
        287145453,
        658490984,
        65107623
    };

    void Start()
    {
        mesh = new Mesh();

        targetMesh.GetComponent<MeshFilter>().mesh = mesh;

        groundAngleChange();
    }

    void Update()
    {
        
    }

    private void OnEnable()
    {
        groundAngleDropdown.value = LevelData.navigations_agentTypeID;
    }

    public void groundAngleChange()
    {
        checkTerrainNavMeshNull();

        navTerrain.agentTypeID = availableAgents[groundAngleDropdown.value];
        LevelData.navigations_agentTypeID = groundAngleDropdown.value;
    }

    public void groundOffsetZChange()
    {
        terrainNavMeshTransform.position = new Vector3(0, 0.1f + groundOffsetZSlider.value, 0);
    }

    public void showGroundToggle()
    {
        if (showGround.isOn)
        {
            navTerrain.BuildNavMesh();
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            recreateShape(triangulation);
            terrainNavMeshTransform.gameObject.SetActive(true);
        }
        else
            terrainNavMeshTransform.gameObject.SetActive(false);
    }

    public void recreateShape(NavMeshTriangulation triangulation)
    {
        TerrainData tData = terrain.terrainData;
        vertices = triangulation.vertices;

        //Adjust vertice heights
        /*for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertice = vertices[i];
            float heightY = terrain.SampleHeight(new Vector3(vertice.x, 0, vertice.z));

            vertice = new Vector3(vertice.x, heightY, vertice.z);
            vertices[i] = vertice;
        }*/

        triangles = new int[triangulation.indices.Length];

        for (int i = 0; i < triangulation.indices.Length; i += 3)
        {
            //var triangleIndex = i / 3;
            var i1 = triangulation.indices[i];
            var i2 = triangulation.indices[i + 1];
            var i3 = triangulation.indices[i + 2];
            /*var p1 = triangulation.vertices[i1];
            var p2 = triangulation.vertices[i2];
            var p3 = triangulation.vertices[i3];
            var areaIndex = triangulation.areas[triangleIndex];*/

            triangles[i] = i1;
            triangles[i + 1] = i2;
            triangles[i + 2] = i3;
        }

        UpdateMesh();
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    public void clearShape()
    {
        mesh.Clear();
    }

    private void checkTerrainNavMeshNull()
    {
        if (navTerrain == null)
        {
            navTerrain = terrain.GetComponent<NavMeshSurface>();
            navTerrain.BuildNavMesh();
        }
    }
}
