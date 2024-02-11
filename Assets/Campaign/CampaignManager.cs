using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class CampaignManager : MonoBehaviour
{
    [HideInInspector] public List<Campaign> beginnerCampaigns = new List<Campaign>();
    [HideInInspector] public List<CampaignItem> beginnerCampaignItems = new List<CampaignItem>();

    //UI
    public CampaignItem campaignItemPrefab;

    public RectTransform beginnerCampaignSV;
    public RectTransform worldRulerCampaignSV;
    public RectTransform conquerorCampaignSV;

    void Start()
    {
        //Create SavedLevels folder, if not created already
        Directory.CreateDirectory(Application.persistentDataPath + "/BeginnerCampaign/");

        ReloadCampaignLists();
    }

    public void NewCampaign(string campaignName, Campaign.Type type)
    {
        if (type == Campaign.Type.BEGINNER)
        {
            Campaign campaign = new Campaign(campaignName, Campaign.Type.BEGINNER, "BeginnerCampaign");
            SaveCampaign(campaign);
            ReloadBeginnerCampaigns();
        } else if (type == Campaign.Type.WORLD_RULER)
        {
            ReloadWorldRulerCampaigns();
        } else if (type == Campaign.Type.CONQUEROR)
        {
            ReloadConquerorCampaigns();
        }
    }

    public void SaveCampaign(Campaign campaign)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/BeginnerCampaign/" + campaign.campaignName + ".camp";
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, campaign);
        stream.Close();
    }

    public void RemoveCampaign(Campaign campaign)
    {
        File.Delete(Application.persistentDataPath + "/BeginnerCampaign/" + campaign.campaignName + ".camp");

        if (campaign.type == Campaign.Type.BEGINNER)
            ReloadBeginnerCampaigns();
        else if (campaign.type == Campaign.Type.WORLD_RULER)
            ReloadWorldRulerCampaigns();
        else if (campaign.type == Campaign.Type.CONQUEROR)
            ReloadConquerorCampaigns();
    }

    public void ReloadCampaignLists()
    {
        ReloadBeginnerCampaigns();
        ReloadWorldRulerCampaigns();
        ReloadConquerorCampaigns();
    }

    public void ReloadBeginnerCampaigns()
    {
        //Clear
        for (int i = 0; i < beginnerCampaignItems.Count; i++)
        {
            Destroy(beginnerCampaignItems[i].gameObject);
        }

        beginnerCampaigns.Clear();
        beginnerCampaignItems.Clear();

        //Reload beginner campaigns
        string[] beginnerCampaignFolders = Directory.GetFiles(Application.persistentDataPath + "/BeginnerCampaign/");
        foreach (string cFolder in beginnerCampaignFolders)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(cFolder, FileMode.Open);
            Campaign campaign = formatter.Deserialize(stream) as Campaign;

            //Add to UI
            GameObject campaignItemGO = Instantiate(campaignItemPrefab.gameObject, beginnerCampaignSV);
            CampaignItem campaignItem = campaignItemGO.GetComponent<CampaignItem>();
            campaignItem.setValues(campaign);
            campaignItemGO.GetComponent<RectTransform>().localPosition = new Vector2(7.5f, -7.5f - 117.5f * beginnerCampaigns.Count);

            beginnerCampaigns.Add(campaign);
            beginnerCampaignItems.Add(campaignItem);

            stream.Close();
        }

        beginnerCampaignSV.sizeDelta = new Vector2(beginnerCampaignSV.sizeDelta.x, 7.5f + 117.5f * beginnerCampaignFolders.Length);
    }

    public void ReloadWorldRulerCampaigns()
    {

    }

    public void ReloadConquerorCampaigns()
    {

    }
}
