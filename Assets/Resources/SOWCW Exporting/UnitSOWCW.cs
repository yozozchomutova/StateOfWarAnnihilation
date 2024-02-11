using UnityEngine;

public class UnitSOWCW
{
    public int unitId;

    public int productionsAvailableFromStart;
    public int[] productionUnits = new int[5] { 0, 0, 0, 0, 0 };
    public int[] productionUnitUpgradeItems = new int[5] { 0, 0, 0, 0, 0 };
    public int teamId;
    public bool hasSateliteProtection = false;
    public float HPpercentage;
    public int xTile, yTile;
    public int x, y;
}
