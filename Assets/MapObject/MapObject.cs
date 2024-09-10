using UnityEngine;

public class MapObject : MonoBehaviour
{
    public string objectId;
    public string objectName;
    public Texture2D icon;

    public Vector2Int gridSize;

    private MeshRenderer[] meshRenderers;

    void Start()
    {
        gameObject.layer = 12; ///MapObject
        meshRenderers = GetComponentsInChildren<MeshRenderer>();

        DeSelect();
    }

    public void DestroyObject()
    {
        LevelData.mapObjects.Remove(this);
        var (unitGridX, unitGridY) = LevelData.gridManager.SamplePosition(transform.position.x, transform.position.z);
        LevelData.gridManager.RemoveMapObject(unitGridX, unitGridY);
        Destroy(gameObject);
    }

    /// <summary>Enables outline </summary>
    public void Select()
    {
        foreach (MeshRenderer renderer in meshRenderers)
            if (renderer.materials.Length > 1)
                renderer.materials[1].SetInt("_Enabled", 1);
    }

    /// <summary>Disables outline </summary>
    public void DeSelect()
    {
        foreach (MeshRenderer renderer in meshRenderers)
            if (renderer.materials.Length > 1)
                renderer.materials[1].SetInt("_Enabled", 0);
    }
    /// <summary>Does this fits in grid system</summary>
    public bool CanBePlaced()
    {
        var (unitGridX, unitGridY) = LevelData.gridManager.SamplePosition(transform.position.x, transform.position.z);
        bool gridColliding = LevelData.gridManager.CheckForCollision(unitGridX, unitGridY, gridSize.x, gridSize.y, transform.eulerAngles.y);

        return !gridColliding;
    }

    /// <summary>Sets specific material (to unit + body) </summary>
    public void SetMeshRendererMat(Material mat)
    {
        foreach (MeshRenderer mr in meshRenderers)
        {
            Material[] mats = mr.materials;
            mats[0] = mat;
            mr.materials = mats;
        }
    }
}
