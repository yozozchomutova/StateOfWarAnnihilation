#region [Libraries] All libraries
using System;
using System.Collections;
using UnityEngine;
using static GlobalList;
using static GroundUnit;
using static Unit;
#endregion

public abstract class AttackingUnit : Unit
{
    #region [Variables] State
    /// <summary> Tells, what is unit currently doing</summary>
    [HideInInspector] public UnitState state;
    #endregion
    #region [Variables] Gameplay properties
    /// <summary> Can unit attack other ground units </summary>
    [HideInInspector] public bool attacksGround;
    /// <summary> Can unit attack other air </summary>
    [HideInInspector] public bool attacksAir;

    //Projectile
    [Header("Projectile")]
    /// <summary> GameObject prefab of projectile </summary>
    public GameObject projectilePrefab;
    /// <summary>  </summary>
    [HideInInspector] public float projectileDamage;
    /// <summary>  </summary>
    [HideInInspector] public float projectileFireDelayBase;
    /// <summary>  </summary>
    [HideInInspector] public float projectileFireDelaySpacing;
    /// <summary>  </summary>
    [HideInInspector] public bool projectileIsFreefall;
    /// <summary>  </summary>
    [HideInInspector] public bool projectileInstantDamage;
    /// <summary>  </summary>
    [HideInInspector] public float projectileFireSpread;
    /// <summary>  </summary>
    [HideInInspector] public float projectilePeakHeight;
    /// <summary>  </summary>
    [HideInInspector] public float projectileFireForce;

    //Firing
    /// <summary> Current fire cooldown </summary>
    [HideInInspector] public float fireCooldownDefault;
    /// <summary> Weapon range of unit </summary>
    [HideInInspector] public float fireRange;
    /// <summary> Weapon range of unit </summary>
    [HideInInspector] public bool dieToFire;
    /// <summary> Weapon range of unit </summary>
    [HideInInspector] public bool autoAttack;

    //Projectile explosion
    /// <summary> Radius of damage INSIDE </summary>
    [HideInInspector] public float radiusBlastCore;
    /// <summary> Radius of damage OUTSIDE </summary>
    [HideInInspector] public float radiusBlastOutter;

    //Motion
    /// <summary> Weapon range of unit </summary>
    [HideInInspector] public float rotateSpeed;
    /// <summary> Cooldown of random head rotations to happen when unit is idle. (Used for RESET, minimal value) </summary>
    [HideInInspector] private float idleRotationCooldownDefaultMin = 7;
    /// <summary> Cooldown of random head rotations to happen when unit is idle. (Used for RESET, maximal value) </summary>
    [HideInInspector] private float idleRotationCooldownDefaultMax = 12;
    #endregion
    #region [Variables] Runtime properties
    //Firing
    /// <summary> Current fire cooldown </summary>
    [HideInInspector] public float fireCooldown;

    //Motion
    /// <summary> Requested unit rotation of on X axis </summary>
    [HideInInspector] public float targetedRotationX = 0f;
    /// <summary> Requested unit rotation of on X axis </summary>
    [HideInInspector] public float targetedRotationY = 0f;
    /// <summary> Rotation of unit on X axis </summary>
    [HideInInspector] public float rotationX;
    /// <summary> Rotation of unit on Y axis </summary>
    [HideInInspector] public float rotationY;

    //Idle motion
    /// <summary> Cooldown of random head rotations to happen when unit is idle </summary>
    [HideInInspector] private float idleRotationCooldown;
    #endregion
    #region [Variables] On game start properties
    /// <summary> Requested unit rotation of on X axis </summary>
    [HideInInspector] public float parentRotationY = 0f;
    #endregion
    #region [Variables] Targetted units state
    /// <summary> Which unit is currently being focused for attack</summary>
    [HideInInspector] public Unit targetUnitCurrent;
    /// <summary> Forced unit has 1st priority. It's being attacked at all cost </summary>
    [HideInInspector] public Unit targetUnitForce;
    /// <summary> Primary unit has 2nd priority. If no better target is found near, primary unit is being focused </summary>
    [HideInInspector] public Unit targetUnitPrimary;
    /// <summary> Idle unit is automatically selected when near unit. It has least priority (3rd)</summary>
    [HideInInspector] public Unit targetUnitAuto;
    #endregion
    #region [Variables] Transforms
    [Header("Transforms")]
    /// <summary> GameObject that identifies as unit head that will be rotated</summary>
    [Tooltip("GameObject that identifies as unit head that will be rotated")]
    public Transform unitHead;
    #endregion
    #region [Variables] Firing VFX + SFX
    [Header("Firing VFX + SFX")]
    /// <summary> Particle system that is played, when unit fires shot at something. </summary>
    public ParticleSystem psFire;

    /// <summary> Sound that is played, when unit fires shot at something. </summary>
    public AudioSource sfxFire;

    /// <summary> Sound that is played, when unit fires shot at something. </summary>
    public Transform[] projectileOutput;
    /// <summary> Sound that is played, when unit fires shot at something. </summary>
    public GameObject[] projectileOutput_light;
    /// <summary> Sound that is played, when unit fires shot at something. </summary>
    public GameObject[] spinGuns;
    #endregion

    #region [Functions] frameUpdate()
    public override void frameUpdate()
    {
        //Debug.Log("State: " + state.ToString());
        //Decide active target unit
        if (targetUnitForce != null && Vector3.Distance(transform.position, targetUnitForce.transform.position) <= fireRange)
        {
            targetUnitCurrent = targetUnitForce;
        }
        else if (targetUnitPrimary != null && Vector3.Distance(transform.position, targetUnitPrimary.transform.position) <= fireRange)
        {
            if (targetUnitAuto == null || targetUnitAuto.unitType <= targetUnitPrimary.unitType)
                targetUnitCurrent = targetUnitPrimary;
        }
        else
        {
            targetUnitCurrent = targetUnitAuto;
        }

        //Rotating tanks head
        parentRotationY = body.transform.localEulerAngles.y;

        float realTankRotateSpeed = rotateSpeed * Time.deltaTime;
        bool rightRotateDirection = false;

        if (rotationY >= 0)
        {
            float minusCurRotation = rotationY - 180;

            if (targetedRotationY > rotationY && targetedRotationY > minusCurRotation ||
                targetedRotationY < rotationY && targetedRotationY < minusCurRotation)
                rightRotateDirection = true;
        }
        else
        {
            float plusCurRotation = rotationY + 180;

            if (targetedRotationY > rotationY && targetedRotationY < plusCurRotation ||
                targetedRotationY < rotationY && targetedRotationY > plusCurRotation)
                rightRotateDirection = true;
        }

        //Apply rotation Y
        if (rotationY > targetedRotationY + realTankRotateSpeed || rotationY < targetedRotationY - realTankRotateSpeed)
            rotationY += rightRotateDirection ? realTankRotateSpeed : -realTankRotateSpeed;

        //Apply rotation X
        rotationX += (targetedRotationX - rotationX) * Time.deltaTime * 10f;

        //Bounds
        if (rotationY > 180)
            rotationY -= 360;
        else if (rotationY < -180)
            rotationY += 360;

        //Apply rotation to gameObject
        unitHead.transform.localRotation = Quaternion.Euler(-90 - rotationX, rotationY, 0);

        //Targetting cooldown
        if (fireCooldown > 0)
            fireCooldown -= Time.deltaTime;

        //Generate random tank head movement
        if (state == UnitState.IDLE)
        {
            idleRotationCooldown -= Time.deltaTime;
            if (psFire != null)
                psFire.Stop();

            if (idleRotationCooldown < 0)
            {
                targetedRotationY = UnityEngine.Random.Range(-180, 180);
                idleRotationCooldown = UnityEngine.Random.Range(idleRotationCooldownDefaultMin, idleRotationCooldownDefaultMax);
            }
        }
        else if (state == UnitState.TARGETING)
        {
            if (targetUnitCurrent != null)
            {
                targetedRotationY = Mathf.Atan2(targetUnitCurrent.transform.position.x - transform.position.x,
                        targetUnitCurrent.transform.position.z - transform.position.z) * Mathf.Rad2Deg - parentRotationY;

                //Fix targetted Y
                if (targetedRotationY < -180)
                    targetedRotationY += 360;
                else if (targetedRotationY > 180)
                    targetedRotationY -= 360;

                float XZdistance = Vector2.Distance(new Vector2(targetUnitAuto.transform.position.x, targetUnitAuto.transform.position.z), new Vector2(transform.position.x, transform.position.z));
                targetedRotationX = Mathf.Atan2(targetUnitAuto.transform.position.y - transform.position.y, XZdistance) * Mathf.Rad2Deg;

                foreach (GameObject spinGun in spinGuns)
                {
                    spinGun.transform.Rotate(Time.deltaTime * 450f,0 , 0);
                }

                //Reloaded? + wait till head is properly rotated and ready
                if (fireCooldown <= 0 && Mathf.Abs(rotationY - targetedRotationY) < 3f)
                {
                    fireCooldown = fireCooldownDefault;
                    if (psFire != null)
                        psFire.Play();

                    if (projectileInstantDamage)
                    {
                        if (sfxFire != null)
                            sfxFire.Play();

                        for (int i = 0; i < projectileOutput.Length; i++)
                        {
                            targetUnitAuto.damage(team.id, projectileDamage);
                        }

                        for (int i = 0; i < projectileOutput_light.Length; i++)
                        {
                            if (projectileOutput_light[i] != null)
                            {
                                StartCoroutine(toggleGO_afterDelay(projectileOutput_light[i], true, 0f));
                                StartCoroutine(toggleGO_afterDelay(projectileOutput_light[i], false, 0.1f));
                            }
                        }
                    }
                    else if (dieToFire)
                    {
                        if (sfxFire != null)
                            sfxFire.Play();

                        destroyUnit(0);
                    }
                    else
                    {
                        float projDelay = projectileFireDelayBase;
                        for (int i = 0; i < projectileOutput.Length; i++)
                        {
                            Projectile proj1 = Instantiate(projectilePrefab.gameObject, projectileOutput[i]).GetComponent<Projectile>();

                            if (projectileIsFreefall)
                            {
                                proj1.setSFX(sfxFire);
                                proj1.prepare(this, targetUnitAuto.transform.position, radiusBlastOutter, radiusBlastCore, projectileDamage, true, true);
                                proj1.enableZOffsetting(projectilePeakHeight * Vector3.Distance(targetUnitAuto.transform.position, transform.position), transform.position, targetUnitAuto.transform.position);
                                proj1.launch((targetUnitAuto.transform.position - transform.position) * projectileFireForce + RandomSpread(-projectileFireSpread, projectileFireSpread), projDelay);
                            }
                            else
                            {
                                proj1.setSFX(sfxFire);
                                proj1.prepare(this, targetUnitAuto, radiusBlastOutter, radiusBlastCore, projectileDamage, true);
                                proj1.launch(projDelay);
                            }

                            projDelay += projectileFireDelaySpacing;
                        }
                    }
                }
            }
        }
    }
    #endregion
    #region [Functions] Utils
    private IEnumerator toggleGO_afterDelay(GameObject go, bool toggle, float waitSeconds)
    {
        yield return new WaitForSeconds(waitSeconds);
        go.SetActive(toggle);
    }

    public static Vector3 RandomSpread(float min, float max)
    {
        return new Vector3(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(min, max));
    }
    #endregion
    #region [Functions] Locating enemies
    private static Collider[] nearUnitsColliders = new Collider[40];
    private static Unit[] nearUnits = new Unit[40];
    private static UnitTypeComparer unitTypeComparer = new UnitTypeComparer();

    private void locateEnemies()
    {
        int foundUnits = Physics.OverlapSphereNonAlloc(gameObject.transform.position, fireRange, nearUnitsColliders, 1 << 14);
        Array.Clear(nearUnits, 0, nearUnits.Length);
        for (int i = 0; i < foundUnits; i++)
        {
            Unit unit = getUnit(nearUnitsColliders[i]);

            nearUnits[i] = unit;
        }

        Array.Sort(nearUnits, unitTypeComparer);

        for (int i = 0; i < foundUnits; i++)
        {
            Unit unit = nearUnits[i];

            if (unit == null)
                continue;

            if (unit.team != team && unit.virtualSpace == VirtualSpace.NORMAL) //Is enemy? Is holographic? Is valid? (ex. In building mode)
            {
                if (attacksGround && !(unit is SMF || unit is AirForce))
                {
                    targetUnitAuto = unit;
                    state = UnitState.TARGETING;
                    return;
                }
                else if (attacksAir)
                {
                    if (unit is SMF)
                    {
                        if (checkAirUnitAngle(unit))
                            return;
                    }
                    else if (unit is AirForce)
                    {
                        if ((unit as AirForce).canBeShotDown)
                            if (checkAirUnitAngle(unit))
                                return;
                    }
                }
            }
        }

        targetUnitAuto = null;
        state = UnitState.IDLE;
        targetedRotationX = 0f;
    }

    private bool checkAirUnitAngle(Unit unit)
    {
        //Unit can't be too above turret = no sight
        float XZdistance = Vector2.Distance(new Vector2(unit.transform.position.x, unit.transform.position.z), new Vector2(transform.position.x, transform.position.z));
        float calculatedAngle = Mathf.Atan2(unit.transform.position.y - transform.position.y,
                XZdistance) * Mathf.Rad2Deg;

        if (calculatedAngle < 80)
        {
            targetUnitAuto = unit;
            state = UnitState.TARGETING;
            return true;
        }

        return false;
    }
    #endregion
    #region [Functions] Receive controlling / commanding

    #endregion

    #region [Overriden functions] onLoad
    public override void onLoad()
    {
        base.onLoad();
        assignGameplayProperties(this, typeof(AttackingUnit), "UnitData/AttackingUnitData");
    }
    #endregion
    #region [Overriden functions] onInit
    public override void onInit(Vector3 position, Vector3 rotation)
    {
        base.onInit(position, rotation);

        if (LevelData.scene == LevelData.Scene.GAME)
        {
            //InvokeRepeating("locateEnemies", UnityEngine.Random.Range(0.1f, 2f), 0.5f);
        }
    }
    #endregion
}