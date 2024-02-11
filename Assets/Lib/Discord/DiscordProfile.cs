using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiscordProfile : MonoBehaviour, DiscordInitializer.DiscordFetchAvatarCallback
{
    public GameObject signedFrame;
    public GameObject unSignedFrame;

    //Signed frame

    //Unsigned frame
    public Text unsignedResult;

    //UI
    public RawImage avatar;
    public Text username;

    //Static values to be used anywhere
    public static string lastUserName;
    public static Texture2D lastAvatar;

    private DiscordInitializer discordInitializer;

    void Start()
    {
        GameObject discordManager = GameObject.Find("DiscordManager");

        if (discordManager == null)
        {
            gameObject.SetActive(false);
        } else
        {
            discordInitializer = discordManager.GetComponent<DiscordInitializer>();

            discordInitializer.StartDiscord(this);

            setDefaultDiscordRPC();
        }
    }

    public void refreshConnection()
    {
        unSignedFrame.SetActive(false);
        discordInitializer.StartDiscord(this);
    }

    public void switchToSignedFrame()
    {
        signedFrame.SetActive(true);
        unSignedFrame.SetActive(false);

        setDefaultDiscordRPC();
    }

    public void switchToNonSignedFrame(string result)
    {
        DiscordProfile.lastUserName = null;
        DiscordProfile.lastAvatar= null;

        signedFrame.SetActive(false);
        unSignedFrame.SetActive(true);

        unsignedResult.text = result;
    }

    public void OnReturn(Texture2D texture)
    {
        lastAvatar = texture;
        avatar.texture = lastAvatar;
    }

    private void setDefaultDiscordRPC()
    {
        DiscordInitializer.resetActivity();
        DiscordInitializer.activity.Details = "In Main menu";
        DiscordInitializer.updateActivity();
    }
}
