using SOWUtils;
using System;
using System.Collections;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class LevelObjective : MonoBehaviour
{
    [HideInInspector] public int teamId;
    [HideInInspector] public TTNode.Ser ser;
    [HideInInspector] public bool isActive = false;
    [HideInInspector] public bool finished = false;

    //UI - Linked
    [HideInInspector] public bool uiActive = false;
    [HideInInspector] private GameObject loUIroot;
    [HideInInspector] public RawImage loRootImg;
    [HideInInspector] public Text loDesc;
    [HideInInspector] public Slider loPB;

    private string fullDescText;

    private int triggeredTimes = 0;
    private Unit startUnit;
    private bool startUnitWasConfigured;

    void Start()
    {
        
    }

    public void HandleUpdate()
    {
        switch (ser.nodeId)
        {
            case BarTaskTriggers.NODE_START_BASE:
            case BarTaskTriggers.NODE_EXTENDER1: 
                triggerNodesAll();
                break;
            case BarTaskTriggers.NODE_TIME_COUNTDOWN:
                float waitSeconds = BitConverter.ToInt32(ser.data, 0);
                //print(teamId + ". :" + triggerNodeWaitSeconds + " T: " + waitSeconds + " UP: " + (triggerNodeWaitSeconds / waitSeconds));
                updateProgress(1f - triggerNodeWaitSeconds / waitSeconds);
                if (triggerNodeWaitSeconds == 0f)
                    triggerNodesAllDelayed(waitSeconds); //Time - seconds
                break;
            case BarTaskTriggers.NODE_GIVE_RESOURCES:
                LevelData.teamStats[teamId].AddMoney(BitConverter.ToInt32(ser.data, 0)); //Money
                LevelData.teamStats[teamId].AddResearch(BitConverter.ToInt32(ser.data, sizeof(Int32))); //Research
                isActive = false;
                break;
            case BarTaskTriggers.NODE_GIVE_AIR_FORCES:
                LevelData.teamStats[teamId].AddAirForce("0_jet1", BitConverter.ToInt32(ser.data, 0)); //Money
                LevelData.teamStats[teamId].AddAirForce("0_destroyer1", BitConverter.ToInt32(ser.data, sizeof(Int32))); //Research
                LevelData.teamStats[teamId].AddAirForce("0_cyclone1", BitConverter.ToInt32(ser.data, sizeof(Int32)*2)); //Research
                LevelData.teamStats[teamId].AddAirForce("0_carrybus1", BitConverter.ToInt32(ser.data, sizeof(Int32)*3)); //Research
                LevelData.teamStats[teamId].AddAirForce("0_debris1", BitConverter.ToInt32(ser.data, sizeof(Int32)*4)); //Research
                isActive = false;
                break;
            case BarTaskTriggers.NODE_LAST_UNIT_STANDING:
            case BarTaskTriggers.NODE_LAST_CC_STANDING:
                if (LevelData.teamStats[teamId].activeTeam)
                {
                    bool lastCC = true;

                    for (int i = 0; i < LevelData.teamStats.Length; i++)
                    {
                        //print("T: " + i + " |AT: " + LevelData.teamStats[i].activeTeam + " HC: " + LevelData.teamStats[i].hasCommandCenter);
                        if (teamId != 0 && i != teamId && LevelData.teamStats[i].activeTeam)
                        {
                            if (ser.nodeId == BarTaskTriggers.NODE_LAST_CC_STANDING && LevelData.teamStats[i].commandCenterPenalty < 5)
                            {
                                lastCC = false;
                            } else if (ser.nodeId == BarTaskTriggers.NODE_LAST_UNIT_STANDING)
                            {
                                for (int j = 0; j < LevelData.units.Count; j++)
                                {
                                    if (LevelData.units[j].team.id == i)
                                    {
                                        lastCC = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (lastCC)
                    {
                        triggerNodesAll();
                        updateProgress(1f);
                    }
                }
                break;
            case BarTaskTriggers.NODE_WIN_GAME:
                LevelManager.levelManager.endGame(teamId, "By objective");
                break;
            case BarTaskTriggers.NODE_LOSE_GAME:
                LevelManager.levelManager.teamOvertakeUnits(teamId, 0);
                break;
            case BarTaskTriggers.NODE_LAUNCH_AIR_FORCE:
                int afID_int = BitConverter.ToInt32(ser.data, 0); //Air force ID
                float x = BitConverter.ToInt32(ser.data, sizeof(Int32)); //Impact X
                float z = BitConverter.ToInt32(ser.data, sizeof(Int32) * 2); //Impact Z
                float y = LevelData.mainTerrain.SampleHeight(new Vector3(x, 0, z)); //Impact Y

                string afID = afID_int == 0 ? "0_jet1" :
                    afID_int == 1 ? "0_destroyer1" :
                    afID_int == 2 ? "0_cyclone1" :
                    afID_int == 3 ? "0_carrybus1" :
                    afID_int == 4 ? "0_debris1" : "unknown_id"
                    ;
                LevelManager.levelManager.sendAirForce(afID, GlobalList.teams[teamId], new Vector3(x, y, z));
                isActive = false;
                break;
            case BarTaskTriggers.NODE_2IN_AND_GATE:
                if (triggeredTimes == 2)
                {
                    triggerNodesAll();
                }
                updateProgress(triggeredTimes / 2f);
                break;
            case BarTaskTriggers.NODE_IS_UNIT_DEAD:
                if (startUnit == null && !startUnitWasConfigured)
                {
                    int unitID = BitConverter.ToInt32(ser.data, 0); //Unit ID
                    startUnit = LevelData.units[unitID];
                    startUnitWasConfigured = true;
                } else if((startUnit == null || startUnit.hp <= 0) && startUnitWasConfigured)
                {
                    triggerNodesAll();
                } else
                {
                    updateProgress(startUnit.getHpNormalized());
                }
                break;
            case BarTaskTriggers.NODE_UNIT_CHECK_DISTANCE:
                if (startUnit == null && !startUnitWasConfigured)
                {
                    int unitID = BitConverter.ToInt32(ser.data, 0); //Unit ID
                    startUnit = LevelData.units[unitID];
                    startUnitWasConfigured = true;
                }
                else if (startUnit != null)
                {
                    x = BitConverter.ToInt32(ser.data, sizeof(Int32));
                    z = BitConverter.ToInt32(ser.data, sizeof(Int32) * 2);
                    float range = BitConverter.ToInt32(ser.data, sizeof(Int32) * 3);
                    y = LevelData.mainTerrain.SampleHeight(new Vector3(x, 0, z)); //Y

                    if (Vector3.Distance(startUnit.transform.position, new Vector3(x, y, z)) <= range)
                        triggerNodesAll();
                }
                break;
            case BarTaskTriggers.NODE_AUTOPATH_UNIT:
                Unit unit = LevelData.units[BitConverter.ToInt32(ser.data, 0)]; //Unit ID + Unit
                x = BitConverter.ToInt32(ser.data, sizeof(Int32));
                z = BitConverter.ToInt32(ser.data, sizeof(Int32) * 2);
                float maxSpeed = (float) BitConverter.ToDouble(ser.data, sizeof(Int32) * 3);
                y = LevelData.mainTerrain.SampleHeight(new Vector3(x, 0, z)); //Y
                unit.body.setAutopath(new Vector3(x,y,z), maxSpeed);

                isActive = false;
                break;
        }
    }

    public void triggerNode()
    {
        //print(teamId + " enabled: " + ser.nodeId);
        isActive = true;
        triggeredTimes++;
    }

    private volatile float triggerNodeWaitSeconds = 0;
    private void triggerNodesAllDelayed(float waitSeconds)
    {
        triggerNodeWaitSeconds = waitSeconds;
        StartCoroutine(triggerNodesAllDelayedIE());
    }

    private IEnumerator triggerNodesAllDelayedIE()
    {
        for (; triggerNodeWaitSeconds > 0; triggerNodeWaitSeconds -= Time.deltaTime)
            yield return null;
        triggerNodesAll();
    }

    public void triggerNodesAll()
    {
        for (int i = 0; i < ser.linkedInputNodeIDs.Length; i++)
        {
            if (ser.linkedInputNodeIDs[i] == 0)
                continue;
            if (ser.linkedInputNodeIDs[i] != -1)
                GameLevelObjectives.allObjectives[teamId][ser.linkedInputNodeIDs[i] - 1].triggerNode();
        }
        isActive = false; //No more active
        finished = true; //Done!
    }

    public void deserializeNode(TTNode.Ser ser, int teamId)
    {
        this.teamId = teamId;
        this.ser = ser;
    }

    public void linkLO(GameObject root)
    {
        loUIroot = root;
        loRootImg = GO.getRawImage(root, "rootImg");
        loDesc = GO.getText(root, "description");
        loPB = GO.getSlider(root, "progress");

        fullDescText = "";
        generateFullDescText(ref fullDescText);

        for (int i = 0; i < ser.linkedInputNodeIDs.Length; i++)
        {
            GameLevelObjectives.allObjectives[teamId][ser.linkedInputNodeIDs[i]-1].generateFullDescText(ref fullDescText);
        }

        uiActive = true;
    }

    public void generateFullDescText(ref string fullDescText)
    {
        //Generate fullDescText
        switch (ser.nodeId)
        {
            case BarTaskTriggers.NODE_EXTENDER1:
                for (int i = 0; i < ser.linkedInputNodeIDs.Length; i++)
                {
                    if (ser.linkedInputNodeIDs[i] != -1)
                    GameLevelObjectives.allObjectives[teamId][ser.linkedInputNodeIDs[i] - 1].generateFullDescText(ref fullDescText);
                }
                break;
            case BarTaskTriggers.NODE_TIME_COUNTDOWN:
                int timeSeconds = BitConverter.ToInt32(ser.data, 0); //Time - seconds
                fullDescText += "Time countdown! Wait " + timeSeconds + " seconds. Reward:\n";
                break;
            case BarTaskTriggers.NODE_GIVE_RESOURCES:
                int money = BitConverter.ToInt32(ser.data, 0);
                int res = BitConverter.ToInt32(ser.data, sizeof(Int32));
                fullDescText += "Money: " + money + "\nResearch: " + res + "\n";
                break;
            case BarTaskTriggers.NODE_GIVE_AIR_FORCES:
                int jet = BitConverter.ToInt32(ser.data, 0);
                int des = BitConverter.ToInt32(ser.data, sizeof(Int32));
                int cyc = BitConverter.ToInt32(ser.data, sizeof(Int32)*2);
                int car = BitConverter.ToInt32(ser.data, sizeof(Int32)*3);
                int deb = BitConverter.ToInt32(ser.data, sizeof(Int32)*4);
                fullDescText += "Jets: " + jet + "\nDestroyers: " + des + "\nCyclones: " + cyc + "\nCarrybuses: " + car + "\nDebrises: " + deb + "\n";
                break;
            case BarTaskTriggers.NODE_LAST_CC_STANDING:
                fullDescText += "Be the only team having Command Center. Destroy other teams Command Centers. Reward:\n";
                break;
            case BarTaskTriggers.NODE_LAST_UNIT_STANDING:
                fullDescText += "Be the only team having units. Destroy other teams units (All of them). Reward:\n";
                break;
            case BarTaskTriggers.NODE_WIN_GAME:
                fullDescText += "Win game!\n";
                break;
            case BarTaskTriggers.NODE_LOSE_GAME:
                fullDescText += "Lose game!\n";
                break;
            case BarTaskTriggers.NODE_LAUNCH_AIR_FORCE:
                int afID_int = BitConverter.ToInt32(ser.data, 0); //Air force ID
                float x = BitConverter.ToInt32(ser.data, sizeof(Int32)); //Impact X
                float z = BitConverter.ToInt32(ser.data, sizeof(Int32) * 2); //Impact Z

                string afID = afID_int == 0 ? "¨Jet" :
                    afID_int == 1 ? "Destroyer" :
                    afID_int == 2 ? "Cyclone" :
                    afID_int == 3 ? "Carrybus" :
                    afID_int == 4 ? "Debris" : "unknown_id"
                    ;

                fullDescText += "Launch an airforce: " + afID + " Position X: " + x + " Position Z: " + z + "\n";
                break;
            case BarTaskTriggers.NODE_UNIT_CHECK_DISTANCE:
                int unitID = BitConverter.ToInt32(ser.data, 0); //Air force ID
                x = BitConverter.ToInt32(ser.data, sizeof(Int32)); //Impact X
                z = BitConverter.ToInt32(ser.data, sizeof(Int32) * 2); //Impact Z
                float range = BitConverter.ToInt32(ser.data, sizeof(Int32) * 3); //Range
                fullDescText += "Unit (ID: " + unitID + ") must reach position: X: " + x + " Z: " + z + " Range: " + range + " |Reward:\n";
                break;
            default:
                fullDescText += "Unknown: " + ser.nodeId + "!\n";
                break;
        }
    }

    public void unlinkFromRoot()
    {
        uiActive = false;
    }

    public void updateUI()
    {
        if (!uiActive)
            return;

        if (loDesc == null) //Reasign, if null (this usually happens when refresh same list)
        {
            linkLO(loUIroot);
        }

        if (finished)
            loRootImg.color = Color.green; 

        loDesc.text = fullDescText;
        //loPB.value = 0f;
    }

    public void updateProgress(float newValue)
    {
        if (uiActive)
            loPB.value = newValue;
    }
}
