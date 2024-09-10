using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLevelMenu : MonoBehaviour
{
    private MainMenuPanel mainMenuPanel;

    //Exit confirm panel
    public MainMenuPanel exitConfirmPanel;
    public Text exitDialog_title, exitDialog_descriptor;

    //Loading screen panel
    public LoadingScreenPanel lsPanel;

    // Start is called before the first frame update
    void Start()
    {
        mainMenuPanel = gameObject.GetComponent<MainMenuPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnResume()
    {
        mainMenuPanel.OnClose();
    }

    public void OnSave()
    {

    }

    public void OnSettings()
    {

    }

    public void OnExit()
    {
        exitDialog_title.text = "Are you sure to Exit?";
        exitDialog_descriptor.text = "All progress will be pernamently removed...";

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
