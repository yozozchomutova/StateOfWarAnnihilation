using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditorSubmenu : MonoBehaviour
{
    public LoadingScreenPanel lsPanel;

    void Start()
    {
        
    }

    public void OnLevelEditor()
    {
        lsPanel.EngageLoading("Level editor", "State Of War Level editor", "TIP: No tips today ...", startLoading(), 1f, 1f);
    }

    IEnumerator startLoading()
    {
        AsyncOperation lsg = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive); //Loading screen Gate

        while (!lsg.isDone)
        {
            yield return null;
        }

        lsPanel.transform.SetParent(GameObject.Find("LSG_mainCanvas").transform); //Retach from main scene to gate
        AsyncOperation mainMenuUnloader = SceneManager.UnloadSceneAsync(1); //Unload main menu

        while (!mainMenuUnloader.isDone)
        {
            yield return null;
        }

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(3, LoadSceneMode.Additive);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        lsPanel.transform.SetParent(GameObject.Find("EditorCanvas").transform); //Retach from gate to scene
        lsg = SceneManager.UnloadSceneAsync(2); //Unload loading screen gate

        while (!lsg.isDone)
        {
            yield return null;
        }

        //Finish
        lsPanel.FinishLoading();
    }
}
