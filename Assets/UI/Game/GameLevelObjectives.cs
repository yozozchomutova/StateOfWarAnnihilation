using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevelObjectives : MonoBehaviour
{
    [HideInInspector] public static Dictionary<int, List<LevelObjective>> allObjectives = new Dictionary<int, List<LevelObjective>>(); //Format: teamId, level objectives
    [HideInInspector] public static List<LevelObjective> visibleObjectives = new List<LevelObjective>();
    [HideInInspector] public static List<GameObject> visibleObjectivesUI = new List<GameObject>();

    public LevelObjective prefabLevelObjective;
    public Transform levelObjectivesParent;

    //UI
    [Header("UI")]
    public GameObject prefabLevelObjectiveUI;
    public RectTransform listContent;
    public RectTransform tSelected;

    //Temporary
    private int watchingTeam;

    public void Init()
    {
        watchingTeam = LevelData.ts.teamId;
        allObjectives.Clear();
        visibleObjectives.Clear();
        visibleObjectivesUI.Clear();

        //Auto-Generate objectives from saved nodes
        for (int teamId = 0; teamId < LevelData.teamStats.Length; teamId++)
        {
            TeamStats02_12 ts = LevelData.teamStats[teamId];
            allObjectives.Add(teamId, new List<LevelObjective>());

            for (int i = 0; i < ts.nodesList.Count; i++)
            {
                LevelObjective lo = Instantiate(prefabLevelObjective.gameObject, levelObjectivesParent).GetComponent<LevelObjective>();
                lo.deserializeNode(ts.nodesList[i], teamId);
                allObjectives[teamId].Add(lo);
            }
        }

        //showTeam(watchingTeam);
    }

    void Update()
    {
        for (int i = 0; i < visibleObjectives.Count; i++)
        {
            visibleObjectives[i].updateUI();
        }
    }

    private void refreshObjectiveList()
    {
        //Disassemble UI
        for (int i = 0; i < visibleObjectivesUI.Count; i++)
        {
            //visibleObjectives[i].unlinkFromRoot();
            Destroy(visibleObjectivesUI[i]);
        }

        //Clear
        visibleObjectives.Clear();
        visibleObjectivesUI.Clear();

        //Assign new
        visibleObjectives.AddRange(allObjectives[watchingTeam]);

        //Generate UI
        for (int i = 0; i < visibleObjectives.Count; i++)
        {
            switch (visibleObjectives[i].ser.nodeId)
            {
                case BarTaskTriggers.NODE_TIME_COUNTDOWN:
                case BarTaskTriggers.NODE_LAST_CC_STANDING:
                case BarTaskTriggers.NODE_LAST_UNIT_STANDING:
                case BarTaskTriggers.NODE_LAUNCH_AIR_FORCE:
                case BarTaskTriggers.NODE_UNIT_CHECK_DISTANCE:
                    GameObject uiElement = Instantiate(prefabLevelObjectiveUI, listContent.transform);
                    uiElement.name = watchingTeam + "_" + i + "_ui_element";
                    uiElement.GetComponent<RectTransform>().anchoredPosition = new Vector2(30, -30 - 230 * visibleObjectivesUI.Count);
                    visibleObjectives[i].linkLO(uiElement);
                    visibleObjectivesUI.Add(uiElement);
                    break;
            }
         }

        adaptListConentHeight();
    }

    private void adaptListConentHeight()
    {
        listContent.sizeDelta = new Vector2(listContent.sizeDelta.x, visibleObjectivesUI.Count * 230 + 30);
    }

    public void showTeam(int teamId)
    {
        watchingTeam = teamId;
        refreshObjectiveList();

        //Move tSelected square
        tSelected.anchoredPosition = new Vector2(3 + 50f * (watchingTeam-1), 3);
    }
}
