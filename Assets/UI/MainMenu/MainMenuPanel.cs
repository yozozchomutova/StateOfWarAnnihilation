using UnityEngine;

public class MainMenuPanel : MonoBehaviour
{
    //Alpha
    private CanvasGroup canvasGroup;
    public bool alphaRising;
    private float curAlpha;
    private float curScale = 1.12f;

    void Start()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
    }

    void Update()
    {
        //Scale
        //curScale += (alphaRising ? -Time.deltaTime : Time.deltaTime) * 1.5f;
        curScale += (alphaRising ? -0.6f : 0.6f) * Time.deltaTime;
        curScale = Mathf.Clamp(curScale, 1, 1.12f);
        transform.localScale = new Vector3(curScale, curScale, curScale);

        //Alpha
        //curAlpha += (alphaRising ? Time.deltaTime : -Time.deltaTime) * 8;
        curAlpha += (alphaRising ? 3f : -3f) * Time.deltaTime;

        if (!alphaRising)
        {
            if (curAlpha <= 0f)
                gameObject.SetActive(false);
        }

        curAlpha = Mathf.Clamp(curAlpha, 0, 1f);
        canvasGroup.alpha = curAlpha;
    }

    public void OnClose()
    {
        alphaRising = false;
    }

    public void show()
    {
        gameObject.SetActive(true);
        alphaRising = true;
    }
}
