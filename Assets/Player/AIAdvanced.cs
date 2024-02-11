using System;
using System.Collections.Generic;
using UnityEngine;

public class AIAdvanced : TeamDriver
{
    [Header("Bot settings")]
    public float smfUpgradeMaxDistance;
    public float smfRepairMaxDistance;
    public float smfAttackChance;
    public float airForceSendChance;
    public float airForceSendCheckRange;
    public float buildingProtectChance;
    public float buildingProtectCheckRange;
    public int buildingTowerAttempts;
    public float buildingTowerMaxDistance;
    public float unitTargetFindingRange;
    public float carrybusSendDistance;

    private Unit grenaderTower;
    private Unit machineGunTower;
    private Unit plasmaticTower;
    private Unit antiAircratTower;
    private Unit antTower;

    private Collider[] unitColliders = new Collider[100];

    public override void onInit()
    {
        grenaderTower = GlobalList.units["0_granaderT1"];
        machineGunTower = GlobalList.units["0_machineGunT1"];
        plasmaticTower = GlobalList.units["0_plasmaticT1"];
        antiAircratTower = GlobalList.units["0_antiAircraftT1"];
        antTower = GlobalList.units["0_antT1"];
    }

    public override void onUpdate()
    {
        if (!LevelData.teamStats[controllingTeam.id].activeTeam)
            return;

        for (int i = 0; i < LevelData.units.Count; i++)
        {
            Unit u = LevelData.units[i];
            if (u.team == controllingTeam)
            {
                if (u is SMF)
                {
                    SMF smf = u as SMF;
                    if (smf.state == SMF.State.IDLE)
                    {
                        //Find work for SMF
                        //1. Check if any near building can be upgraded
                        Unit unitToUpgrade = null;
                        int unitUpgradeArrayIndex = -1;
                        float requiredDistanceUpgrade = smfUpgradeMaxDistance;

                        Unit unitToRepair = null;
                        int unitRepairArrayIndex = -1;
                        float requiredDistanceRepair = smfRepairMaxDistance;

                        Unit unitToAttack = null;
                        int unitAttackArrayIndex = -1;
                        float requiredDistanceAttack = 999999;

                        for (int j = 0; j < LevelData.units.Count; j++)
                        {
                            Unit unit = LevelData.units[j];
                            if (unit.team == controllingTeam)
                            {
                                float distance = Vector3.Distance(smf.transform.position, unit.transform.position);
                                if (distance < requiredDistanceUpgrade && unit.canBeUpgraded(smf))
                                {
                                    unitToUpgrade = unit;
                                    unitUpgradeArrayIndex = j;
                                    requiredDistanceUpgrade = distance;
                                } else if (distance < requiredDistanceRepair && unit.canBeRepaired(smf))
                                {
                                    unitToRepair = unit;
                                    unitRepairArrayIndex = j;
                                    requiredDistanceRepair = distance;
                                }
                            } else if (unit.unitType == Unit.UnitType.BUILDING || unit.unitType == Unit.UnitType.TOWER)
                            {
                                float distance = Vector3.Distance(smf.transform.position, unit.transform.position);
                                float attackUnitChance = UnityEngine.Random.Range(0f, 1f);
                                if (distance < requiredDistanceAttack && attackUnitChance <= smfAttackChance)
                                {
                                    unitToAttack = unit;
                                    unitAttackArrayIndex = j;
                                    requiredDistanceAttack = distance;
                                }
                            }
                        }

                        float distanceToRepairUnit = 0, distanceToAttackUnit = 0;
                        if (unitToRepair != null && unitToAttack != null)
                        {
                            distanceToRepairUnit = Vector3.Distance(unitToRepair.transform.position, transform.position);
                            distanceToAttackUnit = Vector3.Distance(unitToAttack.transform.position, transform.position);
                        }

                        if (unitToUpgrade != null) //Was found anything to upgrade?
                            smf.commandUpgradeUnit(unitToUpgrade);
                        else if (unitToRepair != null && (distanceToRepairUnit <= distanceToAttackUnit)) //Was found anything to repair? (Must be closer)
                            smf.commandRepairUnit(unitToRepair);
                        else if (unitToAttack != null) //Was found anything to attack?
                            smf.commandDestroyUnit(unitToAttack);
                    }
                } else if (u.unitType == Unit.UnitType.BUILDING) {
                    //Protect building?
                    float chance = UnityEngine.Random.Range(0f, 1f);
                    if (chance <= buildingProtectChance)
                    {
                        Collider[] unitsInGroup = Physics.OverlapSphere(u.transform.position, buildingProtectCheckRange, 1 << 14);

                        for (int k = 0; k < unitsInGroup.Length; k++)
                        {
                            Unit checkingUnit = Unit.getUnit(unitsInGroup[k]);
                            if (checkingUnit.team != controllingTeam)
                            {
                                if (checkingUnit.unitType == Unit.UnitType.SMF)
                                {
                                    //Build antiair towers around
                                    if (LevelData.teamStats[controllingTeam.id].antiAircraft && LevelData.teamStats[controllingTeam.id].money >= antiAircratTower.value * 2)
                                    {
                                        buildTowersNear(u.transform.position, antiAircratTower, 2, buildingTowerAttempts, buildingTowerMaxDistance);
                                    }
                                    break;
                                } else if (checkingUnit.unitType == Unit.UnitType.UNIT ||
                                    checkingUnit.unitType == Unit.UnitType.STEPPER ||
                                    checkingUnit.unitType == Unit.UnitType.TOWER)
                                {
                                    //Build ground towers around
                                    if (LevelData.teamStats[controllingTeam.id].granader && LevelData.teamStats[controllingTeam.id].money >= grenaderTower.value * 2)
                                    {
                                        buildTowersNear(u.transform.position, grenaderTower, 2, buildingTowerAttempts, buildingTowerMaxDistance);
                                    } else if (LevelData.teamStats[controllingTeam.id].machineGun && LevelData.teamStats[controllingTeam.id].money >= machineGunTower.value * 2)
                                    {
                                        buildTowersNear(u.transform.position, machineGunTower, 2, buildingTowerAttempts, buildingTowerMaxDistance);
                                    }
                                    else if (LevelData.teamStats[controllingTeam.id].plasmatic && LevelData.teamStats[controllingTeam.id].money >= plasmaticTower.value * 2)
                                    {
                                        buildTowersNear(u.transform.position, plasmaticTower, 2, buildingTowerAttempts, buildingTowerMaxDistance);
                                    }
                                    else if (LevelData.teamStats[controllingTeam.id].ant && LevelData.teamStats[controllingTeam.id].money >= antTower.value * 2)
                                    {
                                        buildTowersNear(u.transform.position, antTower, 2, buildingTowerAttempts, buildingTowerMaxDistance);
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    //Send carrybus near?
                    if (LevelData.teamStats[controllingTeam.id].carrybuses > 0)
                    {
                        Vector3 nearPoint = u.transform.position + (new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized * carrybusSendDistance);
                        sendAirForce("0_carrybus1", nearPoint);
                        LevelData.teamStats[controllingTeam.id].carrybuses--;
                    }

                    if (u is UnitFactory) //Watch for unit tier upgrades
                    {
                        UnitFactory t = (UnitFactory) u;
                        for (int j = 0; j < t.productionUnitsIDs.Length; j++)
                        {
                            Unit producingUnit = GlobalList.units[t.productionUnitsIDs[j]];
                            if (!string.IsNullOrWhiteSpace(producingUnit.upgradableToUnit) && LevelData.teamStats[controllingTeam.id].research >= 500)
                            {
                                LevelData.teamStats[controllingTeam.id].RemoveResearch(500);
                                t.productionUnitsIDs[j] = producingUnit.upgradableToUnit;
                                break;
                            }
                        }
                    }
                }
                else if (u.unitType == Unit.UnitType.UNIT || u.unitType == Unit.UnitType.STEPPER)
                {
                    GroundUnit unit = u as GroundUnit;
                    if (!unit.body.hasDestination)
                    {
                        int attempts = 1;
                        Vector3 center = u.gameObject.transform.position;
                        int foundColliders = Physics.OverlapSphereNonAlloc(center, unitTargetFindingRange, unitColliders, 1 << 14);
                        Unit closestUnit = null;
                        Unit closestEnemyUnit = null;
                        float closestDistance = unitTargetFindingRange; //Must always be lower than closestEnemyDistance
                        float closestEnemyDistance = unitTargetFindingRange;

                        for (int j = 0; j < foundColliders; j++)
                        {
                            float distance = Vector3.Distance(center, unitColliders[j].transform.position);
                            if (distance < closestEnemyDistance)
                            {
                                Unit curUnit = Unit.getUnit(unitColliders[j]);

                                if (curUnit == null)
                                    continue;

                                if (curUnit.unitType == Unit.UnitType.BUILDING || curUnit.unitType == Unit.UnitType.TOWER) {
                                    if (distance < closestDistance)
                                    {
                                        closestUnit = curUnit;
                                        closestDistance = distance;
                                    }

                                    if (curUnit.team.id != controllingTeam.id)
                                    {
                                        closestEnemyUnit = curUnit;
                                        closestEnemyDistance = distance;
                                    }
                                }
                            }
                        }

                        if (closestEnemyUnit != null && closestUnit != closestEnemyUnit)
                            unit.commandAttackUnit(closestEnemyUnit);
                    }
                }
            } else //All other enemy units
            {
                //Send airforce?
                float chance = UnityEngine.Random.Range(0f, 1f);
                if (chance <= airForceSendChance)
                {
                    Collider[] unitsInGroup = Physics.OverlapSphere(u.transform.position, airForceSendCheckRange, 1 << 14);

                    if (unitsInGroup.Length > 5)
                    {
                        if (LevelData.teamStats[controllingTeam.id].jets > 0)
                        {
                            sendAirForce("0_jet1", u.transform.position);
                            LevelData.teamStats[controllingTeam.id].jets--;
                        }
                        else if (LevelData.teamStats[controllingTeam.id].destroyers > 0)
                        {
                            sendAirForce("0_destroyer1", u.transform.position);
                            LevelData.teamStats[controllingTeam.id].destroyers--;
                        }
                        else if (LevelData.teamStats[controllingTeam.id].cyclones > 0)
                        {
                            sendAirForce("0_cyclone1", u.transform.position);
                            LevelData.teamStats[controllingTeam.id].cyclones--;
                        }
                        else if (LevelData.teamStats[controllingTeam.id].debrises > 0)
                        {
                            sendAirForce("0_debris1", u.transform.position);
                            LevelData.teamStats[controllingTeam.id].debrises--;
                        }
                    }
                }
            }
        }
    }
}
