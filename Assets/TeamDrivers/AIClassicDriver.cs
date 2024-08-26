using System;
using UnityEngine;
using static MapLevelManager;

public class AIClassicDriver : TeamDriver
{
    private Team focusTeam;
    private Unit focusUnit;

    [Header("Bot settings")]
    public float smfUpgradeMaxDistance;
    public float smfAttackChance;
    public float airForceSendChance;
    public float airForceSendCheckRange;
    public float buildingProtectChance;
    public float buildingProtectCheckRange;
    public int buildingTowerAttempts;
    public float buildingTowerMaxDistance;

    private Unit grenaderTower;
    private Unit machineGunTower;
    private Unit plasmaticTower;
    private Unit antiAircratTower;
    private Unit antTower;

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
        if (focusUnit == null)
        {
            findTargetToFocus();
        } else if (LevelData.teamStats[controllingTeam.id].activeTeam)
        {

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

                            //Was found anything to upgrade?
                            if (unitToUpgrade != null)
                                smf.commandUpgradeUnit(unitToUpgrade);
                            else if (unitToAttack != null)
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
                            unit.commandMove(focusUnit.transform.position.x, focusUnit.transform.position.z);
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
                                print("Called AIRFORCE: " + LevelData.teamStats[controllingTeam.id].destroyers);
                            }
                            else if (LevelData.teamStats[controllingTeam.id].cyclones > 0)
                            {
                                sendAirForce("0_cyclone1", u.transform.position);
                                LevelData.teamStats[controllingTeam.id].cyclones--;
                            }
                        }
                    }
                }
            }
        }
    }

    private void findTargetToFocus()
    {
        Unit foundCC = null,
            foundAtomizer = null,
            foundAnyOther = null;

        //Locate blue units
        for (int i = 0; i < GlobalList.teams.Length; i++)
        {
            int tId = ((i + controllingTeam.id) % GlobalList.teams.Length) + 1;
            if (tId == controllingTeam.id || tId == 0) //Can't target self or neutral
                continue;

            foreach (Unit u in LevelData.units)
            {
                if (u.team.id == tId)
                {
                    focusTeam = GlobalList.teams[u.team.id];

                    if (u.id == "0_commandCenter1")
                    {
                        foundCC = u;
                        goto onFound;
                    } else if (u.id == "0_atomizer1")
                    {
                        foundAtomizer = u;
                    } else
                    {
                        foundAnyOther = u;
                    }
                }
            }
        }

    onFound:
        focusUnit = foundCC != null ? foundCC :
            foundAtomizer != null ? foundAtomizer :
            foundAnyOther != null ? foundAnyOther : null;
    }
}
