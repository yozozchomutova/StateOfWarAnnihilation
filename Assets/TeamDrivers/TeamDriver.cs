using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using static MapLevelManager;
using static MapLevelManager.UnitSerializable;

public abstract class TeamDriver : MonoBehaviour
{
    [Header("Basic informations")]
    public string id;
    [HideInInspector ]public string driverCategoryId;

    public string driverName;

    public float updateCooldown;
    private float curUpdateCooldown;

    public Team controllingTeam;

    private void Start()
    {
        resetUpdateCooldown();
        onInit();
    }

    private void Update()
    {
        curUpdateCooldown -= Time.deltaTime;
        if (curUpdateCooldown <= 0)
        {
            resetUpdateCooldown();
            onUpdate();
        }
    }

    private void resetUpdateCooldown()
    {
        curUpdateCooldown = updateCooldown;
    }

    public abstract void onInit();

    public abstract void onUpdate();

    protected void buildTowersNear(Vector3 p, Unit tower, int amount, int buildingTowerAttempts, float buildingTowerMaxDistance)
    {
        for (int i = 0; i < amount; i++)
        {
            for (int a = 0; a < buildingTowerAttempts; a++)
            {
                Vector3 nearP = new Vector3(
                    p.x + UnityEngine.Random.Range(-buildingTowerMaxDistance, buildingTowerMaxDistance),
                    p.y,
                    p.z + UnityEngine.Random.Range(-buildingTowerMaxDistance, buildingTowerMaxDistance));

                //Make sure that points is within map bounds
                float offset = 2;
                nearP.x = Mathf.Clamp(nearP.x, offset, Terrain.activeTerrain.terrainData.size.x - offset);
                nearP.z = Mathf.Clamp(nearP.z, offset, Terrain.activeTerrain.terrainData.size.z - offset);
                nearP = new Vector3(nearP.x, Terrain.activeTerrain.SampleHeight(nearP), nearP.z);

                if (!Physics.CheckBox(nearP + GlobalList.bodies[tower.bodyId].collider.center, GlobalList.bodies[tower.bodyId].collider.size, Quaternion.identity, 1 << 14))
                {
                    LevelData.teamStats[controllingTeam.id].RemoveMoney((int)tower.value);//Pay money for unit
                    LevelData.teamStats[controllingTeam.id].towersBuilt++;

                    UnitSerializable towerData = tower.serializeUnit();
                    towerData.setF(KEY_UNIT_HP, 1f);
                    towerData.setI(KEY_UNIT_TEAMID, controllingTeam.id);

                    Unit newTower = MapLevel.spawnUnit(towerData) as Tower;
                    newTower.transform.parent.transform.position = nearP;
                    break;
                    //MapLevel.placeUnit(new UnitLE.UnitLESer(1f, controllingTeam.id, Vector3Ser.fromV(nearP), Vector3Ser.empty(), Vector3Ser.empty()), tower.unitId, FindObjectOfType<GameLevelLoader>().transform);
                }
            }
        }
    }

    protected void sendAirForce(string airForceID, Vector3 p)
    {
        //Make sure that points is within map bounds
        float offset = 2;
        p.x = Mathf.Clamp(p.x, offset, Terrain.activeTerrain.terrainData.size.x - offset);
        p.z = Mathf.Clamp(p.z, offset, Terrain.activeTerrain.terrainData.size.z - offset);

        AirForce referenceAF = GlobalList.units[airForceID] as AirForce;
        UnitSerializable afData = referenceAF.serializeUnit();
        afData.setF(KEY_UNIT_HP, 1f);
        afData.setI(KEY_UNIT_TEAMID, controllingTeam.id);
        AirForce af = MapLevel.spawnUnit(afData) as AirForce;
        af.transform.position = new Vector3(p.x, p.y + referenceAF.heightOffset_end, -40);

        af.commandLaunch(p.x, Terrain.activeTerrain.terrainData.size.x + 55, p.x, p.z, 0.5f);
        LevelData.teamStats[controllingTeam.id].airForcesSent++;
    }
}
