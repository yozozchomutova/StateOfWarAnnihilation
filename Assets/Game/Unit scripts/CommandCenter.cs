#region [Libraries] All libraries
using SOWUtils;
using static SOWUtils.DataTypeUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
#endregion

public class CommandCenter : BuildingUnit
{
    #region [Variables] Gameplay properties
    /// <summary> Can Command Center produce SMFs? </summary>
    [HideInInspector] public bool SMFcanProduce;
    /// <summary> How lnog does it take to produce SMF? (Used to reset) </summary>
    [HideInInspector] public float SMFproduceTimeDefault;
    #endregion
    #region [Variables] Runtime properties
    /// <summary> SMF that is currently assgined to Command Center </summary>
    [HideInInspector] private SMF curSMF;
    /// <summary> How lnog does it take to produce SMF? (Runtime/Currently) </summary>
    [HideInInspector] public float SMFproduceTime;
    #endregion
    #region [Variables] GameObject assigned variables
    [Header("GameObject assigned variables")]
    /// <summary> Where is SMF being deployed </summary>
    public Transform SMFOutputPoint;
    /// <summary> Effect animation for spawning SMF </summary>
    public Animation animSpawnSMF;
    /// <summary> Effect particles for spawning SMF </summary>
    public ParticleSystem particleSpawnSMF;
    #endregion
    #region [Variables] UI
    [HideInInspector] public GameObject enabledGO;
    [HideInInspector] public Slider smfHpBar;
    [HideInInspector] public Slider makeProgressBar;
    [HideInInspector] public Text makeProgressText;
    [HideInInspector] public Text stateText;
    #endregion

    #region [Functions] init() + frameUpdate()
    public override void init()
    {
        base.init();
        SMFproduceTime = SMFproduceTimeDefault; //Reset
    }

    public override void frameUpdate()
    {
        base.frameUpdate();

        //Notify team, that it stil has any command center
        LevelData.teamStats[team.id].commandCenterPenalty = 0;

        if (team.id == 0) //Neutral command centers will always respawn atleast once
            respawnOnDeath = true;

        //SMF production
        if (curSMF == null && SMFcanProduce)
        {
            SMFproduceTime -= Time.deltaTime;
            if (SMFproduceTime < 0)
            {
                SMFproduceTime = SMFproduceTimeDefault; //Reset

                animSpawnSMF.Play("0_commandCenter1_spawnSMF");
                particleSpawnSMF.Play();

                curSMF = MapLevel.spawnUnit(new MapLevelManager.UnitSerializable("0_smf1", 1f, team.id, Vector3.zero, SMFOutputPoint.position, Vector3.zero)) as SMF;
            }
        }
    }
    #endregion

    #region [Overriden functions] onLoad
    public override void onLoad()
    {
        base.onLoad();
        assignGameplayProperties(this, typeof(CommandCenter), "UnitData/CommandCenterData");
    }
    #endregion
    #region [Overriden functions] UI
    public override void onUnitDataUpdate(GameObject additionalProp)
    {
        SMFcanProduce = GO.getToggle(additionalProp, "canProducePvt").isOn;
    }

    public override void onUnitDataGet(GameObject additionalProp)
    {
        GO.getToggle(additionalProp, "canProducePvt").isOn = SMFcanProduce;
    }

    public override void onUnitGameDataGetUpdate(GameObject additionalProp)
    {
        if (SMFcanProduce)
        {
            smfHpBar.value = curSMF == null ? 0 : curSMF.getHpNormalized();
            makeProgressBar.value = curSMF == null ? 1 - SMFproduceTime / SMFproduceTimeDefault : 1;
            makeProgressText.text = curSMF == null ? "CRAFTING" : "IN ACTION";
            stateText.text = curSMF == null ? "State: CRAFTING" : "State: " + curSMF.state.ToString();
        }
    }

    public override void onUnitGameDataGetStart(GameObject additionalProp)
    {
        if (SMFcanProduce)
        {
            enabledGO = GO.getGameObject(additionalProp, "enabled");
            enabledGO.SetActive(true);

            //Parameters
            smfHpBar = GO.getSlider(enabledGO, "smfHpBar");
            makeProgressBar = GO.getSlider(enabledGO, "makeProgressBar");
            makeProgressText = GO.getText(enabledGO, "makeProgressText");
            stateText = GO.getText(enabledGO, "state");
        }
        else
        {
            GameObject disabledGO = GO.getGameObject(additionalProp, "disabled");
            disabledGO.SetActive(true);
        }
    }
    #endregion
    #region [Overriden functions] Serializing/Deserializing
    public override void onUnitSerializing(Dictionary<string, string> data)
    {
        base.onUnitSerializing(data);
        data.Add("canProducePVT", "" + SMFcanProduce);
    }

    public override void onUnitDeserializing(Dictionary<string, string> data)
    {
        base.onUnitDeserializing(data);
        SMFcanProduce = bool.Parse(tryGet(data, "canProducePVT", "" + SMFcanProduce));
    }
    #endregion
}