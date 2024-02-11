using UnityEngine;
using Discord;
using UnityEngine.Networking;
using System.Collections;

public class DiscordInitializer : MonoBehaviour
{
	private static readonly long DISCORD_APP_ID = 859166462724014141;

	private static Discord.ActivityManager activityManager;
	private static Discord.UserManager userManager;
	public static Discord.Activity activity;

	public static Discord.Discord discord;

	//Fetched profile
	public static User currentUser;

	public static bool callbacksAllowed;

	//If it is in main menu scene


	// Use this for initialization
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		if (callbacksAllowed)
        {
			discord.RunCallbacks();
        }
	}

	public void StartDiscord(DiscordProfile dp)
    {
		if (PlayerPrefs.GetInt("stg_discordRpc", 0) == 0)
        {
			PushFailure(dp, "Disabled in Privacy settings");
			return;
		}

		if (!callbacksAllowed)
		{
			try
			{
				if (DISCORD_APP_ID != -1)
                {
					discord = new Discord.Discord(DISCORD_APP_ID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
                } else
                {
					PushFailure(dp, "You're in build mode");
					return;
                }
            } catch (ResultException)
            {
				PushFailure(dp, "Discord app is not running");
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
					updateDP_UI(dp);
				} else
                {
					PushFailure(dp, "Error: Update activity");
					return;
				}
			});

			callbacksAllowed = true;
		} else
        {
			updateDP_UI(dp);
        }
	}

	public void StopDiscord(DiscordProfile dp, string reason)
    {
		if (callbacksAllowed) {
			callbacksAllowed = false;
			discord.Dispose();

			if (dp != null) { dp.switchToNonSignedFrame(reason); }
		}
	}

	private void PushFailure(DiscordProfile dp, string result)
    {
		if (dp != null) { 
			dp.switchToNonSignedFrame(result);
			callbacksAllowed = false;
		}
	}

    private void OnApplicationQuit()
    {
		StopDiscord(null, "end");
    }

	public static void updateActivity()
    {
		if (callbacksAllowed)
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

	public void updateDP_UI(DiscordProfile dp)
    {
		currentUser = userManager.GetCurrentUser();

		if (dp != null)
		{
			DiscordProfile.lastUserName = currentUser.Username;
			dp.username.text = DiscordProfile.lastUserName;
			StartCoroutine(DownloadAvatar(dp));
			dp.switchToSignedFrame();
		}
	}

	public IEnumerator DownloadAvatar(DiscordFetchAvatarCallback callback)
	{
		UnityWebRequest request = UnityWebRequestTexture.GetTexture("https://cdn.discordapp.com/avatars/" + currentUser.Id + "/" + currentUser.Avatar + ".png");
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
			Debug.Log(request.error);
		else
			callback.OnReturn(((DownloadHandlerTexture)request.downloadHandler).texture);
	}

	public interface DiscordFetchAvatarCallback
    {
		public void OnReturn(Texture2D texture);
    }
}
