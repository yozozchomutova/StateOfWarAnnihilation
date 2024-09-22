using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditorLevelMenu : MonoBehaviour
{
    //Exit confirm panel
    public MainMenuPanel exitConfirmPanel;
    public Text levelName, levelNameBcg;

    //Loading screen panel
    public LoadingScreenPanel lsPanel;

    private void OnEnable()
    {
        levelName.text = PanelSaveConfirm.savedLevelName;
        levelNameBcg.text = PanelSaveConfirm.savedLevelName;
    }

    public void OnExit()
    {
        exitConfirmPanel.show();
    }

    //Exit confirm panel
    public void OnExitDialogYes()
    {
        lsPanel.EngageLoading("Main Menu", "", "Canvas", 4, 1);
    }

    public void OnExitDialogNo()
    {
        exitConfirmPanel.OnClose();
    }
}
