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
        lsPanel.EngageLoading("Main Menu", "", "TIP: \"LongArm\" tanks are not aggresive units. Use their range properly.", startLoading(), 0.5f, 0.5f);
    }

    public void OnExitDialogNo()
    {
        exitConfirmPanel.OnClose();
    }

    IEnumerator startLoading()
    {
        AsyncOperation lsg = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive); //Loading screen Gate

        while (!lsg.isDone)
        {
            yield return null;
        }

        lsPanel.transform.SetParent(GameObject.Find("LSG_mainCanvas").transform); //Retach from main scene to gate
        AsyncOperation mainMenuUnloader = SceneManager.UnloadSceneAsync(4); //Unload main menu

        while (!mainMenuUnloader.isDone)
        {
            yield return null;
        }

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        lsPanel.transform.SetParent(GameObject.Find("Canvas").transform); //Retach from gate to scene
        lsg = SceneManager.UnloadSceneAsync(2); //Unload loading screen gate

        while (!lsg.isDone)
        {
            yield return null;
        }

        //Finish
        lsPanel.FinishLoading();
    }
}
