using SimpleFileBrowser;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PanelNewLevel : MonoBehaviour
{
    public static int normalMapSize = 128;

    public PanelLevelInfo panelLevelInfo;

    public EditorManager editorManager;
    public BarBuildings barBuildings;
    public BarTaskTriggers barTaskTriggers;

    public void Create()
    {
        //Reset game
        editorManager.ResetGame();

        //Set default nodes
        //for (int i = 0; i < LevelData.teamStats.Length; i++)
        //{
            barTaskTriggers.resetDefaultNodesEveryone();
            //barTaskTriggers.AddNode("0_start_base", LevelData.teamStats[i], 75, -60);
        //}

        //Set default icon again
        panelLevelInfo.setDefaultIcon();

        gameObject.SetActive(false);
    }

    public void No()
    {
        gameObject.SetActive(false);
    }
}
