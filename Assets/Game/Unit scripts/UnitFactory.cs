#region [Libraries] All libraries
using SOWUtils;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static MapLevelManager;
using static UnitLE;
#endregion

public class UnitFactory : BuildingUnit, UnitInFront.RequestUnitUpgrade
{
    #region [Variables] Flag
    [Header("Flag")]
    /// <summary> Enable flag mechanism, when interacting with building.</summary>
    public bool deployFlagAllowed;
    /// <summary> Point where is flag, which tells where units will go after spawning.</summary>
    public GameObject deployFlagPrefab;
    /// <summary> Point where is flag, which tells where units will go after spawning.</summary>
    private GameObject deployFlag;
    /// <summary> Point where is flag, which tells where units will go after spawning.</summary>
    [HideInInspector] public Vector3 flagPoint;
    #endregion
    #region [Variables] Unit construction
    /// <summary> Point, where unit will be deployed.</summary>
    public Transform deployUnitPoint;
    /// <summary> Enable graphics of constructing units?</summary>
    public bool constructingUnitAllowed;
    /// <summary> Unit which is currently in construction.</summary>
    [HideInInspector] public UnitReference constructingUnit;
    #endregion
    #region [Variables] Game UI
    [Header("Game UI")]
    public UnitInFront UI_UIF_Prefab;
    public FactoryPropUI.CallbackType callbackType;

    [HideInInspector] public UnitInFront[] uifs;

    [HideInInspector] public Text centerUnitName;
    [HideInInspector] public RawImage centerUnitIcon;

    [HideInInspector] public Image progressBarMaxBcg;
    [HideInInspector] public Image progressBar;
    [HideInInspector] public Text unitLvl;
    #endregion
    #region [Variables] Rendering - particles
    [Header("Rendering - particles")]
    /// <summary> Particle that plays, when product is finished.</summary>
    [Tooltip("Particle that plays, when product is finished.")]
    public ParticleSystem onFinishParticle;
    #endregion

    #region [Functions] Init
    public override void onInit(Vector3 position, Vector3 rotation)
    {
        base.onInit(position, rotation);

        if (LevelData.scene != LevelData.Scene.GAME)
            return;

        //Default flag position
        Vector3 forwardVector = Quaternion.Euler(rotation) * Vector3.forward;
        Vector3 factoryForwardPoint = position + forwardVector * 5f;
        Vector3 factoryBackwardPoint = position + forwardVector * -5f;
        factoryForwardPoint = new Vector3(factoryForwardPoint.x, LevelData.mainTerrain.SampleHeight(factoryForwardPoint), factoryForwardPoint.z);
        factoryBackwardPoint = new Vector3(factoryBackwardPoint.x, LevelData.mainTerrain.SampleHeight(factoryBackwardPoint), factoryBackwardPoint.z);
        NavMeshHit forwardHit, backwardHit;

        if (NavMesh.SamplePosition(factoryForwardPoint, out forwardHit, 1f, NavMesh.AllAreas))
            flagPoint = forwardHit.position;
        else if (NavMesh.SamplePosition(factoryBackwardPoint, out backwardHit, 1f, NavMesh.AllAreas))
            flagPoint = backwardHit.position;
        else
            flagPoint = position;

        productionCooldown = productionCooldownDefault;
    }
    #endregion
    #region [Functions] frameUpdate()
    public override void frameUpdate()
    {
        if (team.id == Team.WHITE) //No production for neutral team
            return;

        float skipSpeedMultiplier = productionPosition >= productionUnlocked ? 30 : 1;
        productionCooldown -= Time.deltaTime * productionUnlocked * skipSpeedMultiplier * LevelData.teamStats[team.id].lastEnergy;
        if (productionCooldown < 0f)
        {
            productionCooldown = productionCooldownDefault;

            if (productionPosition < productionUnlocked)
                onProductFinish(productionUnitsIDs[productionPosition]);

            productionPosition++;
            if (productionPosition >= productionCount) //Check for bounds
                productionPosition = 0;

            //Move UI circles
            for (int i = 0; i < uifs.Length; i++)
            {
                if (uifs[i] != null)
                    uifs[i].moveNext();
            }
        }

        if (constructingUnitAllowed)
        {
            if (constructingUnit == null)
            {
                string unitID = productionUnitsIDs[productionPosition];
                UnitSerializable newUnit = new UnitSerializable(unitID, 1f, team.id, Vector3.zero, deployUnitPoint.position, deployUnitPoint.localEulerAngles);
                constructingUnit = MapLevel.spawnUnit(newUnit, VirtualSpace.CONSTRUCTION);
            }
            else
            {
                constructingUnit.updateMeshConstructionMat(1 - productionCooldown / productionCooldownDefault);
            }
        }

        //Check if player wants to set flag
        if ((LevelManager.levelManager.selectedUnits.Count > 0 && LevelManager.levelManager.selectedUnits[0] != null && LevelManager.levelManager.selectedUnits[0].gameObject == gameObject) ||
            (LevelManager.levelManager.hoveredBuilding != null && LevelManager.levelManager.hoveredBuilding.gameObject == gameObject) &&
            team.id == LevelData.ts.teamId)
        {
            if (Input.GetKeyDown(KeyCode.R) && deployFlagAllowed)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 300, 1 << 13))
                {
                    flagPoint = hit.point;
                    deployFlag.transform.position = flagPoint;
                }
            }
        }
        else if (deployFlag != null)
        {
            Destroy(deployFlag);
            deployFlag = null;
        }
    }
    #endregion
    #region [Functions] On production finished
    public void onProductFinish(string productId)
    {
        if (GlobalList.units[productId].unitType == UnitType.AIRFORCE)
        {
            if (productId == "0_jet1")
                LevelData.teamStats[team.id].AddAirForce("0_jet1");
            else if (productId == "0_destroyer1")
                LevelData.teamStats[team.id].AddAirForce("0_destroyer1");
            else if (productId == "0_cyclone1")
                LevelData.teamStats[team.id].AddAirForce("0_cyclone1");
            else if (productId == "0_carrybus1")
                LevelData.teamStats[team.id].AddAirForce("0_carrybus1");
            else if (productId == "0_debris1")
                LevelData.teamStats[team.id].AddAirForce("0_debris1");
            else
                Debug.LogError("Unknown ERROR! UNKNOWN AIRFORCE, IN UnitFactory.cs");
        } else
        {
            //Unit newUnit = MapLevel.placeUnit(GlobalList.units[productId], 1f, team, null, placeUnitParent).GetComponent<OldUnit>();
            GroundUnit newUnit = MapLevel.spawnUnit(constructingUnit.unit.serializeUnit()) as GroundUnit;
            newUnit.commandMove(flagPoint.x, flagPoint.z);

            //Record in stats
            LevelData.teamStats[team.id].unitsProduced++;

            //Remove constructing unit
            if (constructingUnit != null)
                constructingUnit.destroyBoth();

            onFinishParticle.Play();
        }
    }
    #endregion
    #region [Functions] Unit validating
    public bool isUnitValid(string id)
    {
        foreach (string au in productionAllowedUnits)
            if (au == id)
                return true;
        return false;
    }
    #endregion

    #region [Overriden functions] UI
    public override void onUnitDataUpdate(GameObject additionalProp)
    {
        productionCount = (int)GO.getSlider(additionalProp, "unitCount").value;
        productionUnlocked = (int)GO.getSlider(additionalProp, "unitProductionStart").value;

        //Producing 
        productionUnitsIDs = (string[])additionalProp.GetComponent<FactoryPropUI>().puIDs.Clone();

        productionCooldown = productionCooldownDefault;
    }

    public override void onUnitDataGet(GameObject additionalProp)
    {
        FactoryPropUI sf = additionalProp.GetComponent<FactoryPropUI>();
        sf.Init(name, productionMax, productionAllowedUnits, productionDefaultUnit, callbackType);

        GO.getSlider(additionalProp, "unitCount").value = productionCount;
        GO.getSlider(additionalProp, "unitProductionStart").value = productionUnlocked;

        //Producing units
        sf.puIDs = productionUnitsIDs;

        //Update UI
        sf.UpdateUI();
    }

    public override void onUnitGameDataGetUpdate(GameObject additionalProp)
    {
        if (productionPosition >= productionUnlocked)
        {
            centerUnitName.text = "*Skipping*";
            centerUnitIcon.color = Color.clear;
        }
        else
        {
            centerUnitName.text = GlobalList.producingUnits[productionUnitsIDs[productionPosition]].puName;
            centerUnitIcon.texture = GlobalList.producingUnits[productionUnitsIDs[productionPosition]].puIcon;
            centerUnitIcon.color = Color.white;
        }

        for (int i = 0; i < productionCount; i++)
        {
            uifs[i].updateAvailabilityKnob(i < productionUnlocked);
        }

        progressBar.fillAmount = 1 - productionCooldown / productionCooldownDefault;
        //unitLvl.text = "Lvl: " + unitsUnlocked + "/" + unitCount;
    }

    public override void onUnitGameDataGetStart(GameObject additionalProp)
    {
        GO.getRawImage(additionalProp, "bcg_corner").color = team.minimapColor;
        GO.getRawImage(additionalProp, "bcg_corner2").color = team.minimapColor;

        centerUnitName = GO.getText(additionalProp, "centerUnitName");
        centerUnitIcon = GO.getRawImage(additionalProp, "centerUnitIcon");

        progressBarMaxBcg = GO.getImage(additionalProp, "centerProgressBarBcg");
        progressBar = GO.getImage(additionalProp, "centerProgressBar");

        uifs = new UnitInFront[productionCount];
        for (int i = 0; i < productionCount; i++)
        {
            uifs[i] = Instantiate(UI_UIF_Prefab, centerUnitIcon.transform.parent);

            Unit u = GlobalList.units[productionUnitsIDs[i]];
            uifs[i].link(u, isUnitValid(u.upgradableToUnit) && team.id == LevelData.ts.teamId, i < productionUnlocked, productionCount, i - productionPosition, i, this);
        }

        Vector3 factoryForwardPoint = gameObject.transform.position + gameObject.transform.forward * 5f;
        Vector3 testPoint = new Vector3(factoryForwardPoint.x, LevelData.mainTerrain.SampleHeight(factoryForwardPoint), factoryForwardPoint.z);

        if (LevelData.ts.teamId == team.id && deployFlagAllowed)
        {
            if (deployFlag != null)
            {
                Destroy(deployFlag);
                deployFlag = null;
            }

            deployFlag = Instantiate(deployFlagPrefab, flagPoint, Quaternion.identity);
        }
    }
    #endregion
    #region [Overriden functions] Request upgrade unit
    public void onRequestUpgradeUnit(int positionInArray)
    {
        productionUnitsIDs[positionInArray] = GlobalList.units[productionUnitsIDs[positionInArray]].upgradableToUnit;
    }
    #endregion
    #region [Overriden functions] Check validation
    public bool checkValidation(Unit unit)
    {
        return isUnitValid(unit.upgradableToUnit);
    }
    #endregion
}