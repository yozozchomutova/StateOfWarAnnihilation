#region [Libraries] All
using Discord;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
#endregion

public class DiscordManager : MonoBehaviour
{
    #region [Variables] Instance
    public static DiscordManager current;
    #endregion
    #region [Variables] Readonly
    /// <summary>ID for pairing with discord app</summary>
    private static readonly long DISCORD_APP_ID = 859166462724014141;
    #endregion
    #region [Variables] Static variables
    public static bool userConnected = false;
    private static Discord.ActivityManager activityManager;
    private static Discord.UserManager userManager;
    public static Discord.Activity activity;
    public static Discord.Discord discord;
    #endregion
    #region [Variables] UI
    [Header("UI Profile")]
    public TMP_Text ui_username;
    public RawImage ui_profilePic;
    public GameObject ui_frameSigned;
    public GameObject ui_frameUnSigned;
    public TMP_Text ui_unsignedMsg;
    #endregion

    #region [Functions] Unity's functions
    void Start()
    {
        current = this;
        UpdateDiscordService();
    }

    void Update()
    {
        if (userConnected)
        {
            discord.RunCallbacks();
        }
    }

    private void OnApplicationQuit()
    {
        userConnected = false;
        StopDiscord("end");
    }
    #endregion
    #region [Functions] Start/Stop/Update Discord service
    public void UpdateDiscordService()
    {
        StartDiscord();
    }

    public void StartDiscord()
    {
        if (PlayerPrefs.GetInt("service_discordRP", 0) == 0)
        {
            PushFailure("Disabled in Privacy settings");
            return;
        }

        if (!userConnected)
        {
            try
            {
                if (DISCORD_APP_ID != -1)
                {
                    discord = new Discord.Discord(DISCORD_APP_ID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
                }
                else
                {
                    PushFailure("You're in build mode");
                    return;
                }
            }
            catch (ResultException)
            {
                PushFailure("Discord app is not running");
                return;
            }

            activityManager = discord.GetActivityManager();
            userManager = discord.GetUserManager();

            activity = new Discord.Activity
            {
                State = "",
                Details = "",
            };

            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res == Discord.Result.Ok)
                {
                    UISetProfile(userManager.GetCurrentUser());
                }
                else
                {
                    PushFailure("Error: Update activity");
                    return;
                }
            });

            userConnected = true;
        }
        else
        {
            UISetProfile(userManager.GetCurrentUser());
        }
    }

    public void StopDiscord(string reason)
    {
        if (userConnected)
        {
            userConnected = false;
            discord.Dispose();

            UI_ShowAsNONSigned(reason);
        }
    }

    private void PushFailure( string result)
    {
        userConnected = false;
        UI_ShowAsNONSigned(result);
    }
    #endregion
    #region [Functions] Managing discord activity
    public static void updateActivity()
    {
        if (userConnected)
        {
            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res == Discord.Result.Ok)
                {

                }
            });
        }
    }

    public static void resetActivity()
    {
        activity = new Activity();
        activity.Assets.LargeImage = "icon";
        activity.Assets.LargeText = "SOW: " + Application.version;
        updateActivity();
    }
    #endregion
    #region [Functions] UI profile
    public void UISetProfile(User u)
    {
        ui_username.text = u.Username;
        StartCoroutine(UIDownloadAvatar(u, ui_profilePic));
        UI_ShowAsSigned();
    }

    public IEnumerator UIDownloadAvatar(User u, RawImage component)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture("https://cdn.discordapp.com/avatars/" + u.Id + "/" + u.Avatar + ".png");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
            Debug.Log(request.error);
        else
            component.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }

    public void UI_ShowAsSigned()
    {
        ui_frameSigned.SetActive(true);
        ui_frameUnSigned.SetActive(false);
    }

    public void UI_ShowAsNONSigned(string result)
    {
        ui_frameSigned.SetActive(false);
        ui_frameUnSigned.SetActive(true);

        ui_unsignedMsg.text = result;
    }
    #endregion
}
