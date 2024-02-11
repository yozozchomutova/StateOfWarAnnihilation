﻿#region [Libraries] All libraries
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
#endregion

public class UnitBody : UnitReference
{
    #region [Variables] Data info
    /// <summary> ID used for getting reference of body. </summary>
    [Header("Data info")]
    [Tooltip("ID used for getting reference of body.")]
    public string id;
    #endregion
    #region [Variables] Motion
    //Rotation
    /// <summary> Rotation speed of</summary>
    [HideInInspector] public float rotationSpeed;

    /// <summary> X axis of rotation, it's trying to achieve</summary>
    [HideInInspector] public float targetedRotationX = 0;
    /// <summary> Y axis of rotation, it's trying to achieve</summary>
    [HideInInspector] public float targetedRotationY = 0;
    /// <summary> Z axis of rotation, it's trying to achieve</summary>
    [HideInInspector] public float targetedRotationZ = 0;

    /// <summary> X axis of rotation, it currently have</summary>
    [HideInInspector] public float curRotationX = 0;
    /// <summary> Y axis of rotation, it currently have</summary>
    [HideInInspector] public float curRotationY = 0;
    /// <summary> Z axis of rotation, it currently have</summary>
    [HideInInspector] public float curRotationZ = 0;

    //Head for rotating
    /// <summary> Mount used for placing unit (It's just parent for Unit gameObject)</summary>
    [Tooltip("Parent/Transform, where unit will spawn in.")]
    public Transform headMount;
    #endregion
    #region [Variables] Animations
    [Header("Animations")]
    /// <summary> Animation of unit moving somewhere.</summary>
    [Tooltip("Animation of unit moving somewhere.")]
    public Animation animUnitMoving;
    #endregion
    #region [Variables] Physics
    /// <summary> TODO FILL </summary>
    #endregion
    #region [Variables] Navigation system
    private NavMeshPath navPath;
    private NavMeshAgent navAgent;
    [HideInInspector] public bool hasDestination;
    [HideInInspector] public Vector3[] destinations;
    private int currentDestinationID;

    [HideInInspector] public bool interruptable = true;

    public float updateNavigationDelay;
    #endregion
    #region [Variables] VFX
    [Header("Ground steps VFX")]
    public GameObject stepPrefab;
    public Transform[] stepSpawnpoints;
    public float stepMinimalDistance;
    private Vector3 stepLastPosition;

    [Header("VFX Lights")]
    /// <summary> Lights </summary>
    public GameObject[] lights;
    #endregion

    #region [Functions] Unity's Update() + Custom init()
    public override void init()
    {
        base.init();
        //InvokeRepeating("updateNavigation", updateNavigationDelay, updateNavigationDelay);

        //Get navAgent
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.agentTypeID = BarNavigations.availableAgents[LevelData.navigations_agentTypeID];
            navAgent.enabled = unit.virtualSpace == Unit.VirtualSpace.NORMAL;
        }

        //Decide which lighting configuration to use
        LevelData.environment.callBodyTimeCallback(this);
    }

    private void Update()
    {
        if (unit == null)
            return;

        if (navAgent != null && LevelData.scene == LevelData.Scene.EDITOR)
        {
            Destroy(navAgent);
            navAgent = null;
        }

        updateNavigation();
    }

    private void updateNavigation()
    {
        if (hasDestination)
        {
            if (destinations.Length <= currentDestinationID)
            {
                hasDestination = false;
                unit.commandStop();
                if (animUnitMoving != null)
                    animUnitMoving.Stop();

                return;
            }

            //Destination
            Vector3 destination = destinations[currentDestinationID];
            rotationSpeed = navAgent.angularSpeed * Time.deltaTime;

            //Reached destination?
            float distance = Vector3.Distance(destination, gameObject.transform.position);
            if (distance <= 1.5f)
            {
                currentDestinationID++;
            }

            //Is body aligned?
            //Rotating body
            bool rightRotateDirection = false;

            //Fix targetted Y
            if (targetedRotationY < -180)
                targetedRotationY += 360;
            else if (targetedRotationY > 180)
                targetedRotationY -= 360;

            if (curRotationY >= 0)
            {
                float minusCurRotation = curRotationY - 180;

                if (targetedRotationY > curRotationY && targetedRotationY > minusCurRotation ||
                    targetedRotationY < curRotationY && targetedRotationY < minusCurRotation)
                    rightRotateDirection = true;
            }
            else
            {
                float plusCurRotation = curRotationY + 180;

                if (targetedRotationY > curRotationY && targetedRotationY < plusCurRotation ||
                    targetedRotationY < curRotationY && targetedRotationY > plusCurRotation)
                    rightRotateDirection = true;
            }

            //Apply rotation Y
            if (curRotationY > targetedRotationY + rotationSpeed || curRotationY < targetedRotationY - rotationSpeed)
                curRotationY += rightRotateDirection ? rotationSpeed : -rotationSpeed;

            //Apply rotation X
            curRotationX += (targetedRotationX - curRotationX) * Time.deltaTime * 25f;
            curRotationZ += (targetedRotationZ - curRotationZ) * Time.deltaTime * 25f;

            //Bounds
            if (curRotationY > 180)
                curRotationY -= 360;
            else if (curRotationY < -180)
                curRotationY += 360;

            //Apply rotation to gameObject
            gameObject.transform.localRotation = Quaternion.Euler(curRotationX, curRotationY, curRotationZ);

            //Rotate towards destionation
            targetedRotationY = Mathf.Atan2(destination.x - transform.position.x,
                        destination.z - transform.position.z) * Mathf.Rad2Deg;

            //If rotated properly -> Proceed to move
            if (Mathf.Abs(curRotationY - targetedRotationY) < 3f)
            {
                transform.position += transform.forward * navAgent.speed * Time.deltaTime;
                //navAgent.Move(transform.forward * navAgent.speed * Time.deltaTime);
            }

            //Align body to terrain
            Vector3 currentPosition = transform.position;
            Vector3 terrainNormal = LevelData.mainTerrain.terrainData.GetInterpolatedNormal((currentPosition.x / LevelData.mainTerrain.terrainData.size.x),
                                                                (currentPosition.z / LevelData.mainTerrain.terrainData.size.z));

            Quaternion rot = Quaternion.FromToRotation(transform.up, terrainNormal) * transform.rotation;
            targetedRotationX = rot.x * 90;
            targetedRotationZ = rot.z * Mathf.Lerp(-90, 90, targetedRotationX / 180.0f + 0.5f);

            //Ground steps VFX
            if (stepPrefab != null)
            {
                float stepDistance = Vector3.Distance(stepLastPosition, currentPosition);
                if (stepDistance > stepMinimalDistance)
                {
                    stepLastPosition = currentPosition;
                    for (int i = 0; i < stepSpawnpoints.Length; i++)
                    {
                        GameObject newStepGround = Instantiate(stepPrefab, stepSpawnpoints[i].position, stepSpawnpoints[i].rotation);
                    }
                }
            }

            for (int j = 0; j < navPath.corners.Length - 1; j++)
            {
                Debug.DrawLine(navPath.corners[j], navPath.corners[j+1], Color.red);
            }
        }
    }
    #endregion
    #region [Functions] Manipulating with NavAgent
    public void moveTo(Vector3 position)
    {
        if (interruptable)
            StartCoroutine(updatePath(position));
    }

    public void stopMoving()
    {
        hasDestination = false;
        if (animUnitMoving != null)
            animUnitMoving.Stop();
    }

    IEnumerator updatePath(Vector3 position)
    {
        bool pathFound;
        NavMeshHit nHit;

        if (!navAgent.isOnNavMesh)
        {
            NavMesh.SamplePosition(gameObject.transform.position, out nHit, 100, NavMesh.AllAreas);
            warp(nHit.position);
        }

        NavMesh.SamplePosition(position, out nHit, 100, NavMesh.AllAreas);
        navPath = new NavMeshPath();
        pathFound = navAgent.CalculatePath(nHit.position, navPath);
        while (!pathFound)
        {
            yield return new WaitForSeconds(0.5f);
        }

        destinations = navPath.corners;
        currentDestinationID = 0;
        hasDestination = true; //Begin moving

        if (animUnitMoving != null)
            animUnitMoving.Play();
    }

    public void warp(Vector3 pos)
    {
        if (navAgent != null)
            navAgent.Warp(pos);
        else
            transform.localPosition = pos;
    }

    public void setAutopath(Vector3 destination, float maxSpeed)
    {
        navAgent.speed = maxSpeed;
        warp(transform.position);
        //moveTo(destination);
        navAgent.SetDestination(destination);
        interruptable = false; //Lock -> Player won't be able to controll it
    }

    #endregion
    #region [Functions] VFX Day / Night cycle functions
    /// <summary> Called when day starts in weather environment</summary>
    public void onDayStart()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].SetActive(false);
        }
    }

    /// <summary> Called when night starts in weather environment</summary>
    public void onNightStart()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].SetActive(true);
        }
    }
    #endregion
}