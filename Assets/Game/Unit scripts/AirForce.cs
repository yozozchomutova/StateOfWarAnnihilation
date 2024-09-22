#region [Libraries] All libraries
using System;
using System.Collections;
using UnityEngine;
using static MapLevelManager;
using static MapLevelManager.UnitSerializable;
using static Projectile;
#endregion

public class AirForce : Unit
{
    #region [Variables] Data info
    /// <summary> t </summary>
    #endregion
    #region [Variables] Gameplay properties
    /// <summary> Distance between ground and unit </summary>
    [HideInInspector] public float heightOffset_end;
    /// <summary> Can be air force destroyed by enemy units. </summary>
    [HideInInspector] public bool canBeShotDown;
    /// <summary> Instantly teleport to target and skip moving process. </summary>
    [HideInInspector] public bool instantTeleportToTarget;
    /// <summary> Minimal distance to trigger into PERFORMING state </summary>
    [HideInInspector] public float attackTriggerDistance;
    /// <summary> Length of action in ms.</summary>
    [HideInInspector] public float actionLengthTime;

    [HideInInspector] public bool fireIncludesVelocity;

    [HideInInspector] public Vector3 additionalWeaponForce = Vector3.zero;

    //Approaching
    [HideInInspector] public ApproachType approachType;

    //Targetting
    [HideInInspector] public TargetMode targetMode;
    [HideInInspector] public float weaponTargetOffset;

    //Spawn weapons
    [HideInInspector] public bool spawnRandomWeapons;
    [HideInInspector] public int spawnWeaponCount;

    //Weapons
    [HideInInspector] public float weaponDamage;
    [HideInInspector] public float weaponCoreBlast;
    [HideInInspector] public float weaponBlast;
    [HideInInspector] public bool weaponIsFreefall;
    [HideInInspector] public bool shuffleWeaponOrder;

    //Weapon fire delaying
    [HideInInspector] public float weaponLaunchDelay_randomBase;
    [HideInInspector] public float weaponLaunchDelay_spacing;

    [HideInInspector] public float projectileSpreadRandomization;

    //Motion
    [HideInInspector] public float acceleration;
    [HideInInspector] public float speedNormal;
    [HideInInspector] public float speedAction;

    //Construction
    [HideInInspector] public float coreRotationY;
    [HideInInspector] public float offRotY;
    /// <summary> t </summary>
    [HideInInspector] public float adjustmentCornerScale;
    /// <summary> t </summary>
    [HideInInspector] public float terrainImageScale_primary;
    /// <summary> t </summary>
    [HideInInspector] public float MIN_adjustment;
    /// <summary> t </summary>
    [HideInInspector] public float MAX_adjustment;
    #endregion
    #region [Variables] Runtime properties
    /// <summary> Is unit launched and doing it's task on map </summary>
    [HideInInspector] public bool launched;
    /// <summary> Point where unit will end. </summary>
    [HideInInspector] public Vector3 targettedEnd;

    //Motion
    private float curSpeed;
    #endregion
    #region [Variables] Air Force Construction
    [Header("Construction")]
    public Transform[] fans;
    public Transform[] frontThrusters;
    public Transform[] backThrusters;
    public Transform bottomDoorLeft;
    public Transform bottomDoorRight;

    [Header("Animations")]
    public Animation animRoot;
    public AnimationClip animTargetApproach;
    public AnimationClip animTargetLeave;

    [Header("Spawning weapons")]
    public Projectile[] weaponPrefabs;

    [Header("Weapons")]
    public Projectile[] weapons;
    #endregion
    #region [Variables] Rendering
    /// <summary> t </summary>
    public Material adjustmentCornersMat;
    /// <summary> t </summary>
    public Material terrainImageMat_primary;
    #endregion
    #region [Variables] Temporary data
    /// <summary> Stores unit ID, which will be deployed from carrybus. They deploy in count of 8 and overrides default configuration. In case of empty -> this variable is ignored</summary>
    public static string TEMPORARY_CARRYBUS_UNIT = "";
    #endregion

    //TODO -> Tidy it up, I don't have mentality to fix it
    private ActionState actionState = ActionState.MOVING;

    private float targettedSpeed;

    private Vector3 centerTargetTrigger; //Point, where to trigger action/weapons
    private Vector3[] targets;

    private float offRotZ, targettedOffrotZ;
    private float offsetYAcceleration, offsetYSpeed, targettedSpeedY;

    private bool bottomDoorsOpened = false;
    private float bottomDoorLeftR = 155;
    private float bottomDoorRightR = -200;

    #region [Functions] Init/onInit
    public override void onInit(Vector3 position, Vector3 rotation)
    {
        base.onInit(position, rotation);

        targettedSpeed = speedNormal;
        curSpeed = speedNormal;
    }
    #endregion
    #region [Functions] frameUpdate()
    public override void frameUpdate()
    {
        if (launched)
        {
            //Rotations / Rotate towards end
            transform.LookAt(targettedEnd, Vector3.up);
            transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0f);
            transform.Rotate(0, offRotY, offRotZ);
            offRotZ += (targettedOffrotZ - offRotZ) * 2.5f * Time.deltaTime;
            offRotY += coreRotationY * Time.deltaTime;

            //Move towards end
            Vector3 velocity = Vector3.zero;
            if (instantTeleportToTarget && actionState == ActionState.MOVING)
            {
                transform.position = centerTargetTrigger;
            }
            else
            {
                curSpeed += (targettedSpeed - curSpeed) * acceleration * Time.deltaTime;

                Vector3 direction = targettedEnd - transform.position;
                velocity = direction.normalized * curSpeed * Time.deltaTime;
                transform.position = transform.position + velocity;

                //targetted offset Y
                offsetYSpeed += (targettedSpeedY - offsetYSpeed) * offsetYAcceleration * Time.deltaTime;
                transform.position += Vector3.up * offsetYSpeed * Time.deltaTime;
            }

            //Launch weapons trigger
            float distanceToTarget = Vector3.Distance(gameObject.transform.position, centerTargetTrigger);
            if (actionState == ActionState.MOVING && attackTriggerDistance > distanceToTarget)
            {
                onActionStart();
                StartCoroutine(onActionEnd());

                for (int i = 0; i < weapons.Length; i++)
                {
                    Vector3 randomSpread = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
                    Vector3 baseVelocity = fireIncludesVelocity ? velocity : Vector3.zero;
                    weapons[i].launch(baseVelocity + (randomSpread * projectileSpreadRandomization) + additionalWeaponForce, UnityEngine.Random.Range(0, weaponLaunchDelay_randomBase) + (weaponLaunchDelay_spacing * i));
                }
            }

            //Fans animation
            foreach (Transform fan in fans)
            {
                fan.Rotate(curSpeed * Time.deltaTime * 75f, 0, 0);
            }

            //Bottom doors animation
            if (approachType != ApproachType.LAND) //LAND-type air forces have special-controlled doors...
                bottomDoorsOpened = actionState == ActionState.PERFOMING;

            if (bottomDoorLeft != null)
            {
                if (bottomDoorsOpened)
                {
                    float bl_rot = (20 - bottomDoorLeftR) * Time.deltaTime;
                    bottomDoorLeftR += bl_rot;
                    bl_rot = (-200 - bottomDoorRightR) * Time.deltaTime;
                    bottomDoorRightR += bl_rot;
                }
                else
                {
                    float bl_rot = (-90 - bottomDoorLeftR) * Time.deltaTime;
                    bottomDoorLeftR += bl_rot;
                    bl_rot = (-90 - bottomDoorRightR) * Time.deltaTime;
                    bottomDoorRightR += bl_rot;
                }

                bottomDoorLeft.transform.localEulerAngles = new Vector3(bottomDoorLeftR, 0, 0);
                bottomDoorRight.transform.localEulerAngles = new Vector3(bottomDoorRightR, 0, 0);
            }

            //Change speed
            if (actionState != ActionState.PERFOMING || approachType != ApproachType.LAND)
            {
                if (actionState == ActionState.MOVING || actionState == ActionState.PERFOMED)
                    targettedSpeed = speedNormal;
                else if (actionState == ActionState.PERFOMING)
                {
                    if (approachType == ApproachType.SLOWDOWN)
                        targettedSpeed = speedAction;
                    else if (approachType == ApproachType.STOP)
                        targettedSpeed = 0;
                }
            }

            //Reached end?
            if (Vector3.Distance(transform.position, targettedEnd) < 5f)
            {
                destroyUnit(0);
            }
        }
    }
    #endregion
    #region [Functions] Start/End action
    public void onActionStart()
    {
        actionState = ActionState.PERFOMING;

        if (approachType == ApproachType.LAND)
        {
            StartCoroutine(land_and_spawn_animation());
        }
    }

    IEnumerator onActionEnd()
    {
        yield return new WaitForSeconds(actionLengthTime);
        actionState = ActionState.PERFOMED;
    }
    #endregion
    #region [Functions] Commands
    public void commandLaunch(float endX, float endZ, float targetX, float targetZ, float airForceAdjustment)
    {
        //End
        targettedEnd = new Vector3(endX, transform.position.y, endZ);

        //Targets
        float calculatedY = Terrain.activeTerrain.SampleHeight(new Vector3(targetX, 0, targetZ));
        Vector3 mainTargetPoint = new Vector3(targetX, calculatedY, targetZ);
        centerTargetTrigger = new Vector3(targetX, calculatedY + heightOffset_end, targetZ);

        float adjustment1 = airForceAdjustment;

        //T
        if (targetMode == TargetMode.QUAD_TARGET)
        {
            targets = new Vector3[4];
            targets[0] = mainTargetPoint + new Vector3(adjustment1, 0, -adjustment1);
            targets[1] = mainTargetPoint + new Vector3(-adjustment1, 0, -adjustment1);
            targets[2] = mainTargetPoint + new Vector3(adjustment1, 0, adjustment1);
            targets[3] = mainTargetPoint + new Vector3(-adjustment1, 0, adjustment1);
        }
        else if (targetMode == TargetMode.LINE)
        {
            targets = new Vector3[weapons.Length];
            float targetsLengthF = targets.Length;
            for (int i = 0; i < targets.Length; i++)
            {
                float zOffset = (i - targetsLengthF / 2f) * weaponTargetOffset;
                targets[i] = mainTargetPoint + new Vector3(0, 0, zOffset);
            }
        }
        else if (targetMode == TargetMode.CIRCLE)
        {
            targets = new Vector3[weapons.Length];
            float targetsLengthF = targets.Length;
            for (int i = 0; i < targets.Length; i++)
            {
                float partAngle = i / targetsLengthF * 360f;
                float xOffset = Mathf.Sin(partAngle) * weaponTargetOffset;
                float zOffset = Mathf.Cos(partAngle) * weaponTargetOffset;
                targets[i] = mainTargetPoint + new Vector3(xOffset, 0, zOffset);
            }
        }
        else if (targetMode == TargetMode.SQUARES_2x4)
        {
            targets = new Vector3[8];
            adjustment1 /= 2.53f;
            targets[0] = mainTargetPoint + new Vector3(adjustment1, 0, adjustment1 * 3f);
            targets[1] = mainTargetPoint + new Vector3(adjustment1, 0, adjustment1);
            targets[2] = mainTargetPoint + new Vector3(adjustment1, 0, -adjustment1);
            targets[3] = mainTargetPoint + new Vector3(adjustment1, 0, -adjustment1 * 3f);

            targets[4] = mainTargetPoint + new Vector3(-adjustment1, 0, adjustment1 * 3f);
            targets[5] = mainTargetPoint + new Vector3(-adjustment1, 0, adjustment1);
            targets[6] = mainTargetPoint + new Vector3(-adjustment1, 0, -adjustment1);
            targets[7] = mainTargetPoint + new Vector3(-adjustment1, 0, -adjustment1 * 3f);
        }

        //Spawn weapoins, if needed
        if (spawnRandomWeapons)
        {
            weapons = new Projectile[spawnWeaponCount];
            targets = new Vector3[spawnWeaponCount];
            for (int i = 0; i < spawnWeaponCount; i++)
            {
                Projectile randomSelectedWeapon = weaponPrefabs[UnityEngine.Random.Range(0, weaponPrefabs.Length)];
                float randomAngle = UnityEngine.Random.Range(0f, 360f);
                float randomX = Mathf.Sin(randomAngle) * weaponTargetOffset * UnityEngine.Random.Range(0.2f, 1);
                float randomZ = Mathf.Cos(randomAngle) * weaponTargetOffset * UnityEngine.Random.Range(0.2f, 1);
                Vector3 randomCirclePoint = new Vector3(randomX, transform.position.y, randomZ) + centerTargetTrigger;
                weapons[i] = Instantiate(randomSelectedWeapon.gameObject, randomCirclePoint, Quaternion.identity).GetComponent<Projectile>();
                targets[i] = randomCirclePoint;
            }
        }

        //Set air force material for weapons, if needed
        if (spawnRandomWeapons)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].gameObject.GetComponent<MeshRenderer>().material.mainTexture = team.textureUnit;
            }
        }

        //Shuffle weapons
        if (shuffleWeaponOrder)
            shuffleWeapons();

        //Set target for weapons
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].prepare(this, targets[i], weaponBlast, weaponCoreBlast, weaponDamage, false, weaponIsFreefall);
        }

        launched = true; //Go
    }
    #endregion
    #region [Functions] Weapon manipulating (Shuffling)
    public void shuffleWeapons()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(0, weapons.Length);
            Projectile tempGO = weapons[rnd];
            weapons[rnd] = weapons[i];
            weapons[i] = tempGO;
        }
    }
    #endregion
    #region [Functions] Carrybus landing animation
    IEnumerator land_and_spawn_animation()
    {
        //DESCENDING

        //Nose up
        animRoot.clip = animTargetApproach;
        animRoot.Play(PlayMode.StopAll);

        targettedSpeed = 0;
        //targettedOffrotZ = -30;
        offsetYAcceleration = 0.3f;
        targettedSpeedY = -5f;
        yield return new WaitForSeconds(2);

        //Descend faster
        //targettedOffrotZ = -15;
        offsetYAcceleration = 0.8f;
        targettedSpeedY = -3.5f;
        yield return new WaitForSeconds(1.75f);

        //targettedOffrotZ = 0;
        offsetYAcceleration = 0.8f;
        targettedSpeedY = -0.1f;
        yield return new WaitForSeconds(1.5f);

        //UNITS OUT
        bottomDoorsOpened = true;
        yield return new WaitForSeconds(2);

        offsetYAcceleration = 40f;
        targettedSpeedY = 0f;
        int randomUnitSet = UnityEngine.Random.Range(0, CARRYBUS_UNIT_SETS.GetLength(0));
        for (int i = 0; i < targets.Length; i++)
        {
            if (TEMPORARY_CARRYBUS_UNIT != "")
            {
                UnitSerializable unitSer = GlobalList.units[TEMPORARY_CARRYBUS_UNIT].serializeUnit();
                unitSer.setI(KEY_UNIT_TEAMID, team.id);
                unitSer.setF(KEY_UNIT_HP, 1f);

                Unit newUnit = MapLevel.spawnUnit(unitSer);
                newUnit.body.warp(targets[i] + new Vector3(0, 0.15f, 0));
            }
            else
            {
                if (string.IsNullOrEmpty(CARRYBUS_UNIT_SETS[randomUnitSet, i]))
                    continue;

                UnitSerializable unitSer = GlobalList.units[CARRYBUS_UNIT_SETS[randomUnitSet, i]].serializeUnit();
                unitSer.setI(KEY_UNIT_TEAMID, team.id);
                unitSer.setF(KEY_UNIT_HP, 1f);

                Unit newUnit = MapLevel.spawnUnit(unitSer);
                newUnit.body.warp(targets[i] + new Vector3(0, 0.15f, 0));
            }
        }
        yield return new WaitForSeconds(3.5f);

        //ASCENDING
        animRoot.clip = animTargetLeave;
        animRoot.Play(PlayMode.StopAll);

        bottomDoorsOpened = false;
        offsetYAcceleration = 0.8f;
        targettedSpeedY = 0.1f;
        yield return new WaitForSeconds(2.3f);

        offsetYAcceleration = 0.8f;
        targettedSpeedY = 3.5f;
        yield return new WaitForSeconds(1.75f);

        targettedSpeed = speedNormal;
        offsetYAcceleration = 0.3f;
        targettedSpeedY = 5f;
        yield return new WaitForSeconds(2);

        targettedSpeedY = 0f;
    }
    #endregion

    #region [Overriden functions] onLoad
    public override void onLoad()
    {
        base.onLoad();
        assignGameplayProperties(this, typeof(AirForce), "UnitData/AirForceData");
    }
    #endregion

    #region [Enum] Approach type & Action state & Action Type & Target mode
    public enum ApproachType
    {
        FLYBY = 0,
        SLOWDOWN = 1,
        STOP = 2,
        LAND = 3
    }

    public enum ActionState
    {
        MOVING,
        PERFOMING,
        PERFOMED
    }

    public enum ActionType
    {
        DESTROY,
        SPAWN
    }

    public enum TargetMode
    {
        QUAD_TARGET = 0,
        LINE = 1,
        CIRCLE = 2,
        SQUARES_2x4 = 3,
    }
    #endregion

    #region [Extra] Carrybus - list of units patterns
    /*MAPPING:
    * [1] [2] [3] [4]
    * [5] [6] [7] [8]
    * 
    * empty = no unit
    */
    public static readonly string[,] CARRYBUS_UNIT_SETS = new string[,] {
        { "", "0_tonk1", "0_tonk1", "0_flamingo1", "", "0_tonk1", "0_tonk1", "0_flamingo1" },
        { "", "0_tonk2", "0_flamingo2", "", "", "0_tonk2", "0_flamingo2", "" },
        { "", "0_antiAircraft2", "0_antiAircraft2", "", "", "0_antiAircraft2", "0_antiAircraft2", "" },
        { "0_antiAircraft1", "0_antiAircraft1", "0_antiAircraft1", "0_antiAircraft1", "0_antiAircraft1", "0_antiAircraft1", "0_antiAircraft1", "0_antiAircraft1" },
        { "", "0_tonk1", "0_longarm1", "0_antiAircraft2", "", "0_tonk1", "0_longarm1", "0_antiAircraft2" },
        { "0_tonk1", "0_tonk1", "0_flamingo1", "0_flamingo1", "0_tonk1", "0_tonk1", "0_flamingo1", "0_flamingo1" },
        { "", "0_longarm2", "0_heavy1", "", "", "0_tonk2", "0_flamingo2", "" },
        { "", "0_flamingo1", "0_flamingo1", "0_heavy2", "", "0_flamingo1", "0_flamingo1", "0_heavy2" },
        { "", "0_longarm1", "0_heavy1", "0_tonk2", "", "0_longarm1", "0_heavy1", "0_tonk2" },
        { "", "0_antiAircraft1", "0_antiAircraft1", "0_longarm2", "", "0_antiAircraft1", "0_antiAircraft1", "0_longarm2" }
    };
    #endregion
}