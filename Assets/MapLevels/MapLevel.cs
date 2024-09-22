#region [Libraries] All libraries
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static MapLevelManager;
using static MapLevelManager.UnitSerializable;
using static Unit;
#endregion

public class MapLevel
{
    #region [Variables] Unit parent
    /// <summary> Parent that will have all units that will spawn. </summary>
    private static Transform unitParent;
    #endregion

    #region [Functions] Unit parent
    /// <summary> Sets static parent that will have all units that will spawn. </summary>
    public static void setStaticParentTrans(Transform unitParent_)
    {
        unitParent = unitParent_;
    }
    #endregion
    #region [Functions] Spawning units
    /// <summary> Spawns unit on map with data. Default spawnType is NORMAL </summary>
    public static Unit spawnUnit(UnitSerializable sample)
    {
        return spawnUnit(sample, VirtualSpace.NORMAL);
    }

    /// <summary> Spawns unit on map with data. spawnType = How unit will be spawned </summary>
    public static Unit spawnUnit(UnitSerializable sample, VirtualSpace virtualSpace)
    {
        //Create new
        Unit unit = Object.Instantiate(GlobalList.units[sample.getS(KEY_UNIT_ID)].gameObject, unitParent, true).GetComponent<Unit>();
        UnitBody body = null;

        //Create & attach to body & update positions and rotations
        if (unit.hasBodyID())
        {
            body = Object.Instantiate(GlobalList.bodies[unit.bodyId], unitParent, true).GetComponent<UnitBody>();
        }

        //Set data & link
        unit.link(unit, body);
        unit.virtualSpace = virtualSpace;

        if (unit.hasBodyID())
        {
            body.init();
            unit.gameObject.transform.SetParent(body.headMount, false);
            body.warp(sample.getV(KEY_BODY_POSITION));
        }

        unit.init();
        unit.onInit(sample.getV(KEY_BODY_POSITION), sample.getV(KEY_UNIT_ROTATION));
        unit.deserializeUnit(sample);

        if (unit.virtualSpace == VirtualSpace.CONSTRUCTION)
            unit.setMeshRendererMat(GlobalList.matUnitConstructing);
        else
            unit.restoreMeshRendererMat(unit.team);

        LevelData.units.Add(unit);

        if (virtualSpace == VirtualSpace.NORMAL)
        {
            var (originX, originY) = LevelData.gridManager.SamplePosition(unit.transform.position.x, unit.transform.position.z);
            LevelData.gridManager.PlaceUnit(unit, originX, originY, unit.getRoot().eulerAngles.y);
        }
        return unit;
    }
    #endregion
    #region [Functions] Spawn/Destroy Map objects
    public static MapObject PlaceMapObject(MapObject mapObject, Vector3 position, Vector3 rotation)
    {
        var (gridX, gridY) = LevelData.gridManager.SamplePosition(position.x, position.z);

        MapObject newObject = GameObject.Instantiate(mapObject.gameObject).GetComponent<MapObject>();
        newObject.transform.position = position;
        newObject.transform.eulerAngles = rotation;
        LevelData.gridManager.PlaceMapObject(newObject, gridX, gridY, rotation.y);
        LevelData.mapObjects.Add(newObject);
        return newObject;
    }
    #endregion
}
