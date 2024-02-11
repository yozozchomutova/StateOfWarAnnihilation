using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayerList : MonoBehaviour
{
    private SingleplayerTeamSettings[] stsFullList;
    public RectTransform stsScrollViewContent;
    public SingleplayerTeamSettings stsPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        //Clear STS list
        if (stsFullList != null)
            for (int i = 0; i < stsFullList.Length; i++)
                if (stsFullList[i] != null)
                    Destroy(stsFullList[i].gameObject);
        stsFullList = new SingleplayerTeamSettings[GlobalList.teams.Length];

        //Generate new STS list
        float offsetY = 0;
        for (int i = 0; i < GlobalList.teams.Length; i++)
        {
            SingleplayerTeamSettings sts = Instantiate(stsPrefab.gameObject, stsScrollViewContent).GetComponent<SingleplayerTeamSettings>();
            sts.GetComponent<RectTransform>().anchoredPosition = new Vector2(5, offsetY);
            sts.init(i, 0);
            sts.forceSelectDriveCategoryID(LoadLevelPanel.teamPlaySettings[i].teamDriverCategoryId);
            sts.switchToInGameMode();
            stsFullList[i] = sts;
            offsetY -= 100;
        }
        stsScrollViewContent.sizeDelta = new Vector2(stsScrollViewContent.sizeDelta.x-10, -offsetY);
    }
}
