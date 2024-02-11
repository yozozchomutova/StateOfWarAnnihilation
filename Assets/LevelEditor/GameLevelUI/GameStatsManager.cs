using SOWUtils;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameStatsManager : MonoBehaviour
{
    //Bar sector ended
    [Header("Bar sector")]
    public MainMenuPanel bs;
    public Text bs_title;
    public Text bs_reason;

    //Full stats
    [Header("Full stats")]
    public MainMenuPanel fs;
    public Text fs_title;
    public Text fs_reason;

    public GameObject prefabGameTeamStats;
    
    public Transform teamsScrollViewContent;

    private string winReason;

    public void OnGameEnd(int winnerTeamId, string reason)
    {
        winReason = reason;

        Team winnerTeam = GlobalList.teams[winnerTeamId];
        bs_title.text = "<color=#" + ColorUtility.ToHtmlStringRGB(winnerTeam.minimapColor) + ">" + winnerTeam.name + " team</color> has won.";
        bs_reason.text = reason;
        bs.show();

        StartCoroutine(showFullStats(4f, winnerTeam));
    }

    public void listStats()
    {

    }

    IEnumerator showFullStats(float startDelay, Team winnerTeam)
    {
        yield return new WaitForSeconds(startDelay);

        fs_title.text = "Winner: <color=#" + ColorUtility.ToHtmlStringRGB(winnerTeam.minimapColor) + ">" + winnerTeam.name + "</color>";
        fs_reason.text = winReason;

        bs.OnClose();
        fs.show();

        float contentWidth = 0f;
        TeamStats02_12[] ts = LevelData.teamStats;
        for (int i = 0; i < ts.Length; i++)
        {
            TeamStats02_12 t = ts[i];

            //Skip if team wasn't partipiciating in game
            if (!t.activeTeam)
                continue;

            GameObject go = Instantiate(prefabGameTeamStats, teamsScrollViewContent);
            RectTransform rt = go.GetComponent<RectTransform>();

            rt.anchoredPosition = new Vector2(contentWidth, 0);
            contentWidth += rt.sizeDelta.x;

            Team team = GlobalList.teams[ts[i].teamId];
            Color tc = team.minimapColor;
            go.name = "team_" + team.id;

            //Fill stats
            GO.getRawImage(go, "bcgColor").color = new Color(tc.r, tc.g, tc.b, 0.3f);
            GO.getText(go, "teamName").text = team.name;
            GO.getText(go, "moneyEarned").text = "" + t.moneyEarned;
            GO.getText(go, "moneySpent").text = "" + t.moneySpent;
            GO.getText(go, "researchEarned").text = "" + t.researchEarned;
            GO.getText(go, "researchSpent").text = "" + t.researchSpent;
            GO.getText(go, "buildingsCaptured").text = "" + t.buildingsCaptured;
            GO.getText(go, "buildingsLost").text = "" + t.buildingsLost;
            GO.getText(go, "unitsProduced").text = "" + t.unitsProduced;
            GO.getText(go, "unitsLost").text = "" + t.unitsLost;
            GO.getText(go, "airForcesSent").text = "" + t.airForcesSent;
            GO.getText(go, "airForcesProduced").text = "" + t.airForcesProduced;
            GO.getText(go, "airForcesLost").text = "" + t.airForcesLost;
            GO.getText(go, "towersBuilt").text = "" + t.towersBuilt;
            GO.getText(go, "towersLost").text = "" + t.towersLost;
        }

        RectTransform tsrt = teamsScrollViewContent.GetComponent<RectTransform>();
        tsrt.sizeDelta = new Vector2(contentWidth, tsrt.sizeDelta.y);
    }
}
