#region [Libraries] All libraries
using SOWUtils;
using System;
using UnityEngine;
#endregion

public class GroundUnit : AttackingUnit
{
    #region [Variables] VFX Lights
    /// <summary> Lights </summary>
    public GameObject[] lights;
    #endregion

    #region [Functions] Commands
    /// <summary> Move to selected destination </summary>
    public void commandMove(float x, float z)
    {
        if (virtualSpace == VirtualSpace.NORMAL) {
            float calculatedY = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z));
            Vector3 targettedPosition = new Vector3(x, calculatedY, z);
            body.moveTo(targettedPosition);
            state = UnitState.MOVING;
        }
    }
    /// <summary> Move to unit and prepare to attack (sets unit as primary target) </summary>
    public void commandAttackUnit(Unit unit)
    {
        commandMove(unit.transform.localPosition.x, unit.transform.localPosition.z);
        targetUnitPrimary = unit;
        targetUnitForce = null;
        state = UnitState.TARGETING;
    }
    #endregion

    #region [Overriden functions] onInit
    public override void onInit(Vector3 position, Vector3 rotation)
    {
        base.onInit(position, rotation);

        if (LevelData.scene == LevelData.Scene.GAME)
        {
            InvokeRepeating("locateEnemies", UnityEngine.Random.Range(0.1f, 2f), 0.5f);
        }
    }
    #endregion

    #region [Enums] Unit State
    public enum UnitState
    {
        IDLE,
        MOVING,
        MOVING_FROM_GARAGE,
        TARGETING
    }
    #endregion
}