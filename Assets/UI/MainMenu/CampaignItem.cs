using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampaignItem : MonoBehaviour, ConfirmDialog.Callback
{
    private Campaign campaign;

    public Text nameText;
    public Text typeText;

    void Start()
    {
        
    }

    public void setValues(Campaign campaign)
    {
        //Save local for later use...
        this.campaign = campaign;

        nameText.text = campaign.campaignName;

        if (campaign.type == Campaign.Type.BEGINNER)
        {
            typeText.text = "Beginner campaign";
        }
        else if (campaign.type == Campaign.Type.WORLD_RULER)
        {

        }
        else if (campaign.type == Campaign.Type.CONQUEROR)
        {

        }
    }

    public void OnPlay()
    {

    }

    public void OnRemove()
    {
        GameObject.Find("Canvas").GetComponent<ConfirmDialog>().ShowDialog("? Remove ?", this);
    }

    //INTERFACE !!
    public void OnNo() { }
    public void OnYes()
    {
        CampaignManager campaignManager = GameObject.Find("CampaignManager").GetComponent<CampaignManager>();
        campaignManager.RemoveCampaign(campaign);
    }
}
