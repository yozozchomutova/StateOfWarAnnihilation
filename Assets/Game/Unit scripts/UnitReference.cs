using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
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
    /// <summary> Mesh renderers that have outline-shader material </summary>
    [HideInInspector] public MeshRenderer[] unitTeamMrs;
    /// <summary> Mesh renderers that have screen LCD material </summary>
    [HideInInspector] public MeshRenderer[] screenLCDMrs;
    /// <summary> Mesh renderers that have outline-shader material </summary>
    [HideInInspector] public MeshRenderer[] outlineMrs;
    /// <summary> Mesh renderers that have outline-shader material </summary>
    [HideInInspector] public int[] outlineMatsIndexes;
    /// <summary>  </summary>
    [HideInInspector] public string[] meshRenderersDefault_names;
    /// <summary>  </summary>
    [HideInInspector] public string[] meshRenderersDefault_materials;
    #endregion

    public virtual void init()
    {

    }

    public void EditorsInit()
    {
        //New
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

        if (unit.virtualSpace == Unit.VirtualSpace.NORMAL && unit.unitDeathVFXType == Unit.DeathEffect.HEAD_BLOW)
        {
            body.stopMoving();
            Destroy(body.gameObject, 6);
            Destroy(unit.gameObject, 6);
        } else
        {
            if (body != null)
            {
                body.stopMoving();
                Destroy(body.gameObject);
            }
            if (unit != null)
                Destroy(unit.gameObject);
        }
    }

    #region [Functions] Rendering
    /*  All MeshRenderers first material (index = 0) is always they primary material
     * 
     */

    /// <summary> </summary>
    private void registerAllMeshRenderers()
    {
        List<MeshRenderer> unitTeamMrs = new List<MeshRenderer>();
        List<MeshRenderer> screenLCDMrs = new List<MeshRenderer>();
        List<MeshRenderer> outlineMrs = new List<MeshRenderer>();
        List<string> meshRenderersDefault_names = new List<string>();
        List<string> meshRenderersDefault_materials = new List<string>();

        //Gather parent + all children mesh renderers
        meshRenderersAll = gameObject.GetComponentsInChildren<MeshRenderer>();

        //Gather primary materials from all meshrenderers
        for (int i = 0; i < meshRenderersAll.Length; i++)
        {
            MeshRenderer mr = meshRenderersAll[i];
            string matName = mr.sharedMaterials[0].name;
            if (matName == "unitTeamMat")
            {
                unitTeamMrs.Add(mr);
            }
            else if(matName == "screenLCDMat")
            {
                screenLCDMrs.Add(mr);
            } else
            {
                meshRenderersDefault_names.Add(mr.name);
                meshRenderersDefault_materials.Add(mr.sharedMaterials[0].name);
            }

            foreach (Material mat in mr.sharedMaterials)
            {
                if (mat.name == "outlineMat")
                {
                    outlineMrs.Add(mr);
                }
            }
        }

        //List to array
        this.unitTeamMrs = unitTeamMrs.ToArray();
        this.screenLCDMrs = screenLCDMrs.ToArray();
        this.outlineMrs = outlineMrs.ToArray();
        this.meshRenderersDefault_names = meshRenderersDefault_names.ToArray();
        this.meshRenderersDefault_materials = meshRenderersDefault_materials.ToArray();
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
        //Unit Team material
        for (int i = 0; i < unitTeamMrs.Length; i++)
        {
            MeshRenderer mr = unitTeamMrs[i];

            Material[] mats = mr.materials;
            mats[0] = Instantiate(GlobalList.matUnitTeam);
            mats[0].SetColor("_Team", team.minimapColor);
            mr.materials = mats;
        }

        //Screen LCD material
        for (int i = 0; i < screenLCDMrs.Length; i++)
        {
            MeshRenderer mr = screenLCDMrs[i];

            Material[] mats = mr.materials;
            mats[0] = Instantiate(GlobalList.matLCDScreen);
            mats[0].SetColor("_Team", team.minimapColor);
            mr.materials = mats;
        }

        return;

        //TODO DO SOMETHING WITH IT
        for (int i = 0; i < meshRenderersAll.Length; ++i)
        {
            MeshRenderer mr = meshRenderersAll[i];

            if (mr.sharedMaterials[0].name == "unitTeamMat")
            {
                //Material[] mats = mr.materials;
                //mats[0] = Instantiate(GlobalList.matUnitTeam);
                //mats[0].SetTexture("mainTexture", team.getTextureByType(unitType));
                //mr.materials = mats;
            } else
            {
                //TODO FINISH
                //int index = meshRenderersDefault_names.IndexOf(mr.name);
                //string matName = meshRenderersDefault_materials
                //Material defaultMat = meshRenderersDefault_materials[index];
                //mr.material = defaultMat;
            }
        }
    }

    /// <summary> Sets specific material (to unit + body) </summary>
    public void setMeshRendererMat(Material mat)
    {
        setMeshRendererMatPriv(unit.meshRenderersAll);
        if (body != null)
            setMeshRendererMatPriv(body.meshRenderersAll);

        void setMeshRendererMatPriv(MeshRenderer[] mrs)
        {
            foreach (MeshRenderer mr in mrs)
            {
                Material[] mats = mr.materials;
                mats[0] = mat;
                mats[0].SetColor("_Team", unit.team.minimapColor);
                mr.materials = mats;
                //mr.material.SetTexture("mainTexture", mat.mainTexture);
            }
        }
    }

    public void SelectUnit()
    {
        process(unit.outlineMrs);
        if (body)
            process(body.outlineMrs);

        void process(MeshRenderer[] meshRends)
        {
            foreach (MeshRenderer mr in meshRends)
            {
                Material[] mats = mr.materials;
                mats[0].SetFloat("_Selected_Overlay", 0.25f);
                mats[1].SetInt("_Enabled", 1);
                mr.materials = mats;
            }
        }
    }

    public void DeSelectUnit()
    {
        process(unit.outlineMrs);
        if (body)
            process(body.outlineMrs);

        void process(MeshRenderer[] meshRends)
        {
            foreach (MeshRenderer mr in meshRends)
            {
                Material[] mats = mr.materials;
                mats[0].SetFloat("_Selected_Overlay", 0);
                mats[1].SetInt("_Enabled", 0);
                mr.materials = mats;
            }
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