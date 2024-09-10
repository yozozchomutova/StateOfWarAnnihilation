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
    public override bool canBePlaced(LevelData.Scene scene)
    {
        bool gridFreeSpace = base.canBePlaced(scene);

        if (scene == LevelData.Scene.GAME)
        {
            bool costFactor = LevelData.scene == LevelData.Scene.GAME ? LevelData.ts.money >= value : true;
            bool teamUnitNear = false;

            Collider[] units = Physics.OverlapSphere(getRoot().position, 5, 1 << 14);
            foreach (Collider c in units)
            {
                Unit u = c.GetComponent<Unit>();
                if (u.team.id == LevelData.ts.teamId)
                {
                    if (u.unitType == UnitType.BUILDING || u.unitType == UnitType.TOWER)
                    {
                        if (u != this)
                        {
                            teamUnitNear = true;
                            break;
                        }
                    }
                }
            }

            return gridFreeSpace && costFactor && teamUnitNear;
        }

        return gridFreeSpace;
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