using UnityEngine;
using UnityEngine.UI;

public class BeginnerCampaignPanel : MonoBehaviour
{
    //Add panel
    public MainMenuPanel newBeginnerCampaign;

    //UI
    public InputField campaignNameIF;

    public void OnAdd()
    {
        newBeginnerCampaign.show();
    }

    public void OnAddCampaign()
    {
        CampaignManager campaignManager = GameObject.Find("CampaignManager").GetComponent<CampaignManager>();
        campaignManager.NewCampaign(campaignNameIF.text, Campaign.Type.BEGINNER);

        newBeginnerCampaign.OnClose();
    }
}
