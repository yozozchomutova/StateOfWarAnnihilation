using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenPanel : MonoBehaviour
{
    public Text title;
    public Text subTitle;
    public Text tip;

    private float curAlpha = 0f;
    private float curScale = 1.1f;

    private bool changing = false;
    private bool risingByShowing = false;

    //Callback
    private IEnumerator startLoadingCB;

    //Delaying
    private float startDelay;
    private float endDelay;

    //UI
    public CanvasGroup alphaCG;

    void Start()
    {
        transform.localScale = new Vector3(curScale, curScale, curScale);
    }

    void Update()
    {
        if (changing && risingByShowing)
        {
            curAlpha += Time.deltaTime * 2f;
            curScale -= Time.deltaTime / 5f;
            updateUI();

            if (curAlpha == 1f)
            {
                changing = false;
                StartCoroutine(perfomStartDelay());
            }
        } else if (changing && !risingByShowing)
        {
            curAlpha -= Time.deltaTime * 2f;
            curScale += Time.deltaTime / 5f;
            updateUI();

            if (curAlpha == 0f)
            {
                changing = false;
                gameObject.SetActive(false);
            }
        }
    }

    private void updateUI()
    {
        curAlpha = Mathf.Clamp(curAlpha, 0, 1);
        curScale = Mathf.Clamp(curScale, 1.0f, 1.1f);

        alphaCG.alpha = curAlpha;
        transform.localScale = new Vector3(curScale, curScale, curScale);
    }

    public void EngageLoading(string titleText, string subTitleText, string tipText, IEnumerator startLoadingCB, float startDelay, float endDelay)
    {
        title.text = titleText;
        subTitle.text = subTitleText;
        tip.text = tipText;

        updateUI();
        gameObject.SetActive(true);

        this.startLoadingCB = startLoadingCB;

        this.startDelay = startDelay;
        this.endDelay = endDelay;

        changing = true;
        risingByShowing = true;
    }

    public void FinishLoading()
    {
        curAlpha = 1f;
        curScale = 1.0f;
        StartCoroutine(perfomEndDelay());
    }

    IEnumerator perfomStartDelay()
    {
        yield return new WaitForSeconds(startDelay);

        StartCoroutine(startLoadingCB);
    }

    IEnumerator perfomEndDelay()
    {
        yield return new WaitForSeconds(endDelay);

        risingByShowing = false;
        changing = true;
    }
}
