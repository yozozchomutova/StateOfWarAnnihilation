using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenPanel : MonoBehaviour
{
    //UI
    public CanvasGroup alphaCG;

    public Text title;
    public Text subTitle;
    public Text tip;

    private float curAlpha;
    private float curScale;

    void Start()
    {
        transform.localScale = new Vector3(curScale, curScale, curScale);
    }

    private void UpdateUI()
    {
        curAlpha = Mathf.Clamp(curAlpha, 0, 1);
        curScale = Mathf.Clamp(curScale, 1.0f, 1.1f);

        alphaCG.alpha = curAlpha;
        transform.localScale = new Vector3(curScale, curScale, curScale);
    }

    public void EngageLoading(string titleText, string subTitleText, string canvasName, int unloadScene, int loadScene)
    {
        title.text = titleText;
        subTitle.text = subTitleText;
        tip.text = "White Command Centers are obtainable...";

        gameObject.SetActive(true);

        StartCoroutine(StartLoading(canvasName, unloadScene, loadScene, 1f));
    }

    IEnumerator StartLoading(string canvasName, int unloadScene, int loadScene, float delay)
    {
        //1st part -> showing
        curAlpha = 0f;
        curScale = 1.1f;

        UpdateUI();

        while (curAlpha != 1)
        {
            curAlpha += Time.deltaTime * 2f;
            curScale -= Time.deltaTime / 5f;
            UpdateUI();
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(delay);

        //Loading, scene switching
        AsyncOperation lsg = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive); //Loading screen Gate
        while (!lsg.isDone)
            yield return null;

        transform.SetParent(GameObject.Find("LSG_mainCanvas").transform); //Retach from main scene to gate

        AsyncOperation mainMenuUnloader = SceneManager.UnloadSceneAsync(unloadScene); //Unload old scene
        while (!mainMenuUnloader.isDone)
            yield return null;

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(loadScene, LoadSceneMode.Additive); //Load new scene
        while (!asyncOperation.isDone)
            yield return null;

        transform.SetParent(GameObject.Find(canvasName).transform, false); //Retach from gate to scene

        lsg = SceneManager.UnloadSceneAsync(2); //Unload loading screen gate
        while (!lsg.isDone)
            yield return null;

        //2nd part -> hiding
        curAlpha = 1f;
        curScale = 1.0f;

        while (curAlpha != 0)
        {
            curAlpha -= Time.deltaTime * 2f;
            curScale += Time.deltaTime / 5f;
            UpdateUI();
            yield return new WaitForEndOfFrame();
        }

        gameObject.SetActive(false);
        yield return new WaitForSeconds(delay);
    }
}