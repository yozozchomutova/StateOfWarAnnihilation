#region [Libraries] All libraries
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapLevelManager;
using static UnityEngine.UI.Image;
#endregion

public class GridManager
{
    #region [Variables] Main 
    /// <summary> Main map </summary>
    public Tile[,] tiles;
    /// <summary> Width of tiles array </summary>
    public int tilesWidth;
    /// <summary> Height of tiles array </summary>
    public int tilesHeight;
    public Texture2D grayMap, selectionMap;
    public Color[] grayMapColors, selectionMapColors, fullBlack, fullWhite;
    #endregion
    #region [Variables] Settings
    /// <summary> How large is 1 tile </summary>
    public float tileWorldSize = 0.25f;
    #endregion
    #region [Variables] Others
    /// <summary> Mesh renderer of grid mesh / gameobject </summary>
    private MeshRenderer mr;
    #endregion

    #region [Functions] Initialization
    public void Nullate()
    {
        tiles = null;
    }

    /// <summary Creates new tiles array</summary>
    /// <param name="width"></param>
    /// <param name="height">probably same as width</param>
    public void NewGrid(TerrainData td, float width, float height)
    {
        //Initialize
        mr = GlobalList.gridParent.GetComponent<MeshRenderer>();

        tilesWidth = (int) (width / tileWorldSize);
        tilesHeight = (int) (height / tileWorldSize);

        tiles = new Tile[tilesWidth, tilesHeight];

        grayMap = new Texture2D(tilesWidth, tilesHeight);
        grayMap.filterMode = FilterMode.Point;
        grayMapColors = new Color[tilesWidth * tilesHeight];

        selectionMap = new Texture2D(tilesWidth, tilesHeight);
        selectionMap.filterMode = FilterMode.Point;
        selectionMapColors = new Color[tilesWidth * tilesHeight];

        fullBlack = new Color[tilesWidth * tilesHeight];
        fullWhite = new Color[tilesWidth * tilesHeight];

        for (int x = 0; x < tilesWidth; x++)
        {
            for (int y = 0; y < tilesHeight; y++)
            {
                SetColorPixel(grayMapColors, x, y, Color.black);
                SetColorPixel(selectionMapColors, x, y, Color.black);
                SetColorPixel(fullBlack, x, y, Color.black);
                SetColorPixel(fullWhite, x, y, Color.white);

                tiles[x, y] = new Tile();
            }
        }

        UpdateTexture2D(grayMap, grayMapColors);
        UpdateTexture2D(selectionMap, selectionMapColors);

        BuildGrid(td);
    }
    #endregion
    #region [Functions] Check for collisions / fetching data from grid
    /// <summary>
    /// Checks if there's free space in area
    /// </summary>
    /// <returns>true = colliding with something / false = no collision detected, free space</returns>
    public bool CheckSquareCollision(int originX, int originY, float checkWidth, float checkHeight)
    {
        Vector2Int start, end;
        (start, end) = ToPoints(originX, originY, checkWidth, checkHeight);

        if (start.x < 0 || start.y < 0 || end.x >= tilesWidth || end.y >= tilesHeight) //Out of bounds
            return true;

        for (int y = start.y; y < end.y; y++)
        {
            for (int x = start.x; x < end.x; x++)
            {
                if (!tiles[x, y].IsFree())
                    return true;
            }
        }

        return false;
    }

    public bool CheckForCollision(int originX, int originY, int checkWidth, int checkHeight, float rotation)
    {
        List<Vector2Int> cols = GenerateColliders(originX, originY, checkWidth, checkHeight, rotation);
        foreach (Vector2Int v in cols)
        {
            if (v.x < 0 || v.y < 0 || v.x >= tilesWidth || v.y >= tilesHeight) //Out of bounds
                return true;

            if (!tiles[v.x, v.y].IsFree())
                return true;
        }

        return false;
    }
    #endregion
    #region [Functions] Grid operations (Adding, Removing stuff)
    public void BuildGrid(TerrainData td)
    {
        Mesh mesh = GenerateMeshFromTerrain(td);
        GlobalList.gridParent.GetComponent<MeshFilter>().mesh = mesh;

        GenerateTerrainWalkables(td);
        UpdateTexture2D(grayMap, grayMapColors);

        mr.material.SetFloat("_Tiling", tilesWidth);
        mr.material.SetTexture("_Graying", grayMap);
        mr.material.SetTexture("_Selection", selectionMap);
    }

    public void PlaceUnit(Unit u, int originX, int originY, float rotation)
    {
        Vector2Int o = new Vector2Int(originX, originY);

        List<Vector2Int> cols = GenerateColliders(originX, originY, u.gridSize.x, u.gridSize.y, rotation);
        foreach (Vector2Int v in cols)
        {
            tiles[v.x, v.y].objectOrigin = o;
            tiles[v.x, v.y].unit= u;

            SetColorPixel(grayMapColors, v.x, v.y, Color.white);
        }

        UpdateTexture2D(grayMap, grayMapColors);
    }

    public void RemoveUnit(int originX, int originY)
    {
        Unit u = tiles[originX, originY].unit;
        List<Vector2Int> cols = GenerateColliders(originX, originY, u.gridSize.x, u.gridSize.y, u.transform.eulerAngles.y);
        foreach (Vector2Int v in cols)
        {
            tiles[v.x, v.y].Clear();

            SetColorPixel(grayMapColors, v.x, v.y, Color.black);
        }

        UpdateTexture2D(grayMap, grayMapColors);
    }

    public void PlaceMapObject(MapObject mo, int originX, int originY, float rotation) {
        Vector2Int o = new Vector2Int(originX, originY);

        List<Vector2Int> cols = GenerateColliders(originX, originY, mo.gridSize.x, mo.gridSize.y, rotation);
        foreach (Vector2Int v in cols)
        {
            tiles[v.x, v.y].objectOrigin = o;
            tiles[v.x, v.y].mapObject = mo;

            SetColorPixel(grayMapColors, v.x, v.y, Color.white);
        }

        UpdateTexture2D(grayMap, grayMapColors);
    }

    public void RemoveMapObject(int originX, int originY)
    {
        MapObject mo = tiles[originX, originY].mapObject;
        List<Vector2Int> cols = GenerateColliders(originX, originY, mo.gridSize.x, mo.gridSize.y, mo.transform.eulerAngles.y);
        foreach (Vector2Int v in cols)
        {
            tiles[v.x, v.y].Clear();

            SetColorPixel(grayMapColors, v.x, v.y, Color.black);
        }

        UpdateTexture2D(grayMap, grayMapColors);
    }

    /// <summary>Destroys units and map objects depending on brush</summary>
    public void DestroyInBrush(int originX, int originY)
    {
        Unit u = tiles[originX, originY].unit;
        if (u)
        {
            //Create undo action
            UnitSerializable unitS = u.serializeUnit();
            byte[] data = ArrayWorker.SerializableToBytes(unitS);
            EditorManager.self.createUndoAction(EditorManager.UndoType.REMOVE_UNIT, data, 1);

            RemoveUnit(originX, originY);
            u.destroyBoth();
        }

        MapObject mo = tiles[originX, originY].mapObject;
        if (mo)
        {
            //Create undo action

            mo.DestroyObject();
        }
    }

    public void GenerateTerrainWalkables(TerrainData td)
    {
        float[,] heights = td.GetHeights(0, 0, td.heightmapResolution, td.heightmapResolution);

        for (int x = 0; x < tilesWidth; x++)
        {
            for (int y = 0; y < tilesHeight; y++)
            {
                tiles[x, y].terrainWalkable = true;

                float fx = x;
                float fy = y;

                int terPointX = (int)(fx / tilesWidth * td.heightmapResolution);
                int terPointY = (int)(fy / tilesHeight * td.heightmapResolution);

                float normX = fx / tilesWidth;
                float normY = fy / tilesHeight;

                float offset = 0.000f;
                float steepnes = td.GetSteepness(normX + offset, normY + offset);
                if (x == 0 || y == 0 || x == tilesWidth - 1 || y == tilesHeight - 1)
                {
                    tiles[x, y].terrainWalkable = false;
                }
                else if (
                    Mathf.Abs(steepnes) > 22 ||
                    heights[terPointY, terPointX] < 0.3f
                    )
                {
                    tiles[x, y].terrainWalkable = false;
                    tiles[x + 1, y].terrainWalkable = false;
                    tiles[x - 1, y].terrainWalkable = false;
                    tiles[x, y + 1].terrainWalkable = false;
                    tiles[x, y - 1].terrainWalkable = false;
                }

                if (tiles[x, y].IsFree())
                {
                    SetColorPixel(grayMapColors, x, y, Color.black);
                }
                else
                {
                    SetColorPixel(grayMapColors, x, y, Color.white);
                }
            }
        }
    }
    /// <summary>X,Y position Width,Height size and optional rotation</summary>
    /// <returns>List of points in grid that forms collider</returns>
    private List<Vector2Int> GenerateColliders(int originX, int originY, int w, int h, float rotation = 0f)
    {
        List<Vector2Int> cols = new List<Vector2Int>();

        //Adjust rotation
        int intRot = (int)MathF.Round(rotation);
        int offsetX = 0;
        int offsetY = 0;

        if (intRot >= 90)
        {
            offsetX += -1;
        }
        if (intRot >= 180)
        {
            offsetY += -1;
        }
        if (intRot > 180)
        {
            offsetX += 1;
        }
        if (intRot > 270)
        {
            offsetY += 1;
            //offsetX += -1;
        }

        // Convert rotation from degrees to radians
        int negRot = -intRot;
        double normPi = Math.PI / 180;
        double rad = normPi * negRot;

        double sin = Math.Round(Math.Sin(rad) * 100000.0) / 100000.0;
        double cos = Math.Round(Math.Cos(rad) * 100000.0) / 100000.0;

        float dest_width = w * 2f + 1;
        float dest_height = h * 2f + 1;

        // Loop through the specified area
        for (int y = 0; y <= dest_height; y++)
        {
            for (int x = 0; x <= dest_width; x++)
            {
                double x0 = w / 2f - cos * dest_width / 2f - sin * dest_height / 2f;
                double y0 = h / 2f - cos * dest_height / 2f + sin * dest_width / 2f;

                // Apply rotation
                int rotatedX = (int)Math.Floor(cos * x + sin * y + x0) + offsetX;
                int rotatedY = (int)Math.Floor(-sin * x + cos * y + y0) + offsetY;

                // Ensure final coordinates are within bounds
                if (rotatedX >= 0 && rotatedX < w && rotatedY >= 0 && rotatedY < h)
                {
                    cols.Add(new Vector2Int(
                        originX + x - (int)(dest_width / 2f),
                        originY + y - (int)(dest_height / 2f)
                        ));
                }
            }
        }

        return cols;
    }
    #endregion
    #region [Functions] Some math and calculations
    /// <summary>
    /// Centers position directly to center of tile
    /// </summary>
    /// <returns>X, Z of aligned position</returns>
    public (float, float) AlignPosition(float posX, float posZ)
    {
        return (
            posX - (posX % tileWorldSize) + tileWorldSize / 2f,
            posZ - (posZ % tileWorldSize) + tileWorldSize / 2f
            );
    }

    /// <summary>
    /// Returns X,Y (indexes) tile from specific world position
    /// </summary>
    /// <param name="posX">world position X</param>
    /// <param name="posZ">world position Z</param>
    /// <returns>X, Y of tile</returns>
    public (int, int) SamplePosition(float posX, float posZ)
    {
        return (
            (int)(posX / tileWorldSize),
            (int)(posZ / tileWorldSize)
            );
    }

    /// <returns>(start, end) Vector2 Integer points</returns>
    public (Vector2Int, Vector2Int) ToPoints(int originX, int originY, float checkWidth, float checkHeight)
    {
        return (
            new Vector2Int(originX - (int)MathF.Floor(checkWidth / 2f), originY - (int)MathF.Floor(checkHeight / 2f)),
            new Vector2Int(originX + (int)MathF.Ceiling(checkWidth / 2f), originY + (int)MathF.Ceiling(checkHeight / 2f))
        );
    }
    #endregion
    #region [Functions] Visuals
    public void ShowGrid()
    {
        GlobalList.gridParent.SetActive(true);
    }

    public void HideGrid()
    {
        GlobalList.gridParent.SetActive(false);
    }

    public void UpdateSelection(int originX, int originY, int w, int h, bool selValid, float rotation = 0f)
    {
        if (GlobalList.gridParent.activeSelf)
        {
            UpdateTexture2D(selectionMap, fullBlack, false);

            List<Vector2Int> cols = GenerateColliders(originX, originY, w, h, rotation);
            foreach (Vector2Int v in cols)
            {
                selectionMap.SetPixel(v.x, v.y, Color.white);
            }

            selectionMap.Apply();
            mr.material.SetInt("_SelectionValid", selValid ? 1 : 0);
        }
    }

    private void SetColorPixel(Color[] array, int x, int y, Color c)
    {
        array[y * tilesWidth + x] = c;
    }

    private void UpdateTexture2D(Texture2D txt2D, Color[] colors, bool apply = true) {
        UpdateTexture2D(txt2D, 0, 0, tilesWidth, tilesHeight, colors, apply);
    }

    private void UpdateTexture2D(Texture2D txt2D, int x, int y, int w, int h, Color[] colors, bool apply = true)
    {
        txt2D.SetPixels(x, y, w, h, colors);
        if (apply) txt2D.Apply();
    }

    private Mesh GenerateMeshFromTerrain(TerrainData terrainData)
    {
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        Vector3 terrainSize = terrainData.size;
        float[,] heights = terrainData.GetHeights(0, 0, width, height);

        Vector3[] vertices = new Vector3[width * height];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(width - 1) * (height - 1) * 6];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                float heightValue = heights[y, x];
                vertices[index] = new Vector3(
                    x * terrainSize.x / (width - 1),
                    heightValue * terrainSize.y,
                    y * terrainSize.z / (height - 3));
                uvs[index] = new Vector2(x / (float)(width - 1), y / (float)(height - 3));
            }
        }

        int triangleIndex = 0;
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int currentIndex = y * width + x;
                triangles[triangleIndex++] = currentIndex;
                triangles[triangleIndex++] = currentIndex + width;
                triangles[triangleIndex++] = currentIndex + width + 1;

                triangles[triangleIndex++] = currentIndex;
                triangles[triangleIndex++] = currentIndex + width + 1;
                triangles[triangleIndex++] = currentIndex + 1;
            }
        }

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();

        return mesh;
    }
    #endregion

    #region [Class] Tile
    public class Tile
    {
        public bool terrainWalkable;
        public Vector2Int objectOrigin;
        public Unit unit;
        public MapObject mapObject;

        public bool IsFree()
        {
            return terrainWalkable && !unit && !mapObject;
        }

        public void Clear()
        {
            unit = null;
            mapObject = null;
        }
    }
    #endregion
}