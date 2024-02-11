using SOWUtils;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameAirForces : MonoBehaviour
{
    private Color teamColor;
    private Color disabledColor = new Color(0, 0, 0, 0.25f);

    [Header("UI Element")]
    public GameObject elementUIPrefab;

    private RectTransform[] uiElements;
    private Image[] btnBcg;
    private Text[] keyShortcut;
    private Image[] countBcg;
    private Text[] count;

    private Vector2[] targetedPositions;

    [Header("UI Element positioning")]
    public float baseOffset;
    public float baseSpacing;

    [Header("UI Element hide")]
    public float hideMoveSpeed;
    public float showOffset;
    public float hideOffset;
    public HideUIDirection hideUIDirection;

    public enum HideUIDirection
    {
        LEFT,
        DOWN
    }

    private void Update()
    {
        //Update UI element positions
        for (int i = 0; i < uiElements.Length; i++)
        {
            uiElements[i].anchoredPosition += (targetedPositions[i] - uiElements[i].anchoredPosition) * hideMoveSpeed * Time.deltaTime;
        }
    }

    public void SetupUI()
    {
        teamColor = GlobalList.teams[LevelData.ts.teamId].minimapColor;

        int airForcePUCount = GlobalList.airForecsPU.Count;
        uiElements = new RectTransform[airForcePUCount];
        btnBcg = new Image[airForcePUCount];
        keyShortcut = new Text[airForcePUCount];
        countBcg = new Image[airForcePUCount];
        count = new Text[airForcePUCount];
        targetedPositions = new Vector2[airForcePUCount];

        for (int i = 0; i < airForcePUCount; i++)
        {
            GameObject newPref = Instantiate(elementUIPrefab, gameObject.transform);
            RectTransform uiElement = newPref.GetComponent<RectTransform>();
            uiElement.gameObject.name = "element " + i;
            uiElements[i] = uiElement;

            int finalIndex = i;
            newPref.GetComponent<Button>().onClick.AddListener(delegate { LevelManager.levelManager.prepareAirForce(finalIndex); }); //Listener

            btnBcg[i] = uiElement.gameObject.GetComponent<Image>();

            GO.getRawImage(uiElement.gameObject, "icon").texture = GlobalList.airForecsPU[i].puIcon;

            keyShortcut[i] = GO.getText(uiElement.gameObject, "keyShortcut");
            keyShortcut[i].text = "[" + (i+1) + "]";

            countBcg[i] = GO.getImage(uiElement.gameObject, "countBcg");

            count[i] = GO.getText(uiElement.gameObject, "count");

            targetedPositions[i] = Vector2.zero;
        }
    }

    public void ShowColor(bool disabled)
    {
        for (int i = 0; i < btnBcg.Length; i++)
        {
            btnBcg[i].color = disabled ? disabledColor : teamColor;
            countBcg[i].color = disabled ? disabledColor : teamColor;
        }
    }

    public void UpdateCounst(int[] newCounts)
    {
        for (int i = 0; i < newCounts.Length; i++)
        {
            count[i].text = "" + newCounts[i];
        }

        UpdatePositions(newCounts);
    }

    public void UpdatePositions(int[] newCounts)
    {
        float curOffset = baseOffset;
        if (hideUIDirection == HideUIDirection.DOWN)
        {
            for (int i = 0; i < uiElements.Length; i++)
            {
                if (newCounts[i] == 0) //Hide
                {
                    targetedPositions[i] = new Vector2(curOffset, hideOffset);
                } else //Show
                {
                    targetedPositions[i] = new Vector2(curOffset, showOffset);
                    curOffset += baseSpacing;
                }
            }
        } else if (hideUIDirection == HideUIDirection.LEFT)
        {
            for (int i = 0; i < uiElements.Length; i++)
            {
                if (newCounts[i] == 0) //Hide
                {
                    targetedPositions[i] = new Vector2(hideOffset, curOffset);
                }
                else //Show
                {
                    targetedPositions[i] = new Vector2(showOffset, curOffset);
                    curOffset += baseSpacing;
                }
            }
        }
    }
}
