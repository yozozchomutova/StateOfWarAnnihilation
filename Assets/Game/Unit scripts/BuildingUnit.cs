#region [Libraries] All libraries
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static SOWUtils.DataTypeUtils;
#endregion

public abstract class BuildingUnit : Unit
{
    #region [Variables] Gameplay properties
    /// <summary> What is default unit for production </summary>
    [HideInInspector] public string productionDefaultUnit;
    /// <summary> What is maximum limit of producing elements at one building </summary>
    [HideInInspector] public int productionMax;
    /// <summary> How long does it take to make 1 unit of producing element (does not change, It's used for reset) </summary>
    [HideInInspector] public float productionCooldownDefault;
    /// <summary> Which unit ID's are allowed to be in productionUnitIDs </summary>
    [HideInInspector] public string[] productionAllowedUnits;
    #endregion
    #region [Variables] Data set by users
    /// <summary> How much of the producing elements is available (unlocked + non-unlocked) </summary>
    [HideInInspector] public int productionCount;
    /// <summary> How much of the producing elements is unlocked right now (More can be unlocked via SMF) </summary>
    [HideInInspector] public int productionUnlocked;
    /// <summary> Which units are in production line </summary>
    [HideInInspector] public string[] productionUnitsIDs;
    /// <summary> How long does it take to make 1 unit of producing element </summary>
    [HideInInspector] public float productionCooldown;
    #endregion
    #region [Variables] Runtime properties
    /// <summary> What is maximum limit of producing elements at one building </summary>
    [HideInInspector] public int productionPosition;
    #endregion

    #region [Functions] Init & frameUpdate
    public override void init()
    {
        base.init();
    }

    public override void frameUpdate()
    {

    }
    #endregion
    #region [Functions] Repairing/Upgrading
    public void repair(Unit sourceUnit, float hpAmount)
    {
        hp += hpAmount * Time.deltaTime;

        if (hp >= hpMax)
        {
            //Stop unit from repairing
            sourceUnit.commandStop();
            if (LevelData.tsCmp(team.id))
                GameLevelUIController.logMessage("Building repaired", true, gameObject.transform.position);
        }

        hp = Mathf.Min(hpMax, hp); // Border/Limit
    }

    public void upgrade(Unit sourceUnit, float addUP)
    {
        upgradeProgress += addUP * Time.deltaTime;
        if (upgradeProgress >= 100)
        {
            upgradeProgress = 0;
            commandUpgradeLevel();

            //Stop unit from upgrading
            sourceUnit.commandStop();
            if (LevelData.ts.teamId == team.id)
                GameLevelUIController.logMessage("Building upgraded", true, gameObject.transform.position);
        }
    }

    public override bool canBeUpgraded(Unit source)
    {
        return productionUnlocked < productionMax && source is SMF;
    }

    public override bool canBeRepaired(Unit source)
    {
        return hp < hpMax && source is SMF;
    }
    #endregion

    #region [Overriden functions] onLoad
    public override void onLoad()
    {
        base.onLoad();
        //assignGameplayProperties(this, typeof(BuildingUnit), "UnitData/BuildingData");
        assignGameplayProperties(this, typeof(BuildingUnit), "UnitData/FactoryBuildingData");

        //Default values
        productionCount = 3;
        productionUnlocked = 1;
        productionUnitsIDs = new string[productionMax];
        for (int i = 0; i < productionMax; i++)
            productionUnitsIDs[i] = productionDefaultUnit;
    }
    #endregion
    #region [Overrideable functions] Commands
    /// <summary> Command for upgrading unit to next level </summary>
    public override void commandUpgradeLevel()
    {
        productionUnlocked++;
    }
    #endregion
    #region [Overriden functions] Serializing/Deserializing
    public override void onUnitSerializing(Dictionary<string, string> data)
    {
        base.onUnitSerializing(data);
        data.Add("unitCount", "" + productionCount);
        data.Add("unitsUnlocked", "" + productionUnlocked);
        data.Add("producingUnits", arrayToString(productionUnitsIDs));
    }

    public override void onUnitDeserializing(Dictionary<string, string> data)
    {
        base.onUnitDeserializing(data);
        productionCount = int.Parse(tryGet(data, "unitCount", "" + productionCount));
        productionUnlocked = int.Parse(tryGet(data, "unitsUnlocked", "" + productionUnlocked));
        productionUnitsIDs = stringToArray(tryGet(data, "producingUnits", "" + arrayToString(productionUnitsIDs)));
    }
    #endregion
}