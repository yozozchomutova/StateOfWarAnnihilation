#region [Libraries] All libraries
using System;
using UnityEngine;
using static GroundUnit;
#endregion

public class SMF : Unit
{
    #region [Variables] Gameplay properties
    /// <summary> Height that specifies distance between terrain point underneath SMF and SMF itself </summary>
    [HideInInspector] public float floatingHeight;
    /// <summary> Maximal speed </summary>
    [HideInInspector] public float moveMaxSpeed;
    /// <summary> How fast will it change it's speed </summary>
    [HideInInspector] public float moveAcceleration;

    /// <summary> How fast can SMF destroy stuff</summary>
    [HideInInspector] public float destroyRate;
    /// <summary> How fast can SMF upgrade stuff</summary>
    [HideInInspector] public float upgradeRate;
    /// <summary> How fast can SMF repair stuff</summary>
    [HideInInspector] public float repairRate;

    //Side weapon - Weapon, that is used to attack ground units
    /// <summary> Cooldown of side weapon.</summary>
    [HideInInspector] public float sideWeaponCooldownDefault;
    /// <summary> Damage of side weapon.</summary>
    [HideInInspector] public float sideWeaponDamage;
    /// <summary> Range of side weapon.</summary>
    [HideInInspector] public float sideWeaponRange;
    /// <summary> Assigned projectile for side weapon.</summary>
    [Header("Side weapon")]
    public Projectile sideWeaponProjectile;
    /// <summary> Where to output side weapon.</summary>
    public Transform sideWeaponOutput;

    /// <summary> Default value of idle alert time, which tells how long can SMF stand still without any popup notification to player.</summary>
    [HideInInspector] public float idleAlertTimeDefault;
    #endregion
    #region [Variables] Runtime properties
    /// <summary> Moving velocity of SMF </summary>
    [HideInInspector] private Vector3 velocity;

    /// <summary> Cooldown of side weapon.</summary>
    [HideInInspector] private float sideWeaponCooldown;
    /// <summary> Target unit by side weapon.</summary>
    [HideInInspector] private Unit sideWeaponTarget;

    /// <summary> Tells how long can SMF stand still without any popup notification to player.</summary>
    [HideInInspector] private float idleAlertTime;
    #endregion
    #region [Variables] SMF State
    /// <summary> Tells what is currently SMF doing </summary>
    [HideInInspector] public State state;

    /// <summary> Point, where SMF is currently heading to </summary>
    private Vector3 moveToPosition;
    /// <summary> Unit, which SMF is going something to do about it (check taskOnTargetUnit variable)</summary>
    private Unit targetUnit;
    /// <summary> What to do with targetted unit (when SMF arrives)</summary>
    [HideInInspector] public State taskOnTargetUnit;
    #endregion
    #region [Variables] SMF Effects
    [Header("Effect prefabs")]

    /// <summary> Point, where effect should spawn/happen </summary>
    [Tooltip("Point, where effect should spawn/happen")]
    public Transform effectOutputPos;

    /// <summary> Effect, when SMF destroys something </summary>
    [Tooltip("Effect, when SMF destroys something")]
    public SMF_Effect prefabEffectDestroy;
    /// <summary> Effect, when SMF repairs something </summary>
    [Tooltip("Effect, when SMF repairs something")]
    public SMF_Effect prefabEffectRepair;
    /// <summary> Effect, when SMF upgrades something </summary>
    [Tooltip("Effect, when SMF upgrades something")]
    public SMF_Effect prefabEffectUpgrade;

    /// <summary> Ëffect that is currently happening right now. </summary>
    private SMF_Effect effectCurrent;
    #endregion

    #region [Functions] frameUpdate()
    public override void frameUpdate()
    {
        if (sideWeaponCooldown > 0)
            sideWeaponCooldown -= Time.deltaTime;

        if (sideWeaponTarget != null && sideWeaponCooldown <= 0)
        {
            sideWeaponCooldown = sideWeaponCooldownDefault;

            Projectile proj1 = Instantiate(sideWeaponProjectile.gameObject, sideWeaponOutput).GetComponent<Projectile>();
            proj1.prepare(this, sideWeaponTarget, 0.1f, 0.1f, sideWeaponDamage, true);
            proj1.launch(0);
        }

        //Moving
        if (state == State.IDLE)
        {
            if (LevelData.tsCmp(team.id))
            {
                idleAlertTime -= Time.deltaTime;
                if (idleAlertTime <= 0)
                {
                    GameLevelUIController.logMessage("SMF is Idle", true, gameObject.transform.position);
                    idleAlertTime = idleAlertTimeDefault;
                }
            }
        }
        else if (state == State.MOVING)
        {
            Vector3 direction = moveToPosition - transform.position;
            if (Mathf.Abs(direction.x) > 1f || Mathf.Abs(direction.y) > 1f || Mathf.Abs(direction.z) > 1)
                direction = direction.normalized;

            velocity += (direction - velocity) * moveAcceleration * Time.deltaTime;

            transform.position += velocity * moveMaxSpeed * Time.deltaTime;

            //Arrived to the destination?
            if (Vector3.Distance(transform.position, moveToPosition) <= 0.01f)
            {
                velocity = Vector3.zero;
                if (targetUnit != null)
                    state = taskOnTargetUnit;
                else
                    state = State.IDLE;
            }
        }
        else if (state == State.DESTROYING)
        {
            if (effectCurrent == null || effectCurrent.usage != State.DESTROYING)
            {
                startSMFEffect(prefabEffectDestroy);
            }
            else
            {
                //Unit still alive?
                if (targetUnit != null)
                    targetUnit.damage(team.id, destroyRate * Time.deltaTime); //Damage active unit
                else
                    commandStop();
            }
        }
        else if (state == State.UPGRADING)
        {
            if (effectCurrent == null || effectCurrent.usage != State.UPGRADING)
            {
                startSMFEffect(prefabEffectUpgrade);
            }
            else
            {
                //Unit still alive? Is it already upgraded?
                if (targetUnit != null && targetUnit.canBeUpgraded(this))
                    (targetUnit as BuildingUnit).upgrade(this, upgradeRate); //Upgrade
                else
                    commandStop();
            }
        }
        else if (state == State.REPAIRING)
        {
            if (effectCurrent == null || effectCurrent.usage != State.REPAIRING)
            {
                startSMFEffect(prefabEffectRepair);
            }
            else
            {
                //Unit still alive? Is it already repaired?
                if (targetUnit != null && targetUnit.canBeRepaired(this))
                    (targetUnit as BuildingUnit).repair(this, repairRate); //Repair
                else
                    commandStop();
            }
        }
    }
    #endregion
    #region [Functions] Locating enemies
    private Collider[] nearUnitsColliders = new Collider[40];
    private void locateEnemies()
    {
        int nearEnemiesCount = Physics.OverlapSphereNonAlloc(gameObject.transform.position, sideWeaponRange, nearUnitsColliders, 1 << 14);
        for (int i = 0; i < nearEnemiesCount; i++)
        {
            Unit unit = Unit.getUnit(nearUnitsColliders[i]);

            if (unit == null)
                continue;

            if (unit.team != team/* && !unit.holographic*/) //Is enemy? Is holographic? (ex. In building mode)
            {
                if (unit.id == "0_smf1")
                {
                    sideWeaponTarget = unit;
                    return;
                }

                switch (unit.id)
                {
                    case "0_smf1":
                    case "0_antiAircraft1":
                    case "0_antiAircraft2":
                    case "0_antiAircraft3":
                        sideWeaponTarget = unit;
                        return;
                }
            }
        }

        sideWeaponTarget = null;
    }
    #endregion
    #region [Functions] Commands
    public override void commandStop()
    {
        state = State.IDLE;
        taskOnTargetUnit = State.IDLE;
        targetUnit = null;
        endSMFEffect();
        idleAlertTime = idleAlertTimeDefault;
    }
    public void commandMove(float x, float z)
    {
        float calculatedY = LevelData.mainTerrain.SampleHeight(new Vector3(x, 0, z)) + floatingHeight;
        moveToPosition = new Vector3(x, calculatedY, z);
        state = State.MOVING;
        taskOnTargetUnit = State.IDLE;
        targetUnit = null;
        endSMFEffect();
    }

    private void commandTargetUnit(Unit unit, State taskOnUnit)
    {
        if (unit.unitType == UnitType.BUILDING || unit.unitType == UnitType.TOWER)
        {
            commandMove(unit.transform.localPosition.x, unit.transform.localPosition.z);
            targetUnit = unit;
            taskOnTargetUnit = taskOnUnit;
        }
    }

    public void commandDestroyUnit(Unit unit)
    {
        commandTargetUnit(unit, State.DESTROYING);
    }

    public void commandRepairUnit(Unit unit)
    {
        commandTargetUnit(unit, State.REPAIRING);
    }

    public void commandUpgradeUnit(Unit unit)
    {
        commandTargetUnit(unit, State.UPGRADING);
    }
    #endregion
    #region [Functions] SMF Effects
    private void startSMFEffect(SMF_Effect smf_e)
    {
        endSMFEffect();
        effectCurrent = Instantiate(smf_e.gameObject, effectOutputPos.position, Quaternion.identity).GetComponent<SMF_Effect>();
        effectCurrent.begin();
    }

    private void endSMFEffect()
    {
        if (effectCurrent != null)
        {
            Destroy(effectCurrent.gameObject, 2);
            effectCurrent.stop();
            effectCurrent = null;
        }
    }
    #endregion

    #region [Overriden functions] onLoad
    public override void onLoad()
    {
        base.onLoad();
        assignGameplayProperties(this, typeof(SMF), "UnitData/SMFData");
    }
    #endregion
    #region [Overriden functions] onInit
    public override void onInit(Vector3 position, Vector3 rotation)
    {
        base.onInit(position, rotation);

        if (LevelData.scene == LevelData.Scene.GAME)
        {
            commandMove(position.x, position.z);
            InvokeRepeating("locateEnemies", UnityEngine.Random.Range(0.1f, 2f), 0.5f);
        }
    }
    #endregion

    #region [Enums] SMF States
    public enum State
    {
        IDLE,
        MOVING,
        DESTROYING,
        UPGRADING,
        REPAIRING
    }
    #endregion
}