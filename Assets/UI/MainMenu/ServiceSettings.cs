using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServiceSettings : MonoBehaviour
{
    [Header("Services UI")]
    public Slider serviceSOWAAnalytics;
    public Slider serviceDiscordIntegration;

    private DiscordProfile dp;
    private DiscordInitializer di;

    void Start()
    {
        dp = GameObject.Find("DiscordProfile").GetComponent<DiscordProfile>();
        di = GameObject.Find("DiscordManager").GetComponent<DiscordInitializer>();
    }

    private void OnEnable()
    {
        serviceSOWAAnalytics.value = PlayerPrefs.GetInt("service_sowaAnalytics", 1);
        serviceDiscordIntegration.value = PlayerPrefs.GetInt("service_discordRP", 1);
    }

    public void saveServiceSettings()
    {
        PlayerPrefs.SetInt("service_sowaAnalytics", (int) serviceSOWAAnalytics.value);
        PlayerPrefs.SetInt("service_discordRP", (int) serviceDiscordIntegration.value);
        PlayerPrefs.Save();
    }

    public void applyServiceSettings()
    {
        saveServiceSettings();

        if (PlayerPrefs.GetInt("service_discordRP", 0) == 1)
        {
            di.StartDiscord(dp);
        }
        else
        {
            di.StopDiscord(dp, "Forbiden");
        }
    }
}
