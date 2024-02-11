#region [Libraries] All libraries
using UnityEngine;
using UnityEngine.AI;
#endregion

public class Tower : AttackingUnit
{
    #region [Overriden functions] Can be repaired
    public override bool canBeRepaired(Unit source)
    {
        return hp < hpMax && source is SMF;
    }
    #endregion

    #region [Overriden functions] Can be placed
    public override bool canBePlaced()
    {
        bool neighbourUnitsFactor = Physics.OverlapSphere(transform.position, placementDistance, 1 << 14).Length <= 1;
        bool groundFactor = NavMesh.SamplePosition(getRoot().position, out _, 0.3f, NavMesh.AllAreas);
        bool costFactor = LevelData.scene == LevelData.Scene.GAME ? LevelData.ts.money >= value : true;

        return neighbourUnitsFactor && groundFactor && costFactor;
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
}