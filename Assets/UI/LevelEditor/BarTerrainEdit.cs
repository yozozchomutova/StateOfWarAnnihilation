using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections.Generic;
using System;

public class BarTerrainEdit : MonoBehaviour
{
    private bool editing = false;

    //UI
    public Button btnEditStart;
    public Button btnEditEnd;
    public GameObject terrainEditToolsPanel;

    public SC_TerrainEditor scTerrainEditor;

    public GameObject water;

    //References
    [Header("References")]
    public EditorManager editorManager;
    public Slider brushSize;
    public Slider brushStrength;
    public Slider brushHeight;
    public Dropdown deformMode;

    [Header("32x32 Grid")]
    public GameObject targetMesh;
    private Mesh gridMesh;
    private Vector3[] gridVerts;
    private int[] gridTris;

    private void Start()
    {
        gridMesh = new Mesh();
        targetMesh.GetComponent<MeshFilter>().mesh = gridMesh;
    }

    public void OnEditStart()
    {
        OnEditStateChange(true);
    }

    public void OnEditEnd()
    {
        OnEditStateChange(false);

        //Regenrate grid
        RegenerateGrid();

        //Generate NavMesh
        scTerrainEditor.terrain.GetComponent<NavMeshSurface>().BuildNavMesh();

        //Repaint terrain
        //Change to grass

        //Hills
        /*TerrainData tData = scTerrainEditor.terrain.terrainData;
        float[,] tHeights = tData.GetHeights(0, 0, tData.heightmapResolution, tData.heightmapResolution);
        int width = tData.alphamapWidth;
        int height = tData.alphamapWidth;

        float[,,] alphamap = tData.GetAlphamaps(0, 0, tData.alphamapResolution, tData.alphamapResolution);

        for (int x = 1; x < width-2; x++)
        {
            for (int y = 1; y < height-2; y++)
            {
                if (tHeights[x - 1, y] - tHeights[x, y] < -0.0075f)
                {
                    alphamap[x, y, 1] = 1f;
                }
                if (tHeights[x + 1, y] - tHeights[x, y] < -0.0075f)
                {
                    alphamap[x, y, 1] = 1f;
                }
                if (tHeights[x, y - 1] - tHeights[x, y] < -0.0075f)
                {
                    alphamap[x, y, 1] = 1f;
                }
                if (tHeights[x, y + 1] - tHeights[x, y] < -0.0075f)
                {
                    alphamap[x, y, 1] = 1f;
                }
            }
        }

        tData.SetAlphamaps(0, 0, alphamap);*/
    }

    public void OnEditStateChange(bool editing_)
    {
        this.editing = editing_;

        scTerrainEditor.editingTerrain = editing;

        btnEditStart.interactable = !editing;
        btnEditEnd.interactable = editing;
        terrainEditToolsPanel.SetActive(editing);
    }

    public void OnBrushSettingsChange()
    {
        float offsetArea = brushSize.value % 2;
        scTerrainEditor.area = brushSize.value - offsetArea;
        scTerrainEditor.strength = brushStrength.value;
        scTerrainEditor.height = brushHeight.value;
        scTerrainEditor.brushScaling();
    }

    public void OnDeformModeChange()
    {
        if (deformMode.value == 0)
        {
            scTerrainEditor.deformMode = SC_TerrainEditor.DeformMode.RaiseLower;
        } else if (deformMode.value == 1)
        {
            scTerrainEditor.deformMode = SC_TerrainEditor.DeformMode.FlattenByValue;
        } else if (deformMode.value == 2)
        {
            scTerrainEditor.deformMode = SC_TerrainEditor.DeformMode.FlattenByTerrain;
        }
        else if (deformMode.value == 3)
        {
            scTerrainEditor.deformMode = SC_TerrainEditor.DeformMode.Smooth;
        }
    }

    public void RegenerateGrid()
    {
        //Initialization
        TerrainData td = LevelData.mainTerrain.terrainData;
        int heightmapSize = td.heightmapResolution;
        int halfMapSize = 256;
        int quarterMapSize = 128;

        gridVerts = new Vector3[halfMapSize * halfMapSize];
        Vector2[] uvs = new Vector2[gridVerts.Length];
        List<int> gridTrisList = new List<int>();

        //Gather vertices
        //print("V: " + td.size.x + " Z: " + td.size.z + " H: " + halfMapSize);
        for (int y = 0; y < halfMapSize; y++)
        {
            for (int x = 0; x < halfMapSize; x++)
            {
                float nx = (float)x / (float)halfMapSize; //Normalized X
                float ny = (float)y / (float)halfMapSize; //Normalized Y

                gridVerts[x + y * halfMapSize] = new Vector3((nx / 2f + 0.25f) * td.size.x, td.GetHeight(x + quarterMapSize, y + quarterMapSize), (ny / 2f + 0.25f) * td.size.z);
                uvs[x + y * halfMapSize] = new Vector2(x, y);
            }
        }

        //Link triangles
        for (int y = 0; y < halfMapSize - 1; y++)
        {
            for (int x = 0; x < halfMapSize - 1; x++)
            {
                int vert1 = (x) + (y) * halfMapSize;
                int vert2 = (x) + (y + 1) * halfMapSize;
                int vert3 = (x + 1) + (y + 1) * halfMapSize;

                gridTrisList.Add(vert1);
                gridTrisList.Add(vert2);
                gridTrisList.Add(vert3);

               // uvs[vert1] = new Vector2(0, 0);
                //uvs[vert2] = new Vector2(0, halfMapSize);
                //uvs[vert3] = new Vector2(halfMapSize, halfMapSize);

                gridTrisList.Add((x) + (y) * halfMapSize);
                gridTrisList.Add((x + 1) + (y + 1) * halfMapSize);
                gridTrisList.Add((x + 1) + (y) * halfMapSize);
            }
        }

        //Copy list to array
        gridTris = new int[gridTrisList.Count];
        gridTrisList.CopyTo(gridTris);

        //Update grid mesh
        //gridMesh.Clear();
        gridMesh.vertices = gridVerts;
        gridMesh.triangles = gridTris;
        gridMesh.uv = uvs;
        gridMesh.RecalculateBounds();
    }
}
