#region [Libraries] All libraries
using SOWUtils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
#endregion

public class ResourceFactory : BuildingUnit
{
    #region [Variables] UI
    [HideInInspector] public Image progressBarMaxBcg;
    [HideInInspector] public Image progressBar;
    [HideInInspector] public Text unitLvl;
    #endregion

    #region [Functions] Init
    public override void init()
    {
        base.init();

        if (LevelData.scene != LevelData.Scene.GAME)
            return;

        productionCooldown = productionCooldownDefault;
    }
    #endregion
    #region [Functions] frameUpdate()
    public override void frameUpdate()
    {
        productionCooldown -= Time.deltaTime * productionUnlocked;
        if (productionCooldown < 0f)
        {
            productionCooldown += productionCooldownDefault;

            onProductFinish(productionUnitsIDs[0]);
        }

        if (productionUnitsIDs[0] == "0_energy1")
        {
            LevelData.teamStats[team.id].newEnergy += 0.05f * productionUnlocked;
        }
    }
    #endregion
    #region [Functions] On product finish
    public void onProductFinish(string productId)
    {
        if (productId == "0_goldbrick1")
        {
            LevelData.teamStats[team.id].AddMoney(1);
        }
        else if (productId == "0_research1")
        {
            LevelData.teamStats[team.id].AddResearch(1);
        }
    }
    #endregion
    #region [Functions] UI
    public override void onUnitDataUpdate(GameObject additionalProp)
    {
        productionCount = (int)GO.getSlider(additionalProp, "resourceCount").value;
        productionUnlocked = (int)GO.getSlider(additionalProp, "resourceProductionStart").value;

        //Producing 
        productionUnitsIDs[0] = ((string[])additionalProp.GetComponent<ResourceProductionPropUI>().puIDs.Clone())[0];
    }

    public override void onUnitDataGet(GameObject additionalProp)
    {
        ResourceProductionPropUI sf = additionalProp.GetComponent<ResourceProductionPropUI>();
        MainMenuPanel selectUnitPanel = null;

        sf.Init(name, productionCount, productionAllowedUnits, null, selectUnitPanel);

        GO.getSlider(additionalProp, "resourceCount").value = productionCount;
        GO.getSlider(additionalProp, "resourceProductionStart").value = productionUnlocked;

        //Producing units
        for (int i = 0; i < sf.puIDs.Length; i++)
        {
            sf.puIDs[i] = productionUnitsIDs[0];
        }

        //Update UI
        sf.UpdateUI();
    }

    public override void onUnitGameDataGetUpdate(GameObject additionalProp)
    {
        progressBarMaxBcg.fillAmount = (float) productionUnlocked / productionCount;
        progressBar.fillAmount = (1 - productionCooldown / productionCooldownDefault) * progressBarMaxBcg.fillAmount;
        unitLvl.text = "Lvl: " + productionUnlocked + "/" + productionCount;
    }

    public override void onUnitGameDataGetStart(GameObject additionalProp)
    {
        //getRawImage(additionalProp, "bcg3").color = team.minimapColor;
        GO.getRawImage(additionalProp, "bcg_corner").color = team.minimapColor;
        GO.getRawImage(additionalProp, "bcg_corner2").color = team.minimapColor;

        GO.getText(additionalProp, "centerUnitName").text = GlobalList.producingUnits[productionUnitsIDs[0]].puName;
        GO.getRawImage(additionalProp, "centerUnitIcon").texture = GlobalList.producingUnits[productionUnitsIDs[0]].puIcon;

        progressBarMaxBcg = GO.getImage(additionalProp, "centerProgressBarMaxBcg");
        progressBar = GO.getImage(additionalProp, "centerProgressBar");
        unitLvl = GO.getText(additionalProp, "unitLevel");
    }
    #endregion
}