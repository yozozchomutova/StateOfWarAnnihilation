using UnityEngine;

public class SingleplayerSubmenu : MonoBehaviour
{
    public MainMenuPanel begginerCampaignPanel;
    public MainMenuPanel worldRulerCampaignPanel;
    public MainMenuPanel conquerorCampaignPanel;
    public MainMenuPanel loadLevelPanel;

    void Start()
    {
        
    }

    public void OnBeginnerCampaign()
    {
        begginerCampaignPanel.show();
    }

    public void OnWorldRulerCampaign()
    {
        worldRulerCampaignPanel.show();
    }

    public void OnConquerorCampaign()
    {
        conquerorCampaignPanel.show();
    }

    public void OnLoadLevel()
    {
        loadLevelPanel.show();
    }
}
