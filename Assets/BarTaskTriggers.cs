using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BarTaskTriggers : MonoBehaviour
{
    //NODE LIST
    public const string NODE_START_BASE = "0_start_base";
    public const string NODE_TIME_COUNTDOWN = "0_time_countdown";
    public const string NODE_GIVE_RESOURCES = "0_give_resources";
    public const string NODE_GIVE_AIR_FORCES = "0_give_air_forces";
    public const string NODE_LAUNCH_CARRYBUS = "0_launch_carrybus";
    public const string NODE_LAUNCH_AIR_FORCE = "0_launch_air_force";
    public const string NODE_2IN_AND_GATE = "0_2in_and_gate";
    public const string NODE_LAST_CC_STANDING = "0_last_cc_standing";
    public const string NODE_LAST_UNIT_STANDING = "0_last_unit_standing";
    public const string NODE_WIN_GAME = "0_win_game";
    public const string NODE_LOSE_GAME = "0_lose_game";
    public const string NODE_AUTOPATH_UNIT = "0_autopath_unit";
    public const string NODE_UNIT_CHECK_DISTANCE = "0_unit_check_distance";
    public const string NODE_IS_UNIT_DEAD = "0_is_unit_dead";

    public const string NODE_EXTENDER1 = "0_extender1";

    //
    public GameObject selectNodeList;
    public GameObject deleselectCustomUIBcg;
    public Transform nodeEditor;

    [Header("Team paging")]
    public RawImage paging_teamTitleBcg;
    public Text paging_teamTitle;
    public static int teamPage = 0;

    [Header("Linking nodes")]
    public TTNodeLinker linkerPrefab;
    public static List<TTNodeLinker> ttNodeLinkers = new List<TTNodeLinker>();

    public static TTNode linkNodeFirst;
    public static int linkNodeFirstOutputId;

    //tmp
    public static List<TTNode> visibleNodes = new List<TTNode>();

    void Start()
    {
        nextPage(false);
    }

    // Update is called once per frame
    void Update()
    {
        //Handle TTnode linkers
        for (int i = ttNodeLinkers.Count-1; i >= 0; i--)
        {
            if (!ttNodeLinkers[i].checkValidation())
            {
                Destroy(ttNodeLinkers[i].gameObject);
                ttNodeLinkers.RemoveAt(i);
            }
        }

        //Check if node is not null
        for (int i = visibleNodes.Count - 1; i >= 0; i--)
        {
            if (visibleNodes[i] == null)
            {
                visibleNodes.RemoveAt(i);
            }
        }

        //Look for new TTnode linkers
        for (int i = 0; i < visibleNodes.Count; i++)
        {
            TTNode n = visibleNodes[i];
            for (int j = 0; j < n.inputNodes.Length; j++)
            {
                if (n.inputNodes[j] != null && (!ttNodeLinkers.Any(item => item.nodeFrom == n) || !ttNodeLinkers.Any(item => item.nodeTo == n.inputNodes[j])))
                {
                    print("Link generated: J: " + j + " |From: " + n.nodeName + " |To: " + n.inputNodes[j].nodeName + " NJ: " + n.inputNodeIDs[j]);
                    TTNodeLinker newLinker = Instantiate(linkerPrefab.gameObject, nodeEditor).GetComponent<TTNodeLinker>();
                    newLinker.crateLink(n, j, n.inputNodes[j], n.inputNodeIDs[j]);
                    ttNodeLinkers.Add(newLinker);   
                }
            }
        }
    }

    private void OnEnable()
    {
        refreshVisibleNodes(teamPage);
    }

    public void nextPage(bool backwards)
    {
        clearVisibleNodes();
        teamPage += backwards ? -1 : 1;
        teamPage = Mathf.Clamp(teamPage, 0, GlobalList.teams.Length-1);

        TeamStats02_12 ts = LevelData.teamStats[teamPage];
        Team t = GlobalList.teams[teamPage];

        Color c = t.minimapColor;
        paging_teamTitleBcg.color = new Color(c.r, c.g, c.b, 0.5f);
        paging_teamTitle.color = teamPage == 0 ? Color.black : Color.white;
        paging_teamTitle.text = t.name;

        showVisibleNodes(teamPage);
    }

    public void AddNode(string nodeId)
    {
        AddNode(nodeId, LevelData.teamStats[teamPage], 0, 0, 0);
    }

    public TTNode.Ser AddNode(string nodeId, TeamStats02_12 team, int defaultX, int defaultY, int defaultLinkSize)
    {
        int[] linkedInputNodeIDs = new int[defaultLinkSize];
        /*for (int i = 0; i < linkedInputNodeIDs.Length; i++)
        {
            linkedInputNodeIDs[i] = -1;
        }*/
        int[] linkedInputPropertyIDs = new int[defaultLinkSize];
        /*for (int i = 0; i < linkedInputNodeIDs.Length; i++)
        {
            linkedInputPropertyIDs[i] = -1;
        }*/

        TTNode.Ser s = new TTNode.Ser(nodeId, defaultX, defaultY, null, linkedInputNodeIDs, linkedInputPropertyIDs);
        team.nodesList.Add(s);

        if (teamPage == team.teamId)
        {
            TTNode node = Instantiate(GlobalList.eventNodes[nodeId], nodeEditor).GetComponent<TTNode>();
            node.deserializeNode(s, GlobalList.teams[team.teamId]);
            visibleNodes.Add(node);
            node.deserializeNode2();
        }

        deselectCustomUI();
        return s;
    }

    public void AddNodeExtender1()
    {
        AddNode(NODE_EXTENDER1, LevelData.teamStats[teamPage], 0, 0, 3);
    }

    public void showAddNodeMenu()
    {
        selectNodeList.SetActive(true);
        deleselectCustomUIBcg.SetActive(true);
    }

    public void deselectCustomUI()
    {
        selectNodeList.SetActive(false);
        deleselectCustomUIBcg.SetActive(false);
    }

    public void showVisibleNodes(int teamPage)
    {
        TeamStats02_12 ts = LevelData.teamStats[teamPage];
        for (int i = 0; i < ts.nodesList.Count; i++)
        {
            TTNode.Ser ns = ts.nodesList[i];
            TTNode node = Instantiate(GlobalList.eventNodes [ns.nodeId], nodeEditor).GetComponent<TTNode>();
            node.deserializeNode(ns, GlobalList.teams[ts.teamId]);
            visibleNodes.Add(node);
        }
        foreach (TTNode node in visibleNodes)
        {
            node.deserializeNode2();
        }
    }

    public void clearVisibleNodes()
    {
        clearVisibleNodes(teamPage);
    }
    public void clearVisibleNodes(int teamid)
    {
        TeamStats02_12 t = LevelData.teamStats[teamid];
        t.nodesList.Clear();

        foreach (TTNode node in visibleNodes)
        {
            t.nodesList.Add(node.serializeNode());
            Destroy(node.gameObject);
        }
        visibleNodes.Clear();

        foreach (TTNodeLinker l in ttNodeLinkers)
        {
            Destroy(l.gameObject);
        }
        ttNodeLinkers.Clear();
    }

    public void refreshVisibleNodes(int teamid)
    {
        foreach (TTNode node in visibleNodes)
            Destroy(node.gameObject);
        visibleNodes.Clear();

        foreach (TTNodeLinker l in ttNodeLinkers)
            Destroy(l.gameObject);
        ttNodeLinkers.Clear();

        showVisibleNodes(teamid);
    }

    public void resetDefaultNodesEveryone()
    {
        for (int i = 0; i < LevelData.teamStats.Length; i++)
        {
            resetDefaultNodes(i);
        }
    }

    public void resetDefaultNodesCurrent()
    {
        resetDefaultNodes(teamPage);
    }

    public void resetDefaultNodes(int teamId)
    {
        TeamStats02_12 ts = LevelData.teamStats[teamId];
        clearVisibleNodes(teamId);
        ts.nodesList.Clear();

        if (teamId == 0) //NEUTRAL
        {
            AddNode(NODE_START_BASE, ts, 75, -60, 0);
        } else {
            AddNode(NODE_START_BASE, ts, 75, -60, 1).linkedInputNodeIDs[0] = 2;
            AddNode(NODE_EXTENDER1, ts, 310, -90, 3).linkedInputNodeIDs[1] = 3;
            AddNode(NODE_LAST_CC_STANDING, ts, 450, -60, 1).linkedInputNodeIDs[0] = 4;
            AddNode(NODE_WIN_GAME, ts, 800, -60, 0);
        }

        if (teamId == teamPage)
        {
            refreshVisibleNodes(teamId);
        }
    }

    public void Close()
    {
        //Save first
        clearVisibleNodes(teamPage);

        //Then close
        gameObject.SetActive(false);
    }
}
