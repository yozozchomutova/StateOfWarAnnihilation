using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoading : MonoBehaviour
{
    [Header("UI Root")]
    public MainMenuPanel uiRoot;

    [Header("Checking updated setup side")]
    public Animation setupPanelOUTAnim;
    public RectTransform ss_root;
    public ImageSequence ss_loadingIcon;
    public ImageSequence ss_fineIcon;

    private volatile bool setupIsOkay;
    private volatile bool setupWasChecked; //If setup check test already passed

    [Header("Loading game status UI")]
    public Text lgs_text;
    public Slider lgs_progressBar;

    private IEnumerator gameLoadingProcess;

    void Start()
    {
        gameLoadingProcess = LoadingGameASync();
        StartCoroutine(gameLoadingProcess);
        StartCoroutine(checkSetup());
    }

    private void Update()
    {

    }

    private IEnumerator LoadingGameASync()
    {
        updateLGSUI("init", 0.1f);
        uiRoot.show();

        yield return new WaitForSeconds(2f);

        updateLGSUI("Waiting on setup check to finish", 0.05f);
        while (!setupWasChecked)
            yield return null;

        updateLGSUI("Waiting on user to finish setup", 0.1f);
        while (!setupIsOkay)
            yield return null;

        yield return new WaitForSeconds(1f);

        try
        {
            updateLGSUI("Initializing static list", 0.2f);
            GlobalList.initLoading();
        } catch (Exception e)
        {
            updateLGSUI("Error when loading Global list: " + e.StackTrace, 1f);
            Debug.LogError(e);
            StopCoroutine(gameLoadingProcess);
        }

        yield return new WaitForSeconds(0.2f);

        yield return new WaitForSeconds(1f);

        updateLGSUI("Loading Menu scene", 0.85f);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);

        while (!asyncOperation.isDone)
            yield return null;

        updateLGSUI("Done", 1f);
        uiRoot.OnClose();

        yield return new WaitForSeconds(1f);
    }

    //LGSUI = Loading game status UI
    private void updateLGSUI(string text, float progress)
    {
        lgs_text.text = text + "...";
        lgs_progressBar.value = progress;
    }

    private IEnumerator checkSetup()
    {
        setupIsOkay = true;

        //yield return new WaitForSeconds(2f);

        //setupPanelOUTAnim.Play();

        //yield return new WaitForSeconds(1f);



        yield return new WaitForSeconds(3f);

        //Switch icons
        ss_loadingIcon.gameObject.SetActive(false);
        ss_fineIcon.gameObject.SetActive(true);
        ss_fineIcon.play();

        setupWasChecked = true;
    }

    public void OnQuit()
    {
        Application.Quit(0);
    }

    public void OnPPAccept()
    {
        string agreedPolicyDate = DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToShortTimeString();

        PlayerPrefs.SetString("agreedPolicyDate", agreedPolicyDate);
        PlayerPrefs.SetInt("agreedToPolicy", 1);
        PlayerPrefs.Save();
    }
}
