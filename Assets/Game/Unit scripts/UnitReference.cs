using System.Collections.Generic;
using UnityEngine;

/// <summary> This class is just nothing than reference for Unit and UnitBody. It's used to get Unit from collider fast & efficiently. </summary>
[RequireComponent (typeof(BoxCollider))]
public abstract class UnitReference : MonoBehaviour
{
    [HideInInspector] public Unit unit;
    [HideInInspector] public UnitBody body;
    [HideInInspector] public new BoxCollider collider;

    #region [Variables] Rendering properties
    //Texture managing
    /// <summary> All mesh renderers combined together </summary>
    [HideInInspector] public MeshRenderer[] meshRenderersAll;
    /// <summary> All mesh renderers combined together </summary>
    [HideInInspector] public Material[] meshRenderersDefault;
    #endregion

    public virtual void init()
    {
        collider = GetComponent<BoxCollider>();
        registerAllMeshRenderers();
    }

    /// <summary> Links unit & body, and makes them live together and working together for further operations </summary>
    public void link(Unit unit, UnitBody body)
    {
        unit.unit = unit;
        unit.body = body;

        if (body != null)
        {
            body.unit = unit;
            body.body = body;
        }
    }

    /// <summary> Destroys Unit and Body together</summary>
    public void destroyBoth()
    {
        LevelData.units.Remove(unit);

        if (body != null)
        {
            body.stopMoving();
            Destroy(body.gameObject);
        }
        if (unit != null)
            Destroy(unit.gameObject);
    }

    #region [Functions] Rendering
    private void registerAllMeshRenderers()
    {
        List<MeshRenderer> mrList = new List<MeshRenderer>();

        //Gather parent + all children mesh renderers
        MeshRenderer parentRenderer = gameObject.GetComponent<MeshRenderer>();
        if (parentRenderer != null)
            mrList.Add(parentRenderer);

        registerChildrenMeshRenderers(mrList, gameObject.transform);

        //Convert list to array
        meshRenderersAll = mrList.ToArray();

        //Gather default materials from all meshrenderers
        meshRenderersDefault = new Material[meshRenderersAll.Length];
        for (int i = 0; i < meshRenderersDefault.Length; i++)
            meshRenderersDefault[i] = meshRenderersAll[i].material;
    }

    private void registerChildrenMeshRenderers(List<MeshRenderer> mrList, Transform toCheck)
    {
        for (int i = 0; i < toCheck.childCount; ++i)
        {
            MeshRenderer mr = toCheck.GetChild(i).GetComponent<MeshRenderer>();
            if (mr != null)
                mrList.Add(mr);

            registerChildrenMeshRenderers(mrList, toCheck.GetChild(i));
        }
    }

    /// <summary> Calls private version of this method and restores default material (to unit + body) </summary>
    public void restoreMeshRendererMat(Team team)
    {
        unit.restoreMeshRendererMat(team, unit.unitType);
        if (body != null)
            body.restoreMeshRendererMat(team, unit.unitType);
    }
    /// <summary> Restores default material </summary>
    private void restoreMeshRendererMat(Team team, Unit.UnitType unitType)
    {
        for (int i = 0; i < meshRenderersAll.Length; ++i)
        {
            MeshRenderer mr = meshRenderersAll[i];
            Material defaultMat = meshRenderersDefault[i];

            mr.material = defaultMat;
            mr.material.SetTexture("mainTexture", team.getTextureByType(unitType));
        }
    }

    /// <summary> Calls private version of this method and sets material (to unit + body) </summary>
    public void setMeshRendererMat(Material mat)
    {
        unit.setMeshRendererMatPriv(mat);
        if (body != null)
            body.setMeshRendererMatPriv(mat);
    }
    /// <summary> Sets material </summary>
    private void setMeshRendererMatPriv(Material mat)
    {
        foreach (MeshRenderer mr in meshRenderersAll)
        {
            mr.material = mat;
            mr.material.SetTexture("mainTexture", unit.team.getTextureByType(unit.unitType));
            //mr.material.SetTexture("mainTexture", mat.mainTexture);
        }
    }

    /// <summary> Calls private version of this method and changes parameter of construction material (to unit + body)</summary>
    /// <param name="progressNormalized">Range 0..1</param>
    public void updateMeshConstructionMat(float progressNormalized) //range 0-1
    {
        unit.updateMeshConstructionMatPriv(progressNormalized);
        if (body != null)
            body.updateMeshConstructionMatPriv(progressNormalized);
    }
    /// <summary> Changes parameter of construction material </summary>
    private void updateMeshConstructionMatPriv(float progressNormalized) //range 0-1
    {
        float cutOffHeightProperty = progressNormalized * 1.6f + -0.7f;
        foreach (MeshRenderer mr in meshRenderersAll)
        {
            if (mr.material.name == "unitMatConstruction (Instance)")
                mr.material.SetFloat("_CutoffHeight", cutOffHeightProperty);
        }
    }
    #endregion
    #region [Functions] Get/Set Body properties
    public Vector3 getPosition()
    {
        if (body != null)
            return body.transform.localPosition;
        return transform.localPosition;
    }

    public Vector3 getRotation()
    {
        if (body != null)
            return body.transform.localEulerAngles;
        return transform.localEulerAngles;
    }

    public void setPosition(Vector3 pos)
    {
        if (body != null)
            body.transform.localPosition = pos;
        else 
            transform.localPosition = pos;
    }

    public void setRotation(Vector3 rot)
    {
        if (body != null)
            body.transform.localEulerAngles = rot;
        else
            transform.localEulerAngles = rot;
    }

    public Transform getRoot()
    {
        if (body != null)
            return body.transform;
        return unit.transform;
    }
    #endregion
}