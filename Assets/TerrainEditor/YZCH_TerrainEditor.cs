using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class YZCH_TerrainEditor : MonoBehaviour
{
    public enum Mode
    {
        NONE,
        EDIT_TERRAIN,
        PAINT_TERRAIN,
        PAINT_DETAILS,
    }

    public enum TerrainEditMode
    {
        RAISE_LOWER,
        SMOOTH,
        FLATTEN_BY_HEIGHT,
        FLATTEN_BY_VALUE,
    }

    public EditorManager editorManager;

    public Transform brushImage;
    public Transform buildTarget;
    public Vector3 buildTargPos;
    public float buildTargetScaleMultiplier = 1f;

    private Terrain t;
    private TerrainData tData;
    [HideInInspector] public Mode mode;
    [HideInInspector] public TerrainEditMode editTerrainMode;

    private int xRes, yRes;
    private float[,] savedHeights;
    private float[,,] savedAlphamaps;

    private Texture2D deformTexture;
    private float[,] influencingPoints;
    public float strength = 1;
    private float area = 1;
    public float height = 1;
    private float lastHeight = 1;
    private float strengthSave;

    [Header("UI")]
    public Text pointingHeightTxt;

    //Mouse
    private float x, y;

    private int selectedTextureIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        t = FindObjectOfType<Terrain>();
        tData = t.terrainData;

        //Save original height data
        xRes = tData.heightmapResolution;
        yRes = tData.heightmapResolution;
        savedHeights = tData.GetHeights(0, 0, xRes, yRes);
        savedAlphamaps = tData.GetAlphamaps(0, 0, tData.alphamapResolution, tData.alphamapResolution);
    }

    private void heavyWorkTest(int alphamapRes, float[,,] fetchAlphamaps)
    {
        byte[][] data = new byte[alphamapRes * alphamapRes * fetchAlphamaps.GetLength(2)][];

        for (int i = 0; i < fetchAlphamaps.GetLength(2); i++)
        {
            for (int x = 0; x < fetchAlphamaps.GetLength(0); x++)
            {
                for (int y = 0; y < fetchAlphamaps.GetLength(1); y++)
                {
                    data[x * alphamapRes + y + (alphamapRes * alphamapRes * i)] = BitConverter.GetBytes(fetchAlphamaps[x, y, i]);
                }
            }
        }

        editorManager.createUndoAction(EditorManager.UndoType.TERRAIN_PAINT, data);
    }

    // Update is called once per frame
    void Update()
    {
        if (mode != Mode.NONE && !BarBuildings.IsPointerOverUIElement())
        {
            if (Input.GetMouseButtonDown(0))
            {
                buildTargPos = buildTarget.position - t.GetPosition();
                x = Mathf.Clamp01(buildTargPos.x / tData.size.x);
                y = Mathf.Clamp01(buildTargPos.z / tData.size.z);

                if (mode == Mode.PAINT_TERRAIN)
                {
                    int alphamapRes = tData.alphamapResolution;
                    float[,,] fetchAlphamaps = tData.GetAlphamaps(0, 0, alphamapRes, alphamapRes);

                    Thread backgroundThread = new Thread(() => heavyWorkTest(alphamapRes, fetchAlphamaps));
                    backgroundThread.Start();
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit, 300, 1 << 13))
            {
                buildTarget.position = hit.point;
                brushImage.position = new Vector3(hit.point.x, brushImage.position.y, hit.point.z);

                //Get height data
                lastHeight = hit.point.y / tData.size.y;
                pointingHeightTxt.text = string.Format("Pointing height : {0:0.00}", lastHeight);

                if (Input.GetMouseButton(0))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        strengthSave = strength;
                    }
                    else
                    {
                        strengthSave = -strength;
                    }

                    switch (mode)
                    {
                        case Mode.EDIT_TERRAIN:
                            int startX = (int)(Mathf.Max(hit.point.x - area, 0) / tData.size.x * tData.alphamapResolution);
                            int startY = (int)(Mathf.Max(hit.point.z - area, 0) / tData.size.z * tData.alphamapResolution);
                            int endX = (int)(Mathf.Min((hit.point.x + area) / tData.size.x * tData.alphamapResolution, tData.alphamapResolution));
                            int endY = (int)(Mathf.Min((hit.point.z + area) / tData.size.z * tData.alphamapResolution, tData.alphamapResolution));

                            float[,] heightmaps = tData.GetHeights(startX, startY, endX - startX, endY - startY);
                            for (int i = 0; i < heightmaps.GetLength(0); i++)
                            {
                                for (int j = 0; j < heightmaps.GetLength(1); j++)
                                {
                                    int influencePointX = (int)((float)i / heightmaps.GetLength(0) * influencingPoints.GetLength(0));
                                    int influencePointY = (int)((float)j / heightmaps.GetLength(1) * influencingPoints.GetLength(1));
                                    float influenceAmount = influencingPoints[influencePointX, influencePointY] * strength;
                                    if (editTerrainMode == TerrainEditMode.RAISE_LOWER)
                                    {
                                        heightmaps[i, j] -= influenceAmount * strengthSave / 6f * Time.deltaTime;
                                    }
                                    else if (editTerrainMode == TerrainEditMode.FLATTEN_BY_VALUE)
                                    {
                                        //areaT[i, j] = Mathf.Lerp(areaT[i, j], flattenTarget, craterData[i * newTex.width + j].a * strengthNormalized);
                                        if (influenceAmount > 0.5f)
                                            heightmaps[i, j] = height;
                                    }
                                    else if (editTerrainMode == TerrainEditMode.FLATTEN_BY_HEIGHT)
                                    {
                                        if (influenceAmount > 0.5f)
                                            heightmaps[i, j] = lastHeight;
                                    }
                                    else if (editTerrainMode == TerrainEditMode.SMOOTH)
                                    {
                                        if (i == 0 || i == heightmaps.GetLength(0) - 1 || j == 0 || j == heightmaps.GetLength(1) - 1)
                                            continue;

                                        float heightSum = 0;
                                        for (int ySub = -1; ySub <= 1; ySub++)
                                        {
                                            for (int xSub = -1; xSub <= 1; xSub++)
                                            {
                                                heightSum += heightmaps[i + ySub, j + xSub];
                                            }
                                        }

                                        heightmaps[i, j] = Mathf.Lerp(heightmaps[i, j], (heightSum / 9), influenceAmount * 4f * strength);
                                    }
                                }
                            }

                            tData.SetHeights(startX, startY, heightmaps);
                            break;
                        case Mode.PAINT_TERRAIN:
                            startX = (int)(Mathf.Max(hit.point.x - area, 0) / tData.size.x * tData.alphamapResolution);
                            startY = (int)(Mathf.Max(hit.point.z - area, 0) / tData.size.z * tData.alphamapResolution);
                            endX = (int)(Mathf.Min((hit.point.x + area) / tData.size.x * tData.alphamapResolution, tData.alphamapResolution));
                            endY = (int)(Mathf.Min((hit.point.z + area) / tData.size.z * tData.alphamapResolution, tData.alphamapResolution));

                            float[,,] alphaAreas = tData.GetAlphamaps(startX, startY, endX - startX, endY - startY);
                            for (int i = 0; i < alphaAreas.GetLength(0); i++)
                            {
                                for (int j = 0; j < alphaAreas.GetLength(1); j++)
                                {
                                    for (int at = 0; at < tData.alphamapLayers; at++)
                                    {
                                        int influencePointX = (int)((float)i / alphaAreas.GetLength(0) * influencingPoints.GetLength(0));
                                        int influencePointY = (int)((float)j / alphaAreas.GetLength(1) * influencingPoints.GetLength(1));
                                        float influenceAmount = influencingPoints[influencePointX, influencePointY] * strength;
                                        if (at == selectedTextureIndex)
                                        {
                                            alphaAreas[i, j, at] += influenceAmount;
                                        }
                                        else
                                        {
                                            alphaAreas[i, j, at] -= alphaAreas[i, j, at] * influenceAmount;
                                        }
                                    }
                                }
                            }

                            tData.SetAlphamaps(startX, startY, alphaAreas);
                            break;
                        case Mode.PAINT_DETAILS:
                            startX = (int)(Mathf.Max(hit.point.x - area, 0) / tData.size.x * tData.detailResolution);
                            startY = (int)(Mathf.Max(hit.point.z - area, 0) / tData.size.z * tData.detailResolution);
                            endX = (int)(Mathf.Min((hit.point.x + area) / tData.size.x * tData.detailResolution, tData.detailResolution));
                            endY = (int)(Mathf.Min((hit.point.z + area) / tData.size.z * tData.detailResolution, tData.detailResolution));

                            int[,] details = tData.GetDetailLayer(startX, startY, endX - startX, endY - startY, 0);

                            for (int i = 0; i < details.GetLength(0); i++)
                            {
                                for (int j = 0; j < details.GetLength(1); j++)
                                {
                                    details[i, j] = UnityEngine.Random.Range(0f, 1f) < strength / 25f ? 1 : 0;
                                }
                            }

                            tData.SetDetailLayer(startX, startY, 0, details);
                            break;
                    }
                }
            }
        }
    }

    public void changeMode(Mode newMode)
    {
        this.mode = newMode;
    }

    public void changeBrush(Texture2D newBrush)
    {
        //Convert texture2D to alpha float ponts with [0-1] range.
        Color[] brushPixels = newBrush.GetPixels(0, 0, newBrush.width, newBrush.height);
        influencingPoints = new float[newBrush.width, newBrush.height];

        for(int y = 0; y < influencingPoints.GetLength(1); y++)
        {
            for (int x = 0; x < influencingPoints.GetLength(0); x++)
            {
                influencingPoints[x, y] = brushPixels[x + y * newBrush.width].r;
            }
        }
    }

    public void setBrushSize(float area)
    {
        this.area = area;
        brushImage.localScale = new Vector3(area * buildTargetScaleMultiplier, 1, area * buildTargetScaleMultiplier);
        buildTarget.localScale = new Vector3(area * buildTargetScaleMultiplier, 1, area * buildTargetScaleMultiplier);
    }

    public void setSelectedTextureId(int id)
    {
        this.selectedTextureIndex = id;
    }

    public void setBrushImageVisibility(bool isActive)
    {
        brushImage.gameObject.SetActive(isActive);
    }
}
